using UnityEngine;
using System.Collections;

public class TrackPlacementTool : MonoBehaviour, ITool
{
    public float ballastWidth;
    public float verticalOffset = 0.01f;

    private GameObject m_currentTrackSection;
    private TrackSectionLengthController m_lengthController;
    private TerrainController m_terrainController;

    void Awake()
    {
        m_currentTrackSection = null;
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
        else if (m_currentTrackSection == null && Input.GetMouseButtonDown(1))
        {
            // we're not currently editing any track piece, check if we're mousing over an existing one.
            Vector3 location;
            Collider hit = CameraController.GetMouseHit(out location);
            if (hit != null)
            {
                m_lengthController = hit.GetComponent<TrackSectionLengthController>();
                if (m_lengthController != null)
                {
                    //we're over a track section
                    m_currentTrackSection = m_lengthController.gameObject;

                    float distToStart = (location - m_lengthController.transform.position).magnitude;
                    float distToEnd = (location - m_lengthController.GetEndPoint()).magnitude;

                    if (distToStart <= distToEnd) //need to turn it around so the endpoint is where the ouse pointer is
                    {
                        m_currentTrackSection.transform.position = m_lengthController.GetEndPoint();
                    }
                }
            }

        }
    }

    public void OnDeselect()
    {
        if (m_currentTrackSection != null)
        {
            Destroy(m_currentTrackSection);
            m_currentTrackSection = null;
            m_lengthController = null;
        }
    }
}
