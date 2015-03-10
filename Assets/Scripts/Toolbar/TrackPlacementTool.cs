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
        BaubleController hoveringBaubleController = null;

        {
            Vector3 location;
            Collider hit = CameraController.GetMouseHit(out location);
            if (hit != null) hoveringBaubleController = hit.GetComponent<BaubleController>();
            if (hoveringBaubleController != null) hoveringBaubleController.OnMouseover();
        }

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
                        m_baubleAnchor = (GameObject)Instantiate(baublePrefab, location, m_currentTrackSection.transform.rotation);
                        m_baubles.Add(m_baubleAnchor);
                        m_shapeController.LinkStart(m_baubleAnchor);
                    }
                }
                else // the section should start at the bauble's location and curve if necessary
                {
                    InstantiateTrackSection(hoveringBaubleController.transform.position);

                    Debug.Log("section instantiated at " + m_currentTrackSection.transform.position + " from bauble at " + hoveringBaubleController.transform.position);

                    m_baubleAnchor = hoveringBaubleController.gameObject;
                    m_baubleAnchor.collider.enabled = false;
                    m_shapeController.LinkStart(m_baubleAnchor);
                }
            }
            else // currentTrackSection != null
            {
                m_shapeController.FinalizeShape();

                m_baubleAnchor.collider.enabled = true;
                m_baubleCursor.collider.enabled = true;

                if (m_baubleCursor.GetComponent<BaubleController>().GetLinkCount() == 0)
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
                if (hoveringBaubleController != null)
                {
                    // select this end of the track for editing
                    m_currentTrackSection = hoveringBaubleController.GetLastTrackSection();
                    Debug.Log("Current track section: #" + m_currentTrackSection.GetComponent<ObjectUID>().UID);
                    m_shapeController = m_currentTrackSection.GetComponent<TrackSectionShapeController>();

                    if (hoveringBaubleController.GetLinkCount() == 1)
                    {
                        Debug.Log("Found only 1 link; selecting this bauble");
                        m_baubleCursor = hoveringBaubleController.gameObject;
                        m_baubleCursor.collider.enabled = false;
                        
                    }
                    else
                    {
                        Debug.Log("Found " + hoveringBaubleController.GetLinkCount() + " links, instantiating new bauble");
                        m_baubleCursor = (GameObject)Instantiate(baublePrefab, hoveringBaubleController.transform.position, hoveringBaubleController.transform.rotation);
                        m_baubles.Add(m_baubleCursor);
                    }

                    m_shapeController.SelectBaubleForEditing(hoveringBaubleController.gameObject);
                    m_baubleAnchor = m_shapeController.GetOtherBauble(hoveringBaubleController.gameObject);
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

    public void LinkBaublesFromUIDs()
    {
        foreach (GameObject trackSection in m_trackSections)
        {
            TrackSectionShapeController tssc = trackSection.GetComponent<TrackSectionShapeController>();
            TrackSectionSaveLoad tssl = trackSection.GetComponent<TrackSectionSaveLoad>();
            int startUID = tssl.GetStartBaubleUID();
            int endUID = tssl.GetEndBaubleUID();

            foreach (GameObject bauble in m_baubles)
            {
                int uid = bauble.GetComponent<ObjectUID>().UID;
                if (uid == startUID)
                {
                    tssc.LinkStart(bauble, false);
                    startUID = -1;
                }
                else if (uid == endUID)
                {
                    tssc.LinkEnd(bauble, false);
                    endUID = -1;
                }

                if (startUID < 0 && endUID < 0) break;
            }

            Debug.Log("After linking, my endpoint is " + tssc.GetEndPoint());
            Debug.Log("After linking, my rotation is " + trackSection.transform.rotation);
        }
    }

    public void InstantiateBauble(IDataObject bData)
    {
        GameObject bauble = (GameObject)Instantiate(baublePrefab);
        bauble.GetComponent<BaubleSaveLoad>().LoadFromDataObject(bData);
        bauble.SetActive(false);
        bauble.collider.enabled = true;
        m_baubles.Add(bauble);
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
        m_shapeController.LinkEnd(m_baubleCursor);
    }

    private void DeleteCurrentTrackSection() //m_currentTrackSection != null
    {
        if (m_baubleAnchor.GetComponent<BaubleController>().GetLinkCount() <= 1)
        {
            m_baubles.Remove(m_baubleAnchor);
            Destroy(m_baubleAnchor);
        }
        else
        {
            m_baubleAnchor.collider.enabled = true;
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

    public void ResetLists()
    {
        m_baubles = new List<GameObject>();
        m_trackSections = new List<GameObject>();
    }
}
