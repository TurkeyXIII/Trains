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
        public float angle; // direction that the track leaves this node relative to this node's orientation. -180 < angle < 180.
    }

    private LinkedList<TrackLink> m_tracks;

    public bool fixedRotation { set; private get; }

    private GameObject m_bufferStop;

    void Awake()
    {
        m_isHightlighted = false;
        m_mouseIsOver = false;
        m_normalMaterial = gameObject.GetComponent<Renderer>().material;

        m_tracks = new LinkedList<TrackLink>();
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

    public void RemoveLink(GameObject go)
    {
        Debug.Log("RemoveLink in " + GetComponent<SaveLoad>().UID + " searching for #" + go.GetComponent<SaveLoad>().UID + " in " + GetLinkCount() + " links");
        if (m_tracks == null) return;

        LinkedListNode<TrackLink> node = m_tracks.First;
        while (node != null)
        {
            Debug.Log("Found #" + node.Value.track.GetComponent<SaveLoad>().UID);
            if (node.Value.track == go)
            {
                m_tracks.Remove(node);
                Debug.Log("Link removed; count = " + m_tracks.Count);
                return;
            }
            node = node.Next;
        }

        Debug.Log("Remove link failed - not found");
    }

    public void AddLink(GameObject go)
    {
        TrackLink tl = new TrackLink();
        tl.track = go;

        RecalculateDirections(ref tl);
        
        m_tracks.AddLast(tl);

        Debug.Log(GetComponent<SaveLoad>().UID + " added link " + go.GetComponent<SaveLoad>().UID + "; count = " + m_tracks.Count);
    }

    public void RecalculateDirections(GameObject trackSection)
    {
        LinkedListNode<TrackLink> node = m_tracks.First;
        while (node != null)
        {
            if (node.Value.track == trackSection)
            {
                TrackLink tl = node.Value;
                RecalculateDirections(ref tl);
                node.Value = tl;
                return;
            }
            node = node.Next;
        }
    }

    private void RecalculateDirections(ref TrackLink trackLink)
    {
        //Debug.Log("Calculating directions...");
        GameObject otherBauble = trackLink.track.GetComponent<TrackSectionShapeController>().GetOtherBauble(gameObject);
        if (otherBauble == null)
        {
            trackLink.angle = 0;
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

            trackLink.angle = angle;
        }
    }

    public Quaternion GetRotation(GameObject track)
    {
        TrackLink tl = GetTrackLink(track);
        float angleToRotate = (tl.angle > 90 || tl.angle < -90) ? 180 : 0;
        return transform.rotation * Quaternion.AngleAxis(angleToRotate, transform.up);
    }

    public float GetAngle(GameObject track)
    {
        TrackLink tl = GetTrackLink(track);

        return tl.angle;
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
        return m_tracks.Count + (m_bufferStop == null ? 0 : 1) + (fixedRotation ? 2 : 0);
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
}
