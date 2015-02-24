using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class TrackSectionShapeController : MonoBehaviour {

    public GameObject trackModel;
    public GameObject colliderObjectReference;

	private float m_currentLength;
    private Vector3 m_endPoint;

    private const float c_verticalOffset = 0.01f;
    
    public float ballastWidth = 0.1f;

    private Stack<GameObject> m_currentModels;

    private GameObject c_startBauble, c_endBauble;
    private GameObject m_startTrackLink, m_endTrackLink;

    private Vector3[] m_rail;

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

        SetBaubleVisibility(false);

        m_currentModels = new Stack<GameObject>();
//        m_collider = GetComponent<BoxCollider>();
//        m_collider.center = new Vector3(0, 0.1f, 0);
//        m_collider.size = new Vector3(10, 0.24f, 2.5f);
        SetLength(0);
        transform.position = transform.position;
        m_endPoint = transform.position + Vector3.left;
        m_mode = Mode.Straight;
    }

    public void SetStraight()
    {
        Debug.Log("Setting mode to straight");
        m_mode = Mode.Straight;
        RestoreTrackSections();
        c_endBauble.transform.localRotation = Quaternion.identity;
    }

    public bool IsStraight()
    {
        return m_mode == Mode.Straight;
    }

    public void SetCurved()
    {
        m_mode = Mode.Curved;
    }

    public void LinkStart(GameObject bauble)
    {
        if (bauble == c_startBauble || bauble == c_endBauble) return;

        if (m_startTrackLink != bauble)
        {
            Debug.Log("Linking start");
            transform.position = bauble.transform.position;
            transform.rotation = bauble.transform.rotation;

            m_startTrackLink = bauble;

            bauble.transform.parent.GetComponent<TrackSectionShapeController>().Link(bauble, c_startBauble);

            m_mode = Mode.Curved;
        }
    }

    public void LinkEnd(GameObject bauble)
    {
        if (bauble == c_startBauble || bauble == c_endBauble) return;
        
        if (m_startTrackLink != bauble)
        {
            Debug.Log("Linking end");
            m_endPoint = bauble.transform.position;

            m_endTrackLink = bauble;

            bauble.transform.parent.GetComponent<TrackSectionShapeController>().Link(bauble, c_endBauble);

            m_mode = Mode.Curved;
        }
    }

    public void UnlinkStart()
    {
        Debug.Log("Unlinking start");
        m_startTrackLink = null;

        if (m_endTrackLink == null)
        {
            SetStraight();
        }
    }

    public void UnlinkEnd()
    {
        Debug.Log("Unlinking end");
        m_endTrackLink = null;

        if (m_startTrackLink == null)
        {
            SetStraight();
        }
    }

    public void Link(GameObject myBauble, GameObject linkBauble)
    {
        if (myBauble == c_endBauble)
            LinkEnd(linkBauble);

        else if (myBauble == c_startBauble)
            LinkStart(linkBauble);

        else
            Debug.Log("Error: Link called with unknown myBauble");
    }

    private void RestoreTrackSections()
    {
        foreach (GameObject trackModel in m_currentModels)
        {
            trackModel.GetComponent<VertexCropper>().Restore();
        }
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
                
                newSection.transform.localPosition = new Vector3(xPosition, 0, 0) + (c_verticalOffset * Vector3.up / transform.localScale.y);
                newSection.transform.localRotation = Quaternion.Euler(new Vector3(-90, 0, 0));
                newSection.transform.localScale = Vector3.one;
                
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

        c_endBauble.transform.position = m_endPoint;
    }

    public void SetCurve()
    {
        foreach (GameObject trackModel in m_currentModels)
        {
            Vector3 relativeMovablePosition = new Vector3(m_currentLength/transform.localScale.x - trackModel.transform.localPosition.x, 0, 0) * trackModel.transform.localScale.x;
            Vector3 relativeFixedPosition = (-trackModel.transform.localPosition * trackModel.transform.localScale.x);
            Vector3 relativeTargetPosition = trackModel.transform.InverseTransformPoint(m_endPoint + c_verticalOffset * Vector3.up);
            /*
            Debug.Log("RelativeFixedPosition: " + relativeFixedPosition);
            Debug.Log("RelativeMovablePosition: " + relativeMovablePosition);
            
            Debug.Log("real target position: " + m_endPoint);
            Debug.Log("RelativeTargetPosition: " + relativeTargetPosition);
            */
            trackModel.GetComponent<VertexBender>().Bend(relativeFixedPosition, relativeMovablePosition, relativeTargetPosition);
        }


    }

    public void SetEndPoint(Vector3 point)
    {
        if (point != m_endPoint)
        {
            switch (m_mode)
            {
                case (Mode.Straight):
                    {
                        m_endPoint = point;
                        Vector3 track = m_endPoint - transform.position;

                        transform.rotation = Quaternion.LookRotation(track) * Quaternion.Euler(0, -90, 0);

                        SetLength(track.magnitude);
                        break;
                    }
                case (Mode.Curved):
                    {
                        m_endPoint = point;

                        float length, angle;
                        Vector3 rotationAxis;
                        VertexBender.GetBentLengthAndRotation(Vector3.Cross(transform.up, transform.forward), point - transform.position, out length, out rotationAxis, out angle);
                        if (length > 0)
                        {
                            RestoreTrackSections();

                            SetLength(length);

                            SetCurve();

//                            Debug.Log("end bauble rotation axis: " + rotationAxis + " angle: " + angle);

                            c_endBauble.transform.localRotation = Quaternion.AngleAxis(angle, rotationAxis);

                            
                        }
                        break;
                    }
            }
        }
    }

    public void FinalizeShape()
    {
        BoxCollider[] existingColliders = GetComponentsInChildren<BoxCollider>();
        foreach (BoxCollider boxCollider in existingColliders)
        {
            Destroy(boxCollider.gameObject);
        }

        if (m_mode == Mode.Straight)
        {
            Debug.Log("Finalising straight section");

            m_rail = new Vector3[2];
            m_rail[0] = transform.position;
            m_rail[1] = m_endPoint;
        }
        else if (m_mode == Mode.Curved)
        {
            Debug.Log("Finalising curved section");

            Vector3[] temp = new Vector3[Mathf.CeilToInt(Quaternion.Angle(c_endBauble.transform.localRotation, Quaternion.identity) / 10f * m_currentLength)+1]; // approximate to be fairly large?
            for (int i = 0; i < temp.Length; i++)
            {
                temp[i] = new Vector3((float)(i*m_currentLength)/(float)(temp.Length - 1), 0, 0);
            }

            VertexBenderLogic.BendVectors(temp, out m_rail, new Vector3(m_currentLength, 0, 0), transform.InverseTransformPoint(m_endPoint));

            for (int i = 0; i < m_rail.Length; i++)
            {
                m_rail[i] = transform.TransformPoint(m_rail[i]);
            }

        }

        TerrainController terrrainController = Control.GetControl().GetTerrainController();
        for (int i = 1; i < m_rail.Length; i++)
        {
            terrrainController.SetLineHeight(m_rail[i-1], m_rail[i], ballastWidth);

            float averageTrackHeight = (m_rail[i-1].y + m_rail[i].y) / 2f;

            GameObject boxColliderChild = (GameObject)GameObject.Instantiate(colliderObjectReference);
            boxColliderChild.transform.parent = transform;
            boxColliderChild.transform.position = (m_rail[i-1] + m_rail[i]) / 2f + Vector3.down * averageTrackHeight / 2f;
            boxColliderChild.transform.rotation = Quaternion.LookRotation(m_rail[i] - m_rail[i-1]);
            boxColliderChild.transform.localScale = Vector3.one;

            BoxCollider box = boxColliderChild.GetComponent<BoxCollider>();
            box.size = new Vector3(2.5f, 0.34f - averageTrackHeight / transform.localScale.y, (m_rail[i] - m_rail[i-1]).magnitude / transform.localScale.z);
            box.center = Vector3.zero;
        }

        SetBaubleVisibility(true);
    }

    public Vector3 GetPositionFromTravelDistance(float distance)
    {
        throw new NotImplementedException();
    }

    public void SetStartRotation(Quaternion rotation)
    {
        transform.rotation = rotation;
    }

    public Vector3 GetEndPoint()
    {
        return m_endPoint;
    }

    public void SelectBaubleForEditing(GameObject bauble)
    {
        SetBaubleVisibility(false);

        if (bauble == c_startBauble)
        {
            UnlinkStart();

            if (m_endTrackLink != null)
            {
                LinkStart(m_endTrackLink);
            }
            else
            {
                Debug.Log("Setting start position to " + m_endPoint);
                transform.position = m_endPoint;
            }
        }
        
        UnlinkEnd();
    }

    public void SetBaubleVisibility(bool visible)
    {
        c_endBauble.SetActive(visible);
        c_startBauble.SetActive(visible);
    }
}
