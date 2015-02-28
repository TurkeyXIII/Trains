using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class TrackSectionShapeController : MonoBehaviour {

    public GameObject trackModel;
    public GameObject colliderObjectReference;

	private float m_currentLength;
    private Vector3 m_endPoint;
    private Quaternion m_endRotation;

    private const float c_verticalOffset = 0.01f;
    
    public float ballastWidth = 0.1f;

    private Stack<GameObject> m_currentModels;

    private BaubleController m_startTrackLink, m_endTrackLink;

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

    void OnDestroy()
    {
        UnlinkStart();
        UnlinkEnd();
    }

    public void Initialize()
    {
        m_currentModels = new Stack<GameObject>();
//        m_collider = GetComponent<BoxCollider>();
//        m_collider.center = new Vector3(0, 0.1f, 0);
//        m_collider.size = new Vector3(10, 0.24f, 2.5f);
        SetLength(0);
        transform.position = transform.position;
        m_endPoint = transform.position + Vector3.left;
        m_mode = Mode.Straight;
    }

    private void SetStraight()
    {
        Debug.Log("Setting mode to straight");
        m_mode = Mode.Straight;
        RestoreTrackSections();
        m_endRotation = transform.rotation;
    }

    public bool IsStraight()
    {
        return m_mode == Mode.Straight;
    }

    public void LinkStart(GameObject bauble)
    {
        if (bauble == null)
        {
            UnlinkStart();
            return;
        }

        LinkStart(bauble.GetComponent<BaubleController>());
    }

    public void LinkStart(BaubleController bc)
    {
        if (bc == m_startTrackLink) return;

        UnlinkStart();

        if (bc == null) return;

        Debug.Log("Linking start");
       
        transform.position = bc.transform.position;
        transform.rotation = bc.transform.rotation;
        
        m_startTrackLink = bc;

        bc.AddLink(gameObject);
    }

    public void LinkEnd(GameObject bauble)
    {
        if (bauble == null)
        {
            UnlinkEnd();
            return;
        }

        LinkEnd(bauble.GetComponent<BaubleController>());
    }

    public void LinkEnd(BaubleController bc)
    {
        if (bc == m_endTrackLink) return;

        UnlinkEnd();

        if (bc == null) return;
        
        Debug.Log("Linking end");
        if (m_endPoint != bc.transform.position)
        {
            m_endPoint = bc.transform.position;
        }
        else
        {
            bc.transform.rotation = m_endRotation;
        }
        m_endTrackLink = bc;

        bc.AddLink(gameObject);
    }

    public void UnlinkStart()
    {
        if (m_startTrackLink == null) return;
        Debug.Log("Unlinking start");

        m_startTrackLink.RemoveLink(gameObject);
        m_startTrackLink = null;

        if (m_endTrackLink == null)
        {
            SetStraight();
        }
    }

    public void UnlinkEnd()
    {
        if (m_endTrackLink == null) return;
        Debug.Log("Unlinking end");

        m_endTrackLink.RemoveLink(gameObject);
        m_endTrackLink = null;

        if (m_startTrackLink == null)
        {
            SetStraight();
        }
    }

    public void Link(GameObject bauble)
    {
        float distanceToEnd, distanceToStart;
        distanceToEnd = (m_endPoint - bauble.transform.position).magnitude;
        distanceToStart = (transform.position - bauble.transform.position).magnitude;

        if (distanceToEnd <= distanceToStart)
        {
            LinkEnd(bauble);
        }
        else
        {
            // reverse everything
            Vector3 tempV = m_endPoint;
            m_endPoint = transform.position;
            transform.position = tempV;

            BaubleController tempBC = m_endTrackLink;
            LinkEnd(m_startTrackLink);
            LinkStart(tempBC);
        }


    }

    public GameObject GetStartBauble()
    {
        return m_startTrackLink.gameObject;
    }

    public GameObject GetEndBauble()
    {
        return m_endTrackLink.gameObject;
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
            if (m_startTrackLink.GetLinkCount() == 1 && m_endTrackLink.GetLinkCount() == 1)
                m_mode = Mode.Straight;
            else
                m_mode = Mode.Curved;

            switch (m_mode)
            {
                case (Mode.Straight):
                    {
                        m_endPoint = point;
                        Vector3 track = m_endPoint - transform.position;

                        transform.rotation = Quaternion.LookRotation(track) * Quaternion.Euler(0, -90, 0);

                        SetLength(track.magnitude);
                        m_endRotation = transform.rotation;
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

                            m_endRotation = transform.rotation * Quaternion.AngleAxis(angle, rotationAxis);

                            
                        }
                        break;
                    }
            }
            if (m_endTrackLink.GetLinkCount() == 1)
            {
                m_endTrackLink.transform.rotation = m_endRotation;
                m_endTrackLink.transform.position = m_endPoint;
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

            Vector3[] temp = new Vector3[Mathf.CeilToInt(Quaternion.Angle(transform.rotation * m_endRotation, Quaternion.identity) / 10f * m_currentLength)+1]; // approximate to be fairly large?
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

    // takes a bauble from one end of the track as argument, returns bauble from other end of the track.
    public GameObject SelectBaubleForEditing(GameObject bauble)
    {
        BaubleController otherBaubleController;

        if (bauble == m_startTrackLink.gameObject)
        {
            otherBaubleController = m_endTrackLink;
            Debug.Log("Shape controller returning end track link");

            LinkEnd(m_startTrackLink);
            LinkStart(otherBaubleController);
        }
        else
        {
            otherBaubleController = m_startTrackLink;
        }
        
        return otherBaubleController.gameObject;
    }
}
