using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BaubleController : MonoBehaviour {


	private bool m_mouseIsOver;
    private bool m_isHightlighted;

    public Material hightlightMaterial;
    private Material m_normalMaterial;

    private struct TrackLink
    {
        public GameObject track;
        public float angle; // direction in degrees that the track leaves this node relative to this node's orientation. -180 < angle < 180.
    }

    private Vector3 m_lastPosition;
    private float m_lastCurvature;

    private LinkedList<TrackLink> m_tracks;

    public bool fixedRotation { set; private get; }
    private float m_reciprocalCurvatureRadius;
    private float m_inputCurvature;
    public float publicCurvature;

    private GameObject m_bufferStop;

    void Awake()
    {
        m_isHightlighted = false;
        m_mouseIsOver = false;
        m_normalMaterial = gameObject.GetComponent<Renderer>().material;

        m_tracks = new LinkedList<TrackLink>();
        m_lastPosition = transform.position;
        m_lastCurvature = 0;
    }

    void Update()
    {
        if (m_mouseIsOver != m_isHightlighted)
        {
            if (m_mouseIsOver)
            {
                gameObject.GetComponent<Renderer>().material = hightlightMaterial;
            }
            else
            {
                gameObject.GetComponent<Renderer>().material = m_normalMaterial;
            }

            m_isHightlighted = m_mouseIsOver;
        }

        m_mouseIsOver = false;

        if (transform.position != m_lastPosition || m_reciprocalCurvatureRadius != m_lastCurvature)
        {
            if (m_bufferStop != null) m_bufferStop.transform.position = transform.position;

            bool trackUpdateFailed = false;
            LinkedListNode<TrackLink> node = m_tracks.First;
            while (node != null)
            {
                TrackLink tl = node.Value;
                TrackSectionShapeController tssc = tl.track.GetComponent<TrackSectionShapeController>();

                tl.angle = RecalculateAngle(tl);
                node.Value = tl;
                tssc.GetOtherBauble(gameObject).GetComponent<BaubleController>().RecalculateAngle(tl.track);

                trackUpdateFailed = !(tssc.ShapeTrack());
                if (trackUpdateFailed) break;

                node = node.Next;
            }

            // Reverting bauble position doesn't do anything as the bauble position is immediately moved back in LateUpdate()
            // Require different behaviour to indicate error
            /*
            if (trackUpdateFailed)
            {
                Debug.Log("Track update failed");
                transform.position = m_lastPosition;
                foreach (TrackLink tl in m_tracks)
                {
                    tl.track.GetComponent<TrackSectionShapeController>().ShapeTrack();
                }
            }
            else
            {
                m_lastPosition = transform.position;
            }
            */
            m_lastPosition = transform.position;
            m_lastCurvature = m_reciprocalCurvatureRadius;
        }

        if (m_tracks.Count == 1)
        {
            float angle = m_tracks.First.Value.angle;
            if (angle < -90 || angle > 90)
            {
                m_reciprocalCurvatureRadius = m_inputCurvature;
                publicCurvature = m_reciprocalCurvatureRadius;
            }
            else
            {
                m_reciprocalCurvatureRadius = -m_inputCurvature;
                publicCurvature = m_reciprocalCurvatureRadius;
            }
        }
    }

    public void OnMouseover()
    {
        m_mouseIsOver = true;
    }

    public GameObject GetLastTrackSection()
    {
        if (m_tracks.Count == 0) return null;

        return m_tracks.Last.Value.track;
    }

    // returns the reciprocal of the radius of curvature.
    // positive reciprocal is a curvature to the right of the bauble
    // negative is a curvature to the left
    public float GetCurvature(GameObject track)
    {
        TrackLink tl = GetTrackLink(track);
        if (tl.angle > 90 || tl.angle < -90)
        {
            //Debug.Log("Angle = " + tl.angle + "; Inverting curvature");
            return m_reciprocalCurvatureRadius;
        }
        //Debug.Log("Angle = " + tl.angle + "; not inverting curvature");
        return -m_reciprocalCurvatureRadius;
    }

    public void AdjustCurvature(float r)
    {
        m_inputCurvature +=r;
        m_reciprocalCurvatureRadius = m_inputCurvature;
        publicCurvature = m_reciprocalCurvatureRadius;
    }

    public void ResetCurvature()
    {
        m_inputCurvature = 0;
        m_reciprocalCurvatureRadius = 0;
        publicCurvature = 0;
    }

    public bool IsStraight()
    {
        return (m_reciprocalCurvatureRadius == 0);
    }

    // returns two arrays both as long as there are many tracks connected to this bauble
    // tracks is the list of tracks
    // positions is the position of baubles at the other end of each track
    public void GetBranchPositions(out Vector3[] positions, out GameObject[] tracks)
    {
        positions = new Vector3[m_tracks.Count];
        tracks = new GameObject[m_tracks.Count];
        int i = 0;
        foreach (TrackLink tl in m_tracks)
        {
            positions[i] = tl.track.GetComponent<TrackSectionShapeController>().GetOtherBauble(gameObject).transform.position;
            tracks[i] = tl.track;
            i++;
        }
    }

    public void RemoveLink(GameObject go)
    {
        //Debug.Log("RemoveLink in " + GetComponent<SaveLoad>().UID + " searching for #" + go.GetComponent<SaveLoad>().UID + " in " + GetLinkCount() + " links");
        if (m_tracks == null) return;

        LinkedListNode<TrackLink> node = m_tracks.First;
        while (node != null)
        {
            //Debug.Log("Found #" + node.Value.track.GetComponent<SaveLoad>().UID);
            if (node.Value.track == go)
            {
                m_tracks.Remove(node);
                //Debug.Log("Link removed; count = " + m_tracks.Count);
                return;
            }
            node = node.Next;
        }

        //Debug.Log("Remove link failed - not found");
    }

    public void AddLink(GameObject go)
    {
        TrackLink tl = new TrackLink();
        tl.track = go;
        tl.angle = RecalculateAngle(tl);

        m_tracks.AddLast(tl);

        /*
        Debug.Log("Linking #" + GetComponent<SaveLoad>().UID + "; angle = " + tl.angle);
        if (m_tracks.Count == 1 && (tl.angle > 90 || tl.angle < -90))
        {
            Debug.Log("Spinning");
            m_reciprocalCurvatureRadius = -m_reciprocalCurvatureRadius;
            curvaturePublic = m_reciprocalCurvatureRadius;
        }
        */
    }
    /*
    public void ReverseCurvature()
    {
        Debug.Log("Spinning");
        m_reciprocalCurvatureRadius = -m_reciprocalCurvatureRadius;
        curvaturePublic = m_reciprocalCurvatureRadius;
    }
    */
    public void RecalculateAngle(GameObject trackSection)
    {
        LinkedListNode<TrackLink> node = m_tracks.First;
        while (node != null)
        {
            if (node.Value.track == trackSection)
            {
                TrackLink tl = node.Value;
                tl.angle = RecalculateAngle(tl);
                node.Value = tl;
                return;
            }
            node = node.Next;
        }
    }

    private float RecalculateAngle(TrackLink trackLink)
    {
        //Debug.Log("Calculating directions...");
        GameObject otherBauble = trackLink.track.GetComponent<TrackSectionShapeController>().GetOtherBauble(gameObject);
        if (otherBauble == null)
        {
            //Debug.Log("otherBauble == null");
            return 0;
        }
        else
        {
            Vector3 otherEnd = otherBauble.transform.position;

            //Debug.Log("Vector to other end: " + (otherEnd - transform.position));

            Vector3 endDirection = (otherEnd - transform.position).normalized;

            float dotProduct = Vector3.Dot(endDirection, transform.right);
            if (dotProduct > 1) dotProduct = 1;
            if (dotProduct < -1) dotProduct = -1;
            //Debug.Log("Dot Product: " + dotProduct);

            float angle = Mathf.Rad2Deg * Mathf.Acos(dotProduct);

            if (Vector3.Dot(endDirection, -transform.forward) < 0)
            {
                //Debug.Log("Inverting angle");
                angle = -angle;
            }

            //Debug.Log("Track angle found to be " + angle);

            return angle;
        }
    }

    public Quaternion GetRotation(GameObject track)
    {
        TrackLink tl = GetTrackLink(track);
        return GetRotation(tl);
    }

    private Quaternion GetRotation(TrackLink tl)
    {
        float angleToRotate = (tl.angle > 90 || tl.angle < -90) ? 180 : 0;
        return transform.rotation * Quaternion.AngleAxis(angleToRotate, transform.up);
    }

    public float GetAngle(GameObject track)
    {
        TrackLink tl = GetTrackLink(track);

        return tl.angle;
    }

    // will be dependent on points but just use first valid connection for now
    public TrackSectionShapeController GetTrack(Quaternion direction)
    {
        LinkedListNode<TrackLink> node = m_tracks.First;
        while (node != null)
        {
            if (Quaternion.Angle(direction, GetRotation(node.Value)) < 90)
            {
                return node.Value.track.GetComponent<TrackSectionShapeController>();
            }

            node = node.Next;
        }

        return null;
    }

    private TrackLink GetTrackLink(GameObject track)
    {
        LinkedListNode<TrackLink> node = m_tracks.First;
        while (node != null)
        {
            if (node.Value.track == track)
            {
                return node.Value;
            }
            node = node.Next;
        }
        
        TrackLink tl = new TrackLink();

        tl.angle = 0;
        tl.track = null;

        return tl;
    }

    public int GetLinkCount()
    {
        /*
        foreach (TrackLink tl in m_tracks)
        {
            Debug.Log("Link found: #" + tl.track.GetComponent<TrackUID>().UID);
        }
        */
        return m_tracks.Count;
    }

    public bool CanRotate()
    {
        if (m_bufferStop != null) return false;
        if (fixedRotation) return false;
        return (m_tracks.Count <= 1);
    }

    public void AddBufferStop(GameObject bufferStop)
    {
        if (m_bufferStop != null)
        {
            Destroy(m_bufferStop);
        }
        m_bufferStop = bufferStop;
    }

    public GameObject GetBufferStop()
    {
        return m_bufferStop;
    }

    public void RemoveBufferStop()
    {
        m_bufferStop = null;
    }

    public Quaternion GetRotationForContinuedTrack()
    {
        if (m_tracks.Count != 1) return transform.rotation;

        float angle = m_tracks.First.Value.angle;
        //Debug.Log("Angle = " + angle);
        if (angle > 90 || angle < -90)
            return transform.rotation;
        else
            return transform.rotation * Quaternion.AngleAxis(180, transform.up);
    }

    /*
    public TrackSectionShapeController GetConnectedTrackSection(GameObject go)
    {
        TrackLink fromTrackLink = GetTrackLink(go);
        bool requiresRotation = (fromTrackLink.angle >= -90 && fromTrackLink.angle <= 90);

        LinkedListNode<TrackLink> node = m_tracks.First;
        while (node != null)
        {
            bool isRotated = (node.Value.angle > 90 || node.Value.angle < -90);
            if ((isRotated && requiresRotation) || !(isRotated || requiresRotation))
            {
                return node.Value.track.GetComponent<TrackSectionShapeController>();
            }
        }

        return null;
    }
    */

    
}
