using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class TrackSectionShapeController : MonoBehaviour {

    public GameObject trackModel;

	private float m_currentLength;
    private Vector3 m_endPoint;

    private BoxCollider m_collider;

    private Stack<GameObject> m_currentModels;

    private GameObject c_startBauble, c_endBauble;

    private enum Mode
    {
        Straight,
        Curved
    }

    private Mode m_mode;

    void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        c_startBauble = transform.FindChild("StartBauble").gameObject;
        c_endBauble = transform.FindChild("EndBauble").gameObject;

        m_currentModels = new Stack<GameObject>();
        m_collider = GetComponent<BoxCollider>();
        m_collider.center = new Vector3(0, 0.1f, 0);
        m_collider.size = new Vector3(10, 0.24f, 2.5f);
        SetLength(0);
        m_endPoint = transform.position;
        m_mode = Mode.Straight;
    }

    public void SetStraight()
    {
        m_mode = Mode.Straight;
    }

    public void SetCurved()
    {
        m_mode = Mode.Curved;
    }

    public void SetLength(float length)
    {
        float localLength = length / transform.localScale.x;

        if (length > m_currentLength)
        {
            float localCurrentLength = m_currentLength / transform.localScale.x;

            if (Mathf.FloorToInt(localLength / 10) > Mathf.FloorToInt(localCurrentLength / 10) && m_currentModels.Count > 0)
            {
                m_currentModels.Peek().GetComponent<VertexCropper>().Restore();
            }

            while (m_currentModels.Count * 10 < localLength)
            {
                float xPosition = (5 + (10 * m_currentModels.Count));// * transform.localScale.x;
                GameObject newSection = (GameObject)Instantiate(trackModel, new Vector3(xPosition, 0, 0) + transform.position, Quaternion.Euler(new Vector3(-90, 0, 0)));

                newSection.transform.parent = transform;
                
                newSection.transform.localPosition = new Vector3(xPosition, 0, 0);
                newSection.transform.localRotation = Quaternion.Euler(new Vector3(-90, 0, 0));
                

                m_currentModels.Push(newSection);
            }
        }
        else // length <= m_currentLength
        {
            while ((m_currentModels.Count - 1) * 10 > localLength)
            {
                GameObject unusedSection = m_currentModels.Pop();

                Destroy(unusedSection);
            }
        }


        float lastSectionLength = localLength - ((m_currentModels.Count-1) * 10);

        if (lastSectionLength < 10)
        {
        //    Debug.Log("LastSectionLength: " + lastSectionLength.ToString());
        //    Debug.Log("currentLength: " + m_currentLength.ToString());
            Bounds b = new Bounds(new Vector3(-5+(lastSectionLength/2), 0, 0), new Vector3(lastSectionLength, 10, 10));
            m_currentModels.Peek().GetComponent<VertexCropper>().Crop(b);
        }

        m_currentLength = length;

        c_endBauble.transform.localPosition = new Vector3(localLength, 0, 0);

        Vector3 dummy = m_collider.center;
        dummy.x = localLength/2;
        m_collider.center = dummy;
        dummy = m_collider.size;
        dummy.x = localLength;
        m_collider.size = dummy;
    }

    public void SetEndPoint(Vector3 point)
    {
        if (point != m_endPoint)
        {
            m_endPoint = point;
            Vector3 track = m_endPoint - transform.position;

            transform.rotation = Quaternion.LookRotation(track) * Quaternion.Euler(0, -90, 0);

            SetLength(track.magnitude);
        }
    }

    public Vector3 GetEndPoint()
    {
        return m_endPoint;
    }

    public void SelectBaubleForEditing(GameObject bauble)
    {
        if (bauble == c_endBauble)
        {

        }
        else if (bauble == c_startBauble)
        {
            transform.position = m_endPoint;
        }
        else
        {
            Debug.Log("Error: bauble not found for editing");
        }

    }

    public void SetBaubleVisibility(bool visible)
    {
        c_endBauble.SetActive(visible);
        c_startBauble.SetActive(visible);
    }
}
