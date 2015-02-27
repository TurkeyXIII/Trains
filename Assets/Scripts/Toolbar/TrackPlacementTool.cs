using UnityEngine;
using System.Collections.Generic;

public class TrackPlacementTool : MonoBehaviour, ITool
{
    public GameObject trackSectionPrefab;
    public GameObject baublePrefab;

    private GameObject m_currentTrackSection;
    private TrackSectionShapeController m_shapeController;

    private List<GameObject> m_trackSections;
    private List<GameObject> m_baubles;

    private GameObject m_baubleAnchor;
    private GameObject m_baubleCursor;

    void Awake()
    {
        m_currentTrackSection = null;
        m_trackSections = new List<GameObject>();

        m_baubles = new List<GameObject>();
    }

    void Start()
    {
        Control.GetControl().trackPlacer = this;
    }

    public void UpdateWhenSelected()
    {
        // this is the bauble we're over, if any
        BaubleController baubleController = null;

        {
            Vector3 location;
            Collider hit = CameraController.GetMouseHit(out location);
            if (hit != null) baubleController = hit.GetComponent<BaubleController>();
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
                        m_baubleAnchor = (GameObject)Instantiate(baublePrefab, location, m_currentTrackSection.transform.rotation);
                        m_baubles.Add(m_baubleAnchor);
                    }
                }
                else // the section should start at the bauble's location and curve if necessary
                {
                    InstantiateTrackSection(baubleController.transform.position);
                    m_baubleAnchor = baubleController.gameObject;
                    m_shapeController.LinkStart(m_baubleAnchor);
                }
            }
            else // currentTrackSection != null
            {
                m_shapeController.FinalizeShape();

                m_baubleAnchor.collider.enabled = true;

                if (m_shapeController.IsStraight())
                {
                    m_shapeController.LinkEnd(m_baubleCursor);
                    m_baubleCursor.collider.enabled = true;
                    m_shapeController.LinkStart(m_baubleAnchor);
                    m_baubleAnchor.collider.enabled = true;
                }
                else
                {
                    m_baubles.Remove(m_baubleCursor);
                    Destroy(m_baubleCursor);
                }
                //leave the current section where it is
                m_currentTrackSection = null;
                m_shapeController = null;
                m_baubleAnchor = null;
                m_baubleCursor = null;
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            if (m_currentTrackSection == null)
            {
                if (baubleController != null)
                {
                    // select this end of the track for editing
                    m_currentTrackSection = baubleController.GetLastTrackSection();
                    Debug.Log("Current track section: " + m_currentTrackSection);
                    m_shapeController = m_currentTrackSection.GetComponent<TrackSectionShapeController>();

                    if (baubleController.GetLinkCount() == 1)
                    {
                        Debug.Log("Found only 1 link; selecting this bauble");
                        m_baubleCursor = baubleController.gameObject;
                        m_baubleCursor.collider.enabled = false;
                    }
                    else
                    {
                        Debug.Log("Found " + baubleController.GetLinkCount() + " links, instantiating new bauble");
                        m_baubleCursor = (GameObject)Instantiate(baublePrefab, baubleController.transform.position, baubleController.transform.rotation);
                        m_baubles.Add(m_baubleCursor);
                    }

                    m_baubleAnchor = m_shapeController.SelectBaubleForEditing(baubleController.gameObject);
                    m_baubleAnchor.collider.enabled = false;
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
                        m_shapeController.Link(baubleController.gameObject);
                    }
                }
                else // baubleController == null
                {
                    /*
                    if (m_currentTrackSection.transform.position != m_baubleAnchor.transform.position)
                    {
                        Debug.Log("track section pos: " + m_currentTrackSection.transform.position + " anchor: " + m_baubleAnchor);
                        m_currentTrackSection.transform.position = m_baubleAnchor.transform.position;
                        m_shapeController.UnlinkStart();
                    }
                    */
                    m_shapeController.UnlinkEnd();

                    //stretch it to the mouse pointer
                    Vector3 location;

                    if (CameraController.GetMouseHitTerrainLocation(out location))
                    {
                        m_shapeController.SetEndPoint(location);
                        m_baubleCursor.transform.position = location;
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

        m_baubleCursor = (GameObject)Instantiate(baublePrefab, location, m_currentTrackSection.transform.rotation);
        m_baubles.Add(m_baubleCursor);
    }

    private void DeleteCurrentTrackSection() //m_currentTrackSection != null
    {
        if (m_baubleAnchor.GetComponent<BaubleController>().GetLinkCount() <= 1)
        {
            m_baubles.Remove(m_baubleAnchor);
            Destroy(m_baubleAnchor);
        }
        m_baubles.Remove(m_baubleCursor);
        Destroy(m_baubleCursor);

        m_trackSections.Remove(m_currentTrackSection);
        Destroy(m_currentTrackSection);
        m_currentTrackSection = null;
        m_shapeController = null;
    }

    private void SetTrackSectionBaubleVisibility(bool visible)
    {
        Debug.Log("Setting vis to " + visible);
        foreach (GameObject bauble in m_baubles)
        {
            bauble.SetActive(visible);
        }
    }
}
