using UnityEngine;
using System.Collections.Generic;

public class TrackPlacementTool : Tool
{
    public float verticalOffset;

    private GameObject m_currentTrackSection;
    private TrackSectionShapeController m_shapeController;

    private List<Collider> m_trackColliders;

    private GameObject m_baubleAnchor;
    private GameObject m_baubleCursor;

    private GameObject m_prefabTrackSection;
    private GameObject m_prefabBufferStop;
    private GameObject m_prefabBauble;

    void Awake()
    {
        m_currentTrackSection = null;

        m_trackColliders = new List<Collider>();
    }

    void Start()
    {
        Control.GetControl().trackPlacer = this;
        m_prefabTrackSection = Control.GetControl().prefabTrackSection;
        m_prefabBufferStop = Control.GetControl().prefabBufferStop;
        m_prefabBauble = Control.GetControl().prefabBauble;
    }

    public override void UpdateWhenSelected()
    {
        // this is the bauble we're over, if any
        BaubleController hoveringBaubleController = null;

        {
            Vector3 location;
            Collider hit = CameraController.GetMouseHit(out location);
            if (hit != null) hoveringBaubleController = hit.GetComponent<BaubleController>();
            if (hoveringBaubleController != null) hoveringBaubleController.OnMouseover();
        }

        switch (Control.GetControl().GetToolSelector().GetEffect())
        {
            case Effect.Track:
                {

                    if (Input.GetMouseButtonDown(0))
                    {
                        if (m_currentTrackSection == null)
                        {
                            //instantiate a new track section
                            Vector3 location;
                            if (hoveringBaubleController == null) // the section should be straight
                            {
                                if (CameraController.GetMouseHitTerrainLocation(out location))
                                {
                                    InstantiateTrackSection(location);
                                    m_baubleAnchor = (GameObject)Instantiate(m_prefabBauble, location, m_currentTrackSection.transform.rotation);
                                    m_baubleAnchor.GetComponent<Collider>().enabled = false;
                                    m_shapeController.LinkStart(m_baubleAnchor);
                                }
                            }
                            else // the section should start at the bauble's location and curve if necessary
                            {
                                InstantiateTrackSection(hoveringBaubleController.transform.position);

                                Debug.Log("section instantiated at " + m_currentTrackSection.transform.position + " from bauble at " + hoveringBaubleController.transform.position);

                                m_baubleAnchor = hoveringBaubleController.gameObject;
                                m_baubleAnchor.GetComponent<Collider>().enabled = false;
                                m_shapeController.LinkStart(m_baubleAnchor);
                            }
                        }
                        else // currentTrackSection != null
                        {
                            m_shapeController.FinalizeShape();
                            BoxCollider[] currentColliders = m_currentTrackSection.GetComponentsInChildren<BoxCollider>();
                            //Debug.Log("Finalized track section has " + currentColliders.Length + " colliders");

                            bool positionIsValid = true;
                            GameObject collidedTrackSection = null;
                            foreach (BoxCollider c in currentColliders)
                            {
                                foreach (BoxCollider d in m_trackColliders)
                                {

                                    if (c.bounds.Intersects(d.bounds))
                                    {
                                        if (BoxCollidersOverlap(c, d))
                                        {
                                            //Debug.Log("Collision found");

                                            if (!TrainsMath.AreApproximatelyEqual(c.transform.position.y, d.transform.position.y))
                                            {
                                                collidedTrackSection = d.transform.parent.gameObject;
                                                positionIsValid = false;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            if (positionIsValid)
                            {
                                foreach (Collider c in currentColliders)
                                    m_trackColliders.Add(c);

                                m_shapeController.SetBallast();

                                m_baubleAnchor.GetComponent<Collider>().enabled = true;
                                m_baubleCursor.GetComponent<Collider>().enabled = true;

                                if (m_baubleCursor.GetComponent<BaubleController>().GetLinkCount() == 0)
                                {
                                    Destroy(m_baubleCursor);
                                }
                                //leave the current section where it is
                                m_currentTrackSection = null;
                                m_shapeController = null;
                                m_baubleAnchor = null;
                                m_baubleCursor = null;
                            }
                            else
                            {
                                m_shapeController.Unfinalize();
                                Highlighter[] highlighters = m_currentTrackSection.GetComponentsInChildren<Highlighter>();
                                foreach (Highlighter h in highlighters)
                                    h.Highlight(1);

                                highlighters = collidedTrackSection.GetComponentsInChildren<Highlighter>();
                                foreach (Highlighter h in highlighters)
                                    h.Highlight(1);
                            }
                        }
                    }
                    else if (Input.GetMouseButtonDown(1))
                    {
                        if (m_currentTrackSection == null)
                        {
                            if (hoveringBaubleController != null)
                            {
                                // select this end of the track for editing
                                m_currentTrackSection = hoveringBaubleController.GetLastTrackSection();
                                Debug.Log("Current track section: #" + m_currentTrackSection.GetComponent<ObjectUID>().UID);
                                m_shapeController = m_currentTrackSection.GetComponent<TrackSectionShapeController>();

                                Collider[] colliders = m_shapeController.GetComponentsInChildren<Collider>();
                                foreach (Collider c in colliders)
                                    m_trackColliders.Remove(c);

                                m_shapeController.Unfinalize();

                                if (hoveringBaubleController.GetLinkCount() == 1)
                                {
                                    Debug.Log("Found only 1 link; selecting this bauble");
                                    m_baubleCursor = hoveringBaubleController.gameObject;

                                }
                                else
                                {
                                    Debug.Log("Found " + hoveringBaubleController.GetLinkCount() + " links, instantiating new bauble");
                                    m_baubleCursor = (GameObject)Instantiate(m_prefabBauble, hoveringBaubleController.transform.position, hoveringBaubleController.transform.rotation);
                                }

                                m_shapeController.SelectBaubleForEditing(hoveringBaubleController.gameObject);
                                m_baubleAnchor = m_shapeController.GetOtherBauble(hoveringBaubleController.gameObject);
                                m_baubleAnchor.GetComponent<Collider>().enabled = false;
                                m_baubleCursor.GetComponent<Collider>().enabled = false;
                            }
                        }
                        else //(m_currentTrackSection != null)
                        {
                            DeleteCurrentTrackSection();
                        }
                    }

                    else // no mouse button down
                    {
                        if (m_currentTrackSection == null)
                        {

                        }
                        else //(m_currentTrackSection != null)
                        {
                            if (hoveringBaubleController != null)
                            {
                                m_shapeController.LinkEnd(hoveringBaubleController);
                                m_baubleCursor.SetActive(false);
                            }
                            else // baubleController == null
                            {
                                m_shapeController.LinkEnd(m_baubleCursor);
                                m_baubleCursor.SetActive(true);

                                //stretch it to the mouse pointer
                                Vector3 location;

                                if (CameraController.GetMouseHitTerrainLocation(out location))
                                {
                                    m_shapeController.SetEndPoint(location);
                                    //m_baubleCursor.transform.position = location;
                                }
                                else
                                {
                                    OnDeselect();
                                }
                            }

                        }
                    }
                    break;
                }
            case Effect.BufferStop:
                {
                    if (Input.GetMouseButtonDown(0) && hoveringBaubleController != null && hoveringBaubleController.GetLinkCount() == 1)
                    {
                        GameObject bufferStop = (GameObject)GameObject.Instantiate(m_prefabBufferStop, hoveringBaubleController.transform.position + verticalOffset * hoveringBaubleController.transform.up, hoveringBaubleController.GetRotationForContinuedTrack());
                        bufferStop.GetComponent<BufferStopController>().Link(hoveringBaubleController.gameObject);
                    }
                    else if (Input.GetMouseButtonDown(1) && hoveringBaubleController != null && hoveringBaubleController.GetBufferStop() != null)
                    {
                        GameObject.Destroy(hoveringBaubleController.GetBufferStop());
                        hoveringBaubleController.RemoveBufferStop();
                    }
                    break;
                }
        }
    }

    public override void OnSelect()
    {
        SetTrackSectionBaubleVisibility(true);
    }

    public override void OnDeselect()
    {
        if (m_currentTrackSection != null)
        {
            DeleteCurrentTrackSection();
        }

        SetTrackSectionBaubleVisibility(false);
    }

    public override void OnEffectChange()
    {
        if (m_currentTrackSection != null)
            DeleteCurrentTrackSection();
    }

    public override Effect GetDefaultEffect()
    {
        return Effect.Track;
    }

    private void InstantiateTrackSection(Vector3 location)
    {
        m_currentTrackSection = (GameObject)Instantiate(m_prefabTrackSection, location, Quaternion.identity);
        m_shapeController = m_currentTrackSection.GetComponent<TrackSectionShapeController>();

        m_baubleCursor = (GameObject)Instantiate(m_prefabBauble, location, m_currentTrackSection.transform.rotation);
        m_baubleCursor.GetComponent<Collider>().enabled = false;
        m_shapeController.LinkEnd(m_baubleCursor);
    }

    private void DeleteCurrentTrackSection() //m_currentTrackSection != null
    {
        if (m_baubleAnchor.GetComponent<BaubleController>().GetLinkCount() <= 1)
        {
            Destroy(m_baubleAnchor);
        }
        else
        {
            m_baubleAnchor.GetComponent<Collider>().enabled = true;
        }
        Destroy(m_baubleCursor);

        Collider[] currentColliders = m_currentTrackSection.GetComponentsInChildren<Collider>();
        foreach (Collider c in currentColliders)
            m_trackColliders.Remove(c);

        Destroy(m_currentTrackSection);
        m_currentTrackSection = null;
        m_shapeController = null;
    }

    private void SetTrackSectionBaubleVisibility(bool visible)
    {
        Debug.Log("Setting vis to " + visible);
        foreach (GameObject bauble in Control.GetControl().GetBaubles())
        {
            bauble.SetActive(visible);
        }
    }

    public void ResetLists()
    {
        m_trackColliders = new List<Collider>();
    }

    private static bool BoxCollidersOverlap(BoxCollider a, BoxCollider b)
    {
        return (BoxCollidersRaycastCheck(a, b) || BoxCollidersRaycastCheck(b, a));
    }

    private static bool BoxCollidersRaycastCheck(BoxCollider a, BoxCollider b)
    {
        Vector3 start = a.transform.TransformPoint(new Vector3(a.center.x - a.size.x / 2, a.center.y + a.size.y / 2, a.center.z - a.size.z / 2));
        Vector3 direction = a.transform.forward;
        float distance = a.size.z * a.transform.lossyScale.z;

//        Debug.Log("Start: " + start + ", direction: " + direction + ", distance: " + distance);

        RaycastHit hit;
        if (b.Raycast(new Ray(start, direction), out hit, distance)) return true;

        start = a.transform.TransformPoint(new Vector3(a.center.x + a.size.x / 2, a.center.y + a.size.y / 2, a.center.z - a.size.z / 2));
//        Debug.Log("Start: " + start + ", direction: " + direction + ", distance: " + distance);
        if (b.Raycast(new Ray(start, direction), out hit, distance)) return true;

        return false;
    }
}
