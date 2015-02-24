using UnityEngine;
using System.Collections.Generic;

public class TrackPlacementTool : MonoBehaviour, ITool
{
    public GameObject trackSectionPrefab;

    private GameObject m_currentTrackSection;
    private TrackSectionShapeController m_shapeController;

    private List<GameObject> m_trackSections;

    private Vector3 m_anchor;

    void Awake()
    {
        m_currentTrackSection = null;
        m_trackSections = new List<GameObject>();
    }

    void Start()
    {
        Control.GetControl().trackPlacer = this;
    }

    public void UpdateWhenSelected()
    {
        // this is the bauble we're over, if any
        TrackSectionBaubleController baubleController = null;

        {
            Vector3 location;
            Collider hit = CameraController.GetMouseHit(out location);
            if (hit != null) baubleController = hit.GetComponent<TrackSectionBaubleController>();
            if (baubleController != null) baubleController.OnMouseover();
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (m_currentTrackSection == null)
            {
                //instantiate a new track section
                Vector3 location;
                if (baubleController == null) // the section should be straight
                {
                    if (CameraController.GetMouseHitTerrainLocation(out location))
                    {
                        InstantiateTrackSection(location);
                    }
                }
                else // the section should start at the bauble's location and curve if necessary
                {
                    InstantiateTrackSection(baubleController.transform.position);
                    m_shapeController.LinkStart(baubleController.gameObject);
                }
                m_anchor = m_currentTrackSection.transform.position;
            }
            else // currentTrackSection != null
            {
                m_shapeController.FinalizeShape();

                //leave the current section where it is
                m_currentTrackSection = null;
                m_shapeController = null;
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            if (m_currentTrackSection == null)
            {
                if (baubleController != null)
                {
                    // select this end of the track for editing
                    m_currentTrackSection = baubleController.transform.parent.gameObject; //baubles must be immediate child of length section
                    m_shapeController = m_currentTrackSection.GetComponent<TrackSectionShapeController>();

                    m_shapeController.SelectBaubleForEditing(baubleController.gameObject);
                    m_anchor = m_currentTrackSection.transform.position;
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
                if (baubleController != null)
                {
                    if (m_shapeController.IsStraight())
                    {
                        m_shapeController.LinkStart(baubleController.gameObject);
                        m_shapeController.SetEndPoint(m_anchor);
                    }
                }
                else // baubleController == null
                {
                    if (m_currentTrackSection.transform.position != m_anchor)
                    {
                        Debug.Log("track section pos: " + m_currentTrackSection.transform.position + " anchor: " + m_anchor);
                        m_currentTrackSection.transform.position = m_anchor;
                        m_shapeController.UnlinkStart();
                    }

                    //stretch it to the mouse pointer
                    Vector3 location;

                    if (CameraController.GetMouseHitTerrainLocation(out location))
                    {
                        m_shapeController.SetEndPoint(location);
                    }
                    else
                    {
                        OnDeselect();
                    }
                }

            }
        }
    }

    public void OnSelect()
    {
        SetTrackSectionBaubleVisibility(true);
    }

    public void OnDeselect()
    {
        if (m_currentTrackSection != null)
        {
            DeleteCurrentTrackSection();
        }

        SetTrackSectionBaubleVisibility(false);
    }

    public void InstantiateTrackSection(IDataObject tsData)
    {
        GameObject trackSection = (GameObject)Instantiate(trackSectionPrefab);
        trackSection.GetComponent<TrackSectionSaveLoad>().LoadFromDataObject(tsData);
        m_trackSections.Add(trackSection);
    }

    private void InstantiateTrackSection(Vector3 location)
    {
        m_currentTrackSection = (GameObject)Instantiate(trackSectionPrefab, location, Quaternion.identity);
        m_trackSections.Add(m_currentTrackSection);
        m_shapeController = m_currentTrackSection.GetComponent<TrackSectionShapeController>();
    }

    private void DeleteCurrentTrackSection() //m_currentTrackSection != null
    {
        m_trackSections.Remove(m_currentTrackSection);
        Destroy(m_currentTrackSection);
        m_currentTrackSection = null;
        m_shapeController = null;
    }

    private void SetTrackSectionBaubleVisibility(bool visible)
    {
        Debug.Log("Setting vis to " + visible);
        foreach (GameObject trackSection in m_trackSections)
        {
            TrackSectionShapeController tssc = trackSection.GetComponent<TrackSectionShapeController>();
            tssc.SetBaubleVisibility(visible);
            
        }
    }
}
