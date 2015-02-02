using UnityEngine;
using System.Collections.Generic;

public class TrackPlacementTool : MonoBehaviour, ITool
{
    public float ballastWidth;
    public float verticalOffset = 0.01f;

    private GameObject m_currentTrackSection;
    private TrackSectionShapeController m_shapeController;
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
                    m_shapeController = m_currentTrackSection.GetComponent<TrackSectionShapeController>();
                }
            }
            else
            {
                //need to move the terrain such that it sits directly under the track
                m_terrainController.SetLineHeight(m_currentTrackSection.transform.position + verticalOffset * Vector3.down, m_shapeController.GetEndPoint() + verticalOffset * Vector3.down, ballastWidth);

                //leave the current section where it is
                m_currentTrackSection = null;
                m_shapeController = null;
            }
        }
        else if (m_currentTrackSection != null)
        {
            //stretch it to the mouse pointer
            Vector3 location;

            if (CameraController.GetMouseHitTerrainLocation(out location))
            {
                location += verticalOffset * Vector3.up;
                m_shapeController.SetEndPoint(location);
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
                        m_shapeController = m_currentTrackSection.GetComponent<TrackSectionShapeController>();

                        m_shapeController.SelectBaubleForEditing(hit.gameObject);
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
            m_shapeController = null;
        }

        SetTrackSectionBaubleVisibility(false);
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
