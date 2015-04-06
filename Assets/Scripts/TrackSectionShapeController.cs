using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class TrackSectionShapeController : MonoBehaviour
{

    public GameObject trackModel;
    public GameObject colliderObjectReference;

	private float m_currentLength;
    private Vector3 m_endPoint;
    private Quaternion m_endRotation;

    private float m_verticalOffset;
    
    public float ballastWidth = 0.1f;
    public float trackColliderHeight = 0.05f;

    public float maxRailAngle;

    private Stack<GameObject> m_currentModels;

    private BaubleController m_startTrackLink, m_endTrackLink;

    private Vector3[] m_rail;

    //Curve Parameters
    private float m_L1, m_L2, m_A1, m_A2;
    private float m_theta1, m_theta2;

    private enum Mode
    {
        Straight,
        Curved,
        Compound
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
        m_verticalOffset = Control.GetControl().trackPlacer.verticalOffset;
    }

    private void SetCurvature()
    {
        if (m_endTrackLink == null || m_startTrackLink == null) return;

        if (m_endTrackLink.GetLinkCount() == 1 && m_startTrackLink.GetLinkCount() == 1)
        {
            SetStraight();
        }
        else if (m_endTrackLink.GetLinkCount() == 1 || m_startTrackLink.GetLinkCount() == 1)
        {
            m_mode = Mode.Curved;
        }
        else
        {
            m_mode = Mode.Compound;
        }
    }

    public bool IsStraight()
    {
        return m_mode == Mode.Straight;
    }

    public bool IsCurved()
    {
        return m_mode == Mode.Curved;
    }

    public void SetStraight()
    {
        if (m_mode != Mode.Straight) RestoreTrackSections();
        m_mode = Mode.Straight;
    }

    public void SetCurved()
    {
        m_mode = Mode.Curved;
    }

    public void SetCompound()
    {
        m_mode = Mode.Compound;
    }

    public void LinkStart(GameObject bauble, bool checkStateChanges = true)
    {
        if (bauble == null)
        {
            UnlinkStart();
            return;
        }
        if (!checkStateChanges)
        {
            UnlinkStart();
            BaubleController bc = bauble.GetComponent<BaubleController>();
            m_startTrackLink = bc;
            bc.AddLink(gameObject);
        }
        else
        {
            LinkStart(bauble.GetComponent<BaubleController>());
        }
    }

    public void LinkStart(BaubleController bc)
    {
        if (bc == m_startTrackLink) return;

        UnlinkStart();

        if (bc == null) return;

        //Debug.Log("Linking start");

        m_startTrackLink = bc;
        bc.AddLink(gameObject);

        transform.position = bc.transform.position;
        if (m_endTrackLink != null)
        {
            transform.rotation = bc.GetRotation(gameObject);
        }

        SetCurvature();
    }

    public void LinkEnd(GameObject bauble, bool checkStateChanges = true)
    {
        if (bauble == null)
        {
            UnlinkEnd();
            return;
        }
        if (!checkStateChanges)
        {
            UnlinkEnd();
            BaubleController bc = bauble.GetComponent<BaubleController>();
            m_endTrackLink = bc;
            bc.AddLink(gameObject);
        }
        else
        {
            LinkEnd(bauble.GetComponent<BaubleController>());
        }
    }

    public void LinkEnd(BaubleController bc)
    {
        if (bc == m_endTrackLink) return;

        UnlinkEnd();

        if (bc == null) return;
        
        //Debug.Log("Linking end");
        m_endTrackLink = bc;
        bc.AddLink(gameObject);

        if (bc.GetLinkCount() > 1)
        {
            if (m_startTrackLink != null && m_startTrackLink.GetLinkCount() > 1)
            {
                m_startTrackLink.RecalculateDirections(gameObject);
                transform.rotation = m_startTrackLink.GetRotation(gameObject);
            }
            else
            {
                Quaternion trackRotationAtEnd = bc.GetRotation(gameObject);
                /*
                Vector3 startDirection = (transform.position - bc.transform.position).normalized;
                Debug.Log("Start direction: " + startDirection);
                Vector3 trackDirectionAtEnd = (trackRotationAtEnd * Vector3.right).normalized;
                Debug.Log("Track Direction at End: " + trackDirectionAtEnd);
                */
                float angle = bc.GetAngle(gameObject);

                Vector3 axis = bc.transform.up;

                Debug.Log("curvature angle: " + angle);

                transform.rotation = trackRotationAtEnd * Quaternion.AngleAxis(2 * angle, axis);

                if (m_startTrackLink != null)
                {
                    m_startTrackLink.transform.rotation = transform.rotation;
                }
            }
        }
        else
        {
            bc.transform.rotation = m_endRotation;
        }
        
        SetCurvature();

        SetEndPoint(bc.transform.position);
    }

    public void UnlinkStart()
    {
        if (m_startTrackLink == null) return;
        //Debug.Log("Unlinking start");

        m_startTrackLink.RemoveLink(gameObject);
        m_startTrackLink = null;
    }

    public void UnlinkEnd()
    {
        if (m_endTrackLink == null) return;
        //Debug.Log("Unlinking end");

        m_endTrackLink.RemoveLink(gameObject);
        m_endTrackLink = null;
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

    public void Split(BaubleController centreBauble)
    {
        //Debug.Log("Splitting track");
        GameObject newTrackSection = (GameObject)Instantiate(Control.GetControl().prefabTrackSection, centreBauble.transform.position, centreBauble.transform.rotation);
        TrackSectionShapeController tssc = newTrackSection.GetComponent<TrackSectionShapeController>();

        Control.GetControl().trackPlacer.DeleteCollidersForSection(gameObject);
        
        
        tssc.LinkStart(centreBauble);
        tssc.LinkEnd(m_endTrackLink);
        
        SetEndPoint(centreBauble.transform.position);

        BaubleController startLink = m_startTrackLink;
        UnlinkStart();
        LinkEnd(centreBauble.gameObject, false);
        LinkStart(startLink.gameObject, false);

        tssc.FinalizeShape();

        Unfinalize();
        FinalizeShape();
    }

    private void RestoreTrackSections()
    {
        //Debug.Log("RestoreTrackSections has been called");
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
                //Debug.Log("Restoring");
                m_currentModels.Peek().GetComponent<VertexCropper>().Restore();
            }

            while (m_currentModels.Count * 10 < localLength)
            {
                float xPosition = (5 + (10 * m_currentModels.Count));// * transform.localScale.x;
                GameObject newSection = (GameObject)Instantiate(trackModel, new Vector3(xPosition, 0, 0) + transform.position, Quaternion.Euler(new Vector3(-90, 0, 0)));

                newSection.transform.parent = transform;
                
                newSection.transform.localPosition = new Vector3(xPosition, m_verticalOffset / transform.localScale.y, 0);
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
            //Debug.Log("LastSectionLength: " + lastSectionLength.ToString());
            //Debug.Log("currentLength: " + m_currentLength.ToString());
            Bounds b = new Bounds(new Vector3(-5+(lastSectionLength/2), 0, 0), new Vector3(lastSectionLength, 10, 10));
            m_currentModels.Peek().GetComponent<VertexCropper>().Crop(b);
        }

        m_currentLength = length;

        CalculateRail();
    }

    public void Curve()
    {
        if (!CalculateParameters()) return;

        float length = m_L1 / m_A1 + m_L2 / m_A2;

        RestoreTrackSections();
        SetLength(length);

        foreach (GameObject trackModel in m_currentModels)
        {
            Vector3 relativeMovablePosition = new Vector3(m_currentLength/transform.localScale.x - trackModel.transform.localPosition.x, 0, 0) * trackModel.transform.localScale.x;
            Vector3 relativeFixedPosition = new Vector3(-trackModel.transform.localPosition.x * trackModel.transform.localScale.x, 0, 0);
            Vector3 relativeTargetPosition = trackModel.transform.InverseTransformPoint(m_endPoint + m_verticalOffset * Vector3.up);
            Vector3 relativeTargetDirection = trackModel.transform.InverseTransformDirection(m_endRotation * Vector3.right);

            Vector3[] relativeRailWaypoints = null;
            // pick out the waypoints that are within this model's influence
            int first = m_rail.Length, last = 0;
            for (int i = 0; i < m_rail.Length; i++)
            {
                Vector3 v = m_rail[i];
                if (v.x >= (trackModel.transform.localPosition.x - 5) * transform.localScale.x && v.x < (trackModel.transform.localPosition.x + 5) * transform.localScale.x)
                {
                    if (i < first) first = i;
                    last = i;
                }
            }

            //Debug.Log("First = " + first + ", last = " + last);

            if (first <= last)
            {
                relativeRailWaypoints = new Vector3[(last - first + 1)];
                for (int i = first; i <= last; i++)
                {
                    relativeRailWaypoints[i - first] = m_rail[i] / trackModel.transform.lossyScale.x + relativeFixedPosition;
                }

                //Debug.Log("Relative fixed: " + relativeFixedPosition + ", RelativeEnd: " + relativeMovablePosition);
                //Debug.Log("Relative first rail: " + relativeRailWaypoints[0] + ", Relative last rail: " + relativeRailWaypoints[relativeRailWaypoints.Length - 1]);
            }
            
            trackModel.GetComponent<VertexBender>().Bend(m_L1, m_L2, m_A1 * trackModel.transform.lossyScale.x, m_A2 * trackModel.transform.lossyScale.x, m_theta1, m_theta2,
                                                        relativeFixedPosition, 
                                                        relativeMovablePosition,  
                                                        relativeTargetPosition, 
                                                        relativeTargetDirection, 
                                                        ref relativeRailWaypoints);

            for (int i = 0; i < relativeRailWaypoints.Length; i++)
            {
                m_rail[i + first] = trackModel.transform.TransformPoint(relativeRailWaypoints[i] + relativeFixedPosition) - m_verticalOffset * Vector3.up;
            }
        }
    }

    // this function assigns values to L1, L2 etc assuming m_endPoint, m_endRotation etc are fixed. 
    // Returns false if params can't be found
    private bool CalculateParameters()
    {
        Vector3 startPosition, targetPosition, startDirection, targetDirection;

        startPosition = transform.position;
        targetPosition = m_endPoint;
        startDirection = transform.rotation * Vector3.right;
        targetDirection = m_endRotation * Vector3.right;

        FresnelMath.FindTheta(out m_theta1, out m_theta2, startPosition, targetPosition, startDirection, -targetDirection);
        if (m_theta1 < 0) return false;

        float x = Vector3.Dot((targetPosition - startPosition), startDirection.normalized);

        float phi = m_theta1 + m_theta2;

        m_A2 = FresnelMath.A2(m_theta1, phi, x);
        m_A1 = FresnelMath.A1(m_A2, m_theta1, phi);

        m_L1 = Mathf.Sqrt(m_theta1);
        m_L2 = Mathf.Sqrt(m_theta2);

        return true;
    }

    public void SetEndPoint(Vector3 point)
    {
        //Debug.Log("In SetEndPoint");

        if (point != m_endPoint)
        {
            //Debug.Log("Point != EndPoint");
            m_endPoint = point;


            // ideally, all cases will be considered a compound curve and be handled correctly
            if (m_startTrackLink != null && m_endTrackLink != null)
            {
                if (m_startTrackLink.GetLinkCount() == 1 && m_endTrackLink.GetLinkCount() == 1)
                {
                    SetStraight();
                }
                else
                {
                    if (m_startTrackLink.GetLinkCount() > 1)
                    {
                        Vector3 forward = m_startTrackLink.GetRotation(gameObject) * Vector3.right;
                        if (TrainsMath.AreApproximatelyEqual(Vector3.Dot(point - transform.position, forward), (point - transform.position).magnitude))
                        {
                            SetStraight();
                        }
                        else
                        {
                            if (m_endTrackLink.GetLinkCount() > 1)
                                m_mode = Mode.Compound;
                            else
                                m_mode = Mode.Curved;
                        }
                    }
                    else // m_endTrackLink.GetLinkCount() > 1
                    {
                        Vector3 rearward = m_endTrackLink.GetRotation(gameObject) * Vector3.right;
                        if (TrainsMath.AreApproximatelyEqual(Vector3.Dot(transform.position - point, rearward), (transform.position - point).magnitude))
                        {
                            SetStraight();
                        }
                        else
                        {
                            m_mode = Mode.Curved;
                        }
                    }
                }
            }

            //Debug.Log("Setting #" + GetComponent<SaveLoad>().UID + " from " + transform.position + " to " + point + "; " + m_mode);
            switch (m_mode)
            {
                case (Mode.Straight):
                    {
                        Vector3 track = m_endPoint - transform.position;

                        transform.rotation = Quaternion.LookRotation(track) * Quaternion.Euler(0, -90, 0);
                        //Debug.Log("It is straight");
                        SetLength(track.magnitude);
                        m_endRotation = transform.rotation;
                        break;
                    }
                case (Mode.Curved):
                    {
                        float length, angle;
                        Vector3 rotationAxis;
                        VertexBender.GetBentLengthAndRotation(transform.right, point - transform.position, out length, out rotationAxis, out angle);
                        //Debug.Log("end bauble rotation axis: " + rotationAxis + " angle (radians): " + angle);

                        if (angle > Mathf.PI / 2) //We're asking to bend behind us; just turn the section around
                        {
                            transform.rotation = transform.rotation * Quaternion.AngleAxis(180, rotationAxis);
                            //Debug.Log("I'm spinning around, get out of my way");
                            VertexBender.GetBentLengthAndRotation(transform.right, point - transform.position, out length, out rotationAxis, out angle);
                        }

                        m_endRotation = transform.rotation * Quaternion.AngleAxis(angle * 2 * Mathf.Rad2Deg, rotationAxis);

                        //Debug.Log("end bauble rotation axis: " + rotationAxis + " angle (radians): " + angle);
                        //Debug.Log("endRotation = " + m_endRotation);

                        Curve();

                        break;
                    }
                case (Mode.Compound):
                    {
                        Curve();
                        break;
                    }
            }

            if (m_endTrackLink != null && m_endTrackLink.GetLinkCount() == 1)
            {
                m_endTrackLink.transform.rotation = m_endRotation;
                m_endTrackLink.transform.position = m_endPoint;
            }
            if (m_startTrackLink != null && m_startTrackLink.GetLinkCount() == 1)
            {
                m_startTrackLink.transform.rotation = transform.rotation;
            }
        }
    }

    public void CalculateRail()
    {
        if (m_mode == Mode.Straight)
        {
            //Debug.Log("Calculating straight section rail");

            m_rail = new Vector3[2];
            m_rail[0] = transform.position;
            m_rail[1] = m_endPoint;
        }
        else
        {
            m_rail = RailVectorCreator.CreateRailVectors(m_L1, m_L2, m_A1, m_A2, m_theta1, m_theta2, transform.position, m_endPoint, maxRailAngle);
        }
    }

    public void FinalizeShape()
    {
        BoxCollider[] existingColliders = GetComponentsInChildren<BoxCollider>();
        foreach (BoxCollider boxCollider in existingColliders)
        {
            Destroy(boxCollider.gameObject);
        }

        

        for (int i = 1; i < m_rail.Length; i++)
        {
            float averageTrackHeight = (m_rail[i-1].y + m_rail[i].y) / 2f;

            GameObject boxColliderChild = (GameObject)GameObject.Instantiate(colliderObjectReference);
            boxColliderChild.transform.parent = transform;
            boxColliderChild.transform.position = (m_rail[i-1] + m_rail[i]) / 2f + Vector3.down * averageTrackHeight / 2f + Vector3.up * trackColliderHeight;
            boxColliderChild.transform.rotation = Quaternion.LookRotation(m_rail[i] - m_rail[i-1]);
            boxColliderChild.transform.localScale = Vector3.one;

            BoxCollider box = boxColliderChild.GetComponent<BoxCollider>();
            box.size = new Vector3(2.5f, 0.34f - averageTrackHeight / transform.localScale.y, (m_rail[i] - m_rail[i-1]).magnitude / transform.localScale.z);
            box.center = Vector3.zero;
        }

        if (m_endTrackLink != null) m_endTrackLink.RecalculateDirections(gameObject);
        if (m_startTrackLink != null) m_startTrackLink.RecalculateDirections(gameObject);
    }

    // reverse anything that FinalizeShape() did. Doesn't undo SetBallast though.
    public void Unfinalize()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider c in colliders)
        {
            Destroy(c.gameObject);
        }
    }

    public void SetBallast()
    {
        TerrainController terrrainController = Control.GetControl().GetTerrainController();
        for (int i = 1; i < m_rail.Length; i++)
        {
            terrrainController.SetLineHeight(m_rail[i - 1], m_rail[i], ballastWidth);
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

    public void SelectBaubleForEditing(GameObject bauble)
    {
        if (bauble == m_startTrackLink.gameObject)
        {
            BaubleController otherBaubleController = m_endTrackLink;
            //Debug.Log("Shape controller returning end track link");

            LinkEnd(m_startTrackLink);
            LinkStart(otherBaubleController);
        }
    }

    public Quaternion GetEndRotation()
    {
        return m_endRotation;
    }

    public void SetEndRotation(Quaternion endRotation)
    {
        m_endRotation = endRotation;
    }

    public GameObject GetOtherBauble(GameObject bauble)
    {
        if (m_startTrackLink == null || m_endTrackLink == null) return null;

        if (m_startTrackLink.gameObject == bauble) return m_endTrackLink.gameObject;

        if (m_endTrackLink.gameObject == bauble) return m_startTrackLink.gameObject;

        return null;
    }

    // finds the real-world co-ordinates of the middle of the track closest to location
    public void FindTrackCenter(Vector3 location, out Vector3 centre, out Quaternion direction)
    {
        //Use rails as an approximation of track curvature.

        float minimumDistance = float.MaxValue;
        int minimumDistanceIndex = 0;

        float firstDistance = (location - m_rail[0]).magnitude;
        for (int i = 1; i < m_rail.Length; i++)
        {
            float secondDistance = (location - m_rail[i]).magnitude;
            if (firstDistance + secondDistance < minimumDistance)
            {
                minimumDistance = firstDistance + secondDistance;
                minimumDistanceIndex = i;
            }
            firstDistance = secondDistance;
        }

        Vector3 rail1 = m_rail[minimumDistanceIndex - 1];
        Vector3 rail2 = m_rail[minimumDistanceIndex];
        Vector3 line1to2 = rail2 - rail1;
        Vector3 line1toL = location - rail1;

        float d = Vector3.Dot(line1toL, line1to2) / line1to2.sqrMagnitude;
        if (d < 0) d = 0;
        if (d > 1) d = 1;

        centre = rail1 + d * line1to2;

        direction = Quaternion.LookRotation(line1to2, transform.up);
    }
}

public static class RailVectorCreator
{
    // this function
    public static Vector3[] CreateRailVectors(float L1, float L2, float A1, float A2, float theta1, float theta2, 
                                            Vector3 startPosition, 
                                            Vector3 endPosition,
                                            float maxAngleDeg)
    {
        Vector3[] rails;

        if (L1 == 0)
        {
            rails = new Vector3[2];
            rails[0] = startPosition;
            rails[1] = endPosition;
            return rails;
        }

        float length = L1/A1 + L2/A2;
        float maxAngle = maxAngleDeg * Mathf.Deg2Rad;

        float phi = theta1 + theta2;
        int nVectors = Mathf.FloorToInt(phi / maxAngle) + 2;

        rails = new Vector3[nVectors];

        /*
        rails[0] = Vector3.zero;
        rails[nVectors - 1] = new Vector3(length, 0, 0);
        */
        float anglePerSection = phi / (nVectors - 1);

        for (int i = 0; i < rails.Length; i++)
        {
            float angleForVector = i * anglePerSection;
            float distanceAlongLength;

            if (angleForVector < theta1)
            {
                distanceAlongLength = Mathf.Sqrt(angleForVector) / A1;
            }
            else
            {
                distanceAlongLength = length - Mathf.Sqrt(phi - angleForVector) / A2;
            }

            rails[i] = new Vector3(distanceAlongLength, 0, 0);
//            rails[i] = new Vector3((float)(i * length) / (float)(rails.Length - 1), 0, 0);
        }

        return rails;
    }

}