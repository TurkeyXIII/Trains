using UnityEngine;
using System.Collections.Generic;

public class TrackPlacementTool : MonoBehaviour, ITool
{
    public float ballastWidth;
    public float verticalOffset = 0.01f;

    private GameObject m_currentTrackSection;
    private TrackSectionLengthController m_lengthController;
    private TerrainController m_terrainController;

    private List<GameObject> m_trackSections;

    void Awake()
    {
        m_currentTrackSection = null;
        m_trackSections = new List<GameObject>();
    }

    void Start()
    {
        m_terrainController = GameObject.FindGameObjectWithTag("Terrain").GetComponent<TerrainController>();
    }

    public void UpdateWhenSelected()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (m_currentTrackSection == null)
            {
                //instantiate a new track section
                Vector3 location;
                if (CameraController.GetMouseHitTerrainLocation(out location))
                {
                    location += verticalOffset * Vector3.up;
                    m_currentTrackSection = (GameObject)Instantiate(Control.GetControl().prefabTrackSection, location, Quaternion.identity);
                    m_trackSections.Add(m_currentTrackSection);
                    m_lengthController = m_currentTrackSection.GetComponent<TrackSectionLengthController>();
                }
            }
            else
            {
                //need to move the terrain such that it sits directly under the track
                m_terrainController.SetLineHeight(m_currentTrackSection.transform.position + verticalOffset * Vector3.down, m_lengthController.GetEndPoint() + verticalOffset * Vector3.down, ballastWidth);

                //leave the current section where it is
                m_currentTrackSection = null;
                m_lengthController = null;
            }
        }
        else if (m_currentTrackSection != null)
        {
            //stretch it to the mouse pointer
            Vector3 location;

            if (CameraController.GetMouseHitTerrainLocation(out location))
            {
                location += verticalOffset * Vector3.up;
                m_lengthController.SetEndPoint(location);
            }
            else
            {
                OnDeselect();
            }

        }
        else if (m_currentTrackSection == null)
        {
            // we're not currently editing any track piece, check if we're mousing over an existing one.
            Vector3 location;
            Collider hit = CameraController.GetMouseHit(out location);

            if (hit != null)
            {
                TrackSectionBaubleController baubleController = hit.GetComponent<TrackSectionBaubleController>();
                if (baubleController != null)
                {
                    baubleController.OnMouseover();

                    if (Input.GetMouseButtonDown(1))
                    {
                        // select this end of the track for editing
                        m_currentTrackSection = baubleController.transform.parent.gameObject; //baubles must be immediate child of length section
                        m_lengthController = m_currentTrackSection.GetComponent<TrackSectionLengthController>();

                        m_lengthController.SelectBaubleForEditing(hit.gameObject);
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
            m_trackSections.Remove(m_currentTrackSection);
            Destroy(m_currentTrackSection);
            m_currentTrackSection = null;
            m_lengthController = null;
        }

        SetTrackSectionBaubleVisibility(false);
    }

    private void SetTrackSectionBaubleVisibility(bool visible)
    {
        Debug.Log("Setting vis to " + visible);
        foreach (GameObject trackSection in m_trackSections)
        {
            TrackSectionLengthController tslc = trackSection.GetComponent<TrackSectionLengthController>();
            tslc.SetBaubleVisibility(visible);
            
        }
    }
}
