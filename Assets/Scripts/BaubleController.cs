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
        public Quaternion localRotation; // direction that the track leaves this node relative to this node's orientation.
    }

    private LinkedList<TrackLink> m_tracks;

    void Awake()
    {
        m_isHightlighted = false;
        m_mouseIsOver = false;
        m_normalMaterial = gameObject.renderer.material;

        collider.enabled = false;

        m_tracks = new LinkedList<TrackLink>();
    }

    void Update()
    {
        if (m_mouseIsOver != m_isHightlighted)
        {
            if (m_mouseIsOver)
            {
                gameObject.renderer.material = hightlightMaterial;
            }
            else
            {
                gameObject.renderer.material = m_normalMaterial;
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
        //Debug.Log("RemoveLink searching for #" + go.GetComponent<TrackUID>().UID + " in " + GetLinkCount() + " links");
        LinkedListNode<TrackLink> node = m_tracks.First;
        while (node != null)
        {
            //Debug.Log("Found #" + node.Value.track.GetComponent<TrackUID>().UID);
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

        Vector3 otherEnd;
        if (go.transform.position == transform.position)
        {
            otherEnd = go.GetComponent<TrackSectionShapeController>().GetEndPoint();
        }
        else if (go.GetComponent<TrackSectionShapeController>().GetEndPoint() == transform.position)
        {
            otherEnd = go.transform.position;
        }
        else
        {
            Debug.Log("We shouldn't be here!");
            Debug.Log("pos: " + transform.position + " track start: " + go.transform.position + " track end: " + go.GetComponent<TrackSectionShapeController>().GetEndPoint());
            return;
        }

        float angle = Quaternion.Angle(Quaternion.LookRotation(otherEnd), transform.rotation);

        if (angle > 180)
        {
            tl.localRotation = Quaternion.AngleAxis(180, Vector3.up);
        }
        else
        {
            tl.localRotation = Quaternion.identity;
        }

        m_tracks.AddLast(tl);

        //Debug.Log("Link added; count = " + m_tracks.Count);
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
}
