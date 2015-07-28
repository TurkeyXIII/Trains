using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class TrackSectionShapeController : MonoBehaviour
{

    public GameObject trackModel;
    public GameObject colliderObjectReference;

	private float m_currentLength;
    public Vector3 m_endPoint;
    private Quaternion m_endRotation;

    private float m_verticalOffset;
    
    public float ballastWidth = 0.1f;
    public float trackColliderHeight = 0.05f;

    public float maxRailAngle;

    public float minLength;

    private Stack<GameObject> m_currentModels;

    private BaubleController m_startTrackLink, m_endTrackLink;

    private Vector3[] m_rail;
    private float[] m_waypoints;

    //Curve Parameters
    private float m_L1, m_L2, m_A1, m_A2;
    private float m_theta1, m_theta2;
    private float m_lengthFraction1, m_lengthFraction2;
    private Vector3 m_virtualEndPoint, m_virtualStartPoint;
    private Quaternion m_virtualStartRotation;

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
        m_verticalOffset = Control.GetControl().trackPlacer.verticalOffset;
        SetLength(minLength);
        m_endPoint = Vector3.zero;

        m_startTrackLink = null;
        m_endTrackLink = null;
    }

    public bool IsStraight()
    {
        if (m_startTrackLink.reciprocalCurvatureRadius != 0 || m_endTrackLink.reciprocalCurvatureRadius != 0) return false;

        Vector3 forward = m_endPoint - transform.position;
        return TrainsMath.AreApproximatelyEqual(Vector3.Dot(forward, transform.right), forward.magnitude, 0.000001f);
    }

    public bool IsCurved()
    {
        return !IsStraight();
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
        //if (!bc.CanRotate()) transform.rotation = bc.GetRotation(gameObject);

        if (m_endTrackLink != null)
        {
            m_endTrackLink.RecalculateDirections(gameObject);
            ShapeTrack();
        }
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

        //if (!bc.CanRotate()) m_endRotation = bc.GetRotation(gameObject) * Quaternion.AngleAxis(180, bc.transform.up);

        if (m_startTrackLink != null)
        {
            m_startTrackLink.RecalculateDirections(gameObject);
            ShapeTrack();
        }
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
        Debug.Log("Splitting track");
        GameObject newTrackSection = (GameObject)Instantiate(Control.GetControl().prefabTrackSection, centreBauble.transform.position, centreBauble.transform.rotation);
        TrackSectionShapeController tssc = newTrackSection.GetComponent<TrackSectionShapeController>();
        
        tssc.LinkStart(centreBauble);
        tssc.LinkEnd(m_endTrackLink);

        LinkEnd(centreBauble);
        
        tssc.FinalizeShape();

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

    private bool SetLength(float length)
    {
        if (length < minLength) return false;

        float localLength = length / transform.localScale.x;

        

        while (m_currentModels.Count > 0)
        {
            GameObject section = m_currentModels.Pop();
            Destroy(section);
        }

        float horizontalOffset = 0;
        if (m_A1 > 0 && m_lengthFraction1 < 1)
        {
            horizontalOffset = (1 - m_lengthFraction1) * m_L1 / m_A1 / transform.localScale.x;
        }

        while (m_currentModels.Count * 10 < localLength)
        {
            float xPosition = (5 + (10 * m_currentModels.Count)) + horizontalOffset;
            GameObject newSection = (GameObject)Instantiate(trackModel);

            newSection.transform.parent = transform;

            newSection.transform.localPosition = new Vector3(xPosition, m_verticalOffset / transform.localScale.y, 0);
            newSection.transform.localRotation = Quaternion.Euler(new Vector3(-90, 0, 0));
            newSection.transform.localScale = Vector3.one;

            m_currentModels.Push(newSection);
        }
        
        float lastSectionLength = localLength - ((m_currentModels.Count - 1) * 10);

        if (lastSectionLength < 10)
        {
            //Debug.Log("LastSectionLength: " + lastSectionLength.ToString());
            //Debug.Log("currentLength: " + m_currentLength.ToString());
            Bounds b = new Bounds(new Vector3(-5 + (lastSectionLength / 2), 0, 0), new Vector3(lastSectionLength, 10, 10));
            m_currentModels.Peek().GetComponent<VertexCropper>().Crop(b);
        }

        m_currentLength = length;

        return true;
    }

    private bool SetLength()
    {
        float length;

        if (m_L1 != 0)
            length = (m_L1 * m_lengthFraction1) / m_A1 + (m_L2 * m_lengthFraction2) / m_A2;
            //length = m_L1 / m_A1 + m_L2 / m_A2;
        else
            length = (m_endPoint - transform.position).magnitude;

        return SetLength(length);
    }

    private void Curve()
    {
        float virtualLength = m_L1/m_A1 + m_L2/m_A2;

        Vector3 actualPosition = transform.position;
        Debug.Log("rotation from within Curve(): " + transform.rotation.eulerAngles);
        Quaternion actualRotation = transform.rotation;
        transform.position = m_virtualStartPoint;
        transform.rotation = m_virtualStartRotation;

        Debug.Log("Curve starting from " + transform.position);

        foreach (GameObject trackModel in m_currentModels)
        {
            Vector3 relativeMovablePosition = new Vector3(virtualLength/transform.localScale.x - trackModel.transform.localPosition.x, 0, 0) * trackModel.transform.localScale.x;
            Vector3 relativeFixedPosition = new Vector3(-trackModel.transform.localPosition.x * trackModel.transform.localScale.x, 0, 0);
            Vector3 relativeTargetPosition = trackModel.transform.InverseTransformPoint(m_virtualEndPoint + m_verticalOffset * Vector3.up);
            Vector3 relativeTargetDirection = trackModel.transform.InverseTransformDirection(m_endRotation * Vector3.right);

            Vector3[] relativeRailWaypoints = new Vector3[m_waypoints.Length];
            for (int i = 0; i < relativeRailWaypoints.Length; i++)
            {
                relativeRailWaypoints[i] = new Vector3(m_waypoints[i] / trackModel.transform.lossyScale.x, 0, 0) + relativeFixedPosition;
            }

            trackModel.GetComponent<VertexBender>().Bend(m_L1, m_L2, m_A1 * trackModel.transform.lossyScale.x, m_A2 * trackModel.transform.lossyScale.x, m_theta1, m_theta2,
                                                        relativeFixedPosition, 
                                                        relativeMovablePosition, 
                                                        relativeTargetPosition, 
                                                        relativeTargetDirection, 
                                                        ref relativeRailWaypoints);

            if (m_rail == null)
            {
                m_rail = new Vector3[m_waypoints.Length];
                for (int i = 0; i < relativeRailWaypoints.Length; i++)
                {
                    m_rail[i] = trackModel.transform.TransformPoint(relativeRailWaypoints[i]) - m_verticalOffset * Vector3.up;
                }
            }
        }

        
        Vector3[] positions = new Vector3[m_currentModels.Count];
        Quaternion[] rotations = new Quaternion[m_currentModels.Count];
        int count = 0;
        foreach (GameObject section in m_currentModels)
        {
            positions[count] = section.transform.position;
            rotations[count] = section.transform.rotation;
            count++;
        }
        
        transform.position = actualPosition;
        transform.rotation = actualRotation;
        
        count = 0;
        foreach (GameObject section in m_currentModels)
        {
            section.transform.position = positions[count];
            section.transform.rotation = rotations[count];
            count++;
        }
        
    }

    // this function assigns values to L1, L2 etc assuming m_endPoint, m_endRotation etc are fixed. 
    // Returns false if params can't be found
    private bool CalculateParameters()
    {
        m_virtualEndPoint = m_endPoint;
        m_virtualStartPoint = transform.position;

        m_virtualStartRotation = transform.rotation;

        m_lengthFraction1 = 1;
        m_lengthFraction2 = 1;

        if (IsStraight())
        {
            m_L1 = 0;
            m_L2 = 0;
            m_A1 = 0;
            m_A2 = 0;
            m_theta1 = 0;
            m_theta2 = 0;
            return true;
        }

        Vector3 startPosition, targetPosition, startDirection, targetDirection;

        startPosition = transform.position;
        targetPosition = m_endPoint;
        startDirection = transform.rotation * Vector3.right;
        targetDirection = m_endRotation * Vector3.right;

        // there is no curvature in either end bauble
        if (m_endTrackLink.reciprocalCurvatureRadius == 0 && m_startTrackLink.reciprocalCurvatureRadius == 0)
        {
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

        
        if (!m_startTrackLink.CanRotate() && m_endTrackLink.CanRotate())
        {
            // fixed, straight -> free, radius
            if (m_startTrackLink.reciprocalCurvatureRadius == 0)
            {
                Debug.Log("Fixed, straight -> free, radius");
                float a, theta, fractionOut;

                float x = Vector3.Dot(targetPosition - startPosition, startDirection.normalized);
                float y = ((targetPosition - startPosition) - x * startDirection).magnitude;

                float r;

                if (Vector3.Dot(transform.forward, (targetPosition - startPosition)) > 0)
                    r = 1/m_endTrackLink.reciprocalCurvatureRadius;
                else
                    r = -1/m_endTrackLink.reciprocalCurvatureRadius;

                if (r < 0)
                {
                    m_endTrackLink.reciprocalCurvatureRadius = 0;
                    return CalculateParameters();
                }

                FresnelMath.FindAForPartialTransitionOut(out a, out theta, out fractionOut, r, x, y);

                if (a < 0) return false;

                m_A1 = a;
                m_A2 = a;
                m_theta1 = theta;
                m_theta2 = theta;

                m_L1 = Mathf.Sqrt(theta);
                m_L2 = m_L1;

                float S = FresnelMath.FresnelS(m_L1);
                float C = FresnelMath.FresnelC(m_L2);
                float sin = Mathf.Sin(2*theta);
                float cos = Mathf.Cos(2*theta);
                Vector3 yDir = ((targetPosition - startPosition) - x * startDirection).normalized;

                m_lengthFraction2 = fractionOut;
                m_virtualEndPoint = startPosition + startDirection.normalized * (C + cos * C + sin * S) / a + yDir * (S + sin * C - cos * S) / a;
                
                float angle = -(theta + theta * (1 - (1-fractionOut) * (1-fractionOut))) * Mathf.Sign(m_endTrackLink.reciprocalCurvatureRadius);
                m_endRotation = transform.rotation * Quaternion.AngleAxis(angle * Mathf.Rad2Deg, transform.up);
                m_endTrackLink.transform.rotation = m_endRotation;
            }

            // fixed, radius -> free, straight
            else if (m_endTrackLink.reciprocalCurvatureRadius == 0)
            {
                Debug.Log("fixed, radius -> free, straight");
                float a, theta, fractionIn;

                float x = Vector3.Dot(targetPosition - startPosition, startDirection.normalized);
                float y = ((targetPosition - startPosition) - x * startDirection).magnitude;

                float r;
                if (Vector3.Dot(transform.forward, (targetPosition - startPosition)) > 0)
                    r = 1 / m_startTrackLink.reciprocalCurvatureRadius;
                else
                    r = -1 / m_startTrackLink.reciprocalCurvatureRadius;

                if (r < 0)
                {
                    return false;
                }

                FresnelMath.FindAForPartialTransitionIn(out a, out theta, out fractionIn, r, x, y);
                if (a < 0) return false;

                m_A1 = a;
                m_A2 = a;
                m_theta1 = theta;
                m_theta2 = theta;

                m_L1 = Mathf.Sqrt(theta);
                m_L2 = m_L1;

                float thetap = 1 / (4 * a * a * r * r);

                m_lengthFraction1 = fractionIn;
                float angle = (2 * theta - thetap) * -Mathf.Sign(m_startTrackLink.reciprocalCurvatureRadius);
                m_endRotation = transform.rotation * Quaternion.AngleAxis(angle * Mathf.Rad2Deg, transform.up);
                m_endTrackLink.transform.rotation = m_endRotation;

                Vector3 xDir = -(m_endRotation * Vector3.right);
                Vector3 yDir = (startPosition - targetPosition) - Vector3.Dot((startPosition - targetPosition), xDir) * xDir;
                float S = FresnelMath.FresnelS(m_L1);
                float C = FresnelMath.FresnelC(m_L2);
                float sin = Mathf.Sin(2 * theta);
                float cos = Mathf.Cos(2 * theta);
                m_virtualStartPoint = m_endPoint + xDir.normalized * (C + cos * C + sin * S) / a + yDir.normalized * (S + sin * C - cos * S) / a;
                /*
                GameObject marker = (GameObject)GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.transform.position = m_virtualStartPoint;
                marker.transform.localScale /= 10;
                */
                m_virtualStartRotation = transform.rotation * Quaternion.AngleAxis(thetap * Mathf.Rad2Deg * Mathf.Sign(m_startTrackLink.reciprocalCurvatureRadius), transform.up);
            }

        }

        
        if (m_startTrackLink.CanRotate() && !m_endTrackLink.CanRotate())
        {
            // free, radius -> fixed, straight
            if (m_endTrackLink.reciprocalCurvatureRadius == 0)
            {
                Debug.Log("free, radius -> fixed, straight");
                float a, theta, fractionOut;

                float x = Vector3.Dot(startPosition - targetPosition, -targetDirection.normalized);
                float y = ((startPosition - targetPosition) - x * -targetDirection).magnitude;

                float r;

                if (Vector3.Dot(Vector3.Cross(transform.up, targetDirection), (startPosition - targetPosition)) > 0)
                    r = 1 / m_startTrackLink.reciprocalCurvatureRadius;
                else
                    r = -1 / m_startTrackLink.reciprocalCurvatureRadius;

                if (r < 0)
                {
                    m_startTrackLink.reciprocalCurvatureRadius = 0;
                    return CalculateParameters();
                }

                FresnelMath.FindAForPartialTransitionOut(out a, out theta, out fractionOut, r, x, y);

                if (a < 0) return false;

                m_A1 = a;
                m_A2 = a;
                m_theta1 = theta;
                m_theta2 = theta;

                m_L1 = Mathf.Sqrt(theta);
                m_L2 = m_L1;

                float S = FresnelMath.FresnelS(m_L1);
                float C = FresnelMath.FresnelC(m_L1);
                float sin = Mathf.Sin(2 * theta);
                float cos = Mathf.Cos(2 * theta);
                Vector3 yDir = ((startPosition - targetPosition) - x * -targetDirection).normalized;

                m_lengthFraction1 = fractionOut;
                m_virtualStartPoint = targetPosition - targetDirection.normalized * (C + cos * C + sin * S) / a + yDir * (S + sin * C - cos * S) / a;

                float sign = -Mathf.Sign(m_startTrackLink.reciprocalCurvatureRadius);

                float angle = (theta + theta * (1 - (1 - fractionOut) * (1 - fractionOut)));
                transform.rotation = m_endRotation * Quaternion.AngleAxis(angle * Mathf.Rad2Deg * sign, transform.up);
                m_startTrackLink.transform.rotation = transform.rotation;

                m_virtualStartRotation = m_endRotation * Quaternion.AngleAxis(2*theta * Mathf.Rad2Deg * sign, transform.up);
            }

            // free, straight -> fixed, radius
            else if (m_startTrackLink.reciprocalCurvatureRadius == 0)
            {
                Debug.Log("free, straight -> fixed, radius");

                float a, theta, fractionIn;

                float x = Vector3.Dot(startPosition - targetPosition, -targetDirection.normalized);
                float y = ((startPosition - targetPosition) - x * -targetDirection).magnitude;

                float r;
                if (Vector3.Dot(transform.forward, (startPosition - targetPosition)) > 0)
                    r = 1 / m_endTrackLink.reciprocalCurvatureRadius;
                else
                    r = -1 / m_endTrackLink.reciprocalCurvatureRadius;

                if (r < 0)
                {
                    return false;
                }

                FresnelMath.FindAForPartialTransitionIn(out a, out theta, out fractionIn, r, x, y);
                if (a < 0) return false;

                m_A1 = a;
                m_A2 = a;
                m_theta1 = theta;
                m_theta2 = theta;

                m_L1 = Mathf.Sqrt(theta);
                m_L2 = m_L1;

                float thetap = 1 / (4 * a * a * r * r);

                m_lengthFraction2 = fractionIn;
                float angle = (2 * theta - thetap) * -Mathf.Sign(m_endTrackLink.reciprocalCurvatureRadius);
                transform.rotation = m_endRotation * Quaternion.AngleAxis(angle * Mathf.Rad2Deg, transform.up);
                m_startTrackLink.transform.rotation = transform.rotation;
                m_virtualStartRotation = transform.rotation;

                Vector3 xDir = transform.rotation * Vector3.right;
                Vector3 yDir = (targetPosition - startPosition) - Vector3.Dot((targetPosition - startPosition), xDir) * xDir;
                float S = FresnelMath.FresnelS(m_L1);
                float C = FresnelMath.FresnelC(m_L2);
                float sin = Mathf.Sin(2 * theta);
                float cos = Mathf.Cos(2 * theta);
                m_virtualEndPoint = transform.position + xDir.normalized * (C + cos * C + sin * S) / a + yDir.normalized * (S + sin * C - cos * S) / a;
                /*
                GameObject marker = (GameObject)GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.transform.position = m_virtualEndPoint;
                marker.transform.localScale /= 10;
                */
            }
        }

        if (m_endTrackLink.CanRotate() && m_startTrackLink.CanRotate())
        {
            Debug.Log("both ends free");
            // free, straight -> free, radius
            if (m_startTrackLink.reciprocalCurvatureRadius == 0)
            {
                Debug.Log("start straight");
                float theta, a;

                float distance = (m_endPoint - transform.position).magnitude;

                float r;
                if (m_endTrackLink.reciprocalCurvatureRadius > 0)
                    r = 1 / m_endTrackLink.reciprocalCurvatureRadius;
                else
                    r = -1 / m_endTrackLink.reciprocalCurvatureRadius;


                FresnelMath.FindAForSingleTransition(out a, out theta, r, distance);

                if (a < 0) return false;

                m_A1 = a;
                m_A2 = a;
                m_L1 = Mathf.Sqrt(theta);
                m_L2 = m_L1;
                m_lengthFraction1 = 1;
                m_lengthFraction2 = 0;
                m_theta1 = theta;
                m_theta2 = theta;

                float angle = Mathf.Acos(FresnelMath.FresnelC(m_L1) / m_A1 / distance);

                Debug.Log("angle = " + (angle * Mathf.Rad2Deg) + " degrees");

                transform.rotation *= Quaternion.AngleAxis(angle * Mathf.Rad2Deg * Mathf.Sign(m_endTrackLink.reciprocalCurvatureRadius), transform.up);
                m_virtualStartRotation = transform.rotation;
                m_startTrackLink.transform.rotation = transform.rotation;
                Debug.Log("rotation after correction = " + transform.rotation.eulerAngles);
                m_endRotation = transform.rotation * Quaternion.AngleAxis(theta * Mathf.Rad2Deg * -Mathf.Sign(m_endTrackLink.reciprocalCurvatureRadius), transform.up);
                m_endTrackLink.transform.rotation = m_endRotation;
                float kappa = Mathf.PI - 2 * (theta - angle);
                m_virtualEndPoint = m_endPoint + TrainsMath.RotateVector(transform.position - m_endPoint, transform.up, kappa * Mathf.Sign(m_endTrackLink.reciprocalCurvatureRadius));

            }
            else if (m_endTrackLink.reciprocalCurvatureRadius == 0) // free, raduis -> free, straight
            {
                Debug.Log("end straight");
                float theta, a;

                float distance = (m_endPoint - transform.position).magnitude;

                float r;
                if (m_startTrackLink.reciprocalCurvatureRadius > 0)
                    r = 1 / m_startTrackLink.reciprocalCurvatureRadius;
                else
                    r = -1 / m_startTrackLink.reciprocalCurvatureRadius;

                FresnelMath.FindAForSingleTransition(out a, out theta, r, distance);

                if (a < 0) return false;

                m_A1 = a;
                m_A2 = a;
                m_L1 = Mathf.Sqrt(theta);
                m_L2 = m_L1;
                m_lengthFraction1 = 0;
                m_lengthFraction2 = 1;
                m_theta1 = theta;
                m_theta2 = theta;

                float angle = Mathf.Acos(FresnelMath.FresnelC(m_L1) / m_A1 / distance);

                float sign = Mathf.Sign(m_startTrackLink.reciprocalCurvatureRadius);

                Debug.Log("angle = " + (angle * Mathf.Rad2Deg) + " degrees; radius sign = " + sign);

                m_endRotation *= Quaternion.AngleAxis(angle * Mathf.Rad2Deg * sign, transform.up);
                transform.rotation = m_endRotation * Quaternion.AngleAxis(-theta * Mathf.Rad2Deg * sign, transform.up);
                m_startTrackLink.transform.rotation = transform.rotation;
                m_endTrackLink.transform.rotation = m_endRotation;
                float kappa = Mathf.PI - 2 * (theta - angle);
                m_virtualStartPoint = transform.position + TrainsMath.RotateVector(m_endPoint - transform.position, transform.up, kappa * sign);
                m_virtualStartRotation = Quaternion.LookRotation(transform.position - m_virtualStartPoint, transform.up) * Quaternion.Euler(0, -90, 0) * Quaternion.AngleAxis(-angle * Mathf.Rad2Deg * sign, transform.up);
            }
            else // free, radius -> free, radius
            {
                Debug.Log("both ends curved");

                float theta, a;

                float distance = (m_endPoint - transform.position).magnitude;
                float sign = Mathf.Sign(m_startTrackLink.reciprocalCurvatureRadius);

                if (sign == Mathf.Sign(m_endTrackLink.reciprocalCurvatureRadius))
                {
                    m_endTrackLink.reciprocalCurvatureRadius = 0;
                    return CalculateParameters();
                }

                float rSmall, rLarge, rStart, rEnd;

                if (sign > 0)
                {
                    rStart = 1 / m_startTrackLink.reciprocalCurvatureRadius;
                    rEnd = -1 / m_endTrackLink.reciprocalCurvatureRadius;
                }
                else
                {
                    rStart = -1 / m_startTrackLink.reciprocalCurvatureRadius;
                    rEnd = 1 / m_endTrackLink.reciprocalCurvatureRadius;
                }

                if (rStart > rEnd)
                {
                    rSmall = rEnd;
                    rLarge = rStart;
                }
                else
                {
                    rSmall = rStart;
                    rLarge = rEnd;
                }

                Debug.Log("rLarge = " + rLarge + ", rSmall = " + rSmall + ", dist = " + distance);

                float fraction;
                FresnelMath.FindAForSinglePartialTransition(out a, out theta, out fraction, distance, rSmall, rLarge);

                Debug.Log("a = " + a + ", fraction = " + fraction);

                if (a < 0) return false;

                m_theta1 = theta;
                m_theta2 = theta;
                m_L1 = Mathf.Sqrt(theta);
                m_L2 = m_L1;
                m_A1 = a;
                m_A2 = a;

                float Lp = 1 / (2 * a * rLarge);

                if (rStart > rEnd)
                {
                    // transition in
                    m_lengthFraction1 = fraction;
                    m_lengthFraction2 = 0;

                    float CL = FresnelMath.FresnelC(m_L1);
                    float SL = FresnelMath.FresnelS(m_L1);
                    float alpha = Mathf.Asin((SL - FresnelMath.FresnelS(Lp)) / (a * distance));

                    m_virtualStartRotation = transform.rotation * Quaternion.AngleAxis(alpha * Mathf.Rad2Deg * -sign, transform.up);
                    m_virtualStartPoint = m_endPoint + m_virtualStartRotation * (Vector3.left * CL + Vector3.forward * SL * sign) / a;

                    transform.rotation = m_virtualStartRotation * Quaternion.AngleAxis(Lp * Lp * Mathf.Rad2Deg * -sign, transform.up);
                    m_endRotation = m_virtualStartRotation * Quaternion.AngleAxis(-theta * Mathf.Rad2Deg * -sign, transform.up);
                    m_startTrackLink.transform.rotation = transform.rotation;
                    m_endTrackLink.transform.rotation = m_endRotation;
                    
                    Quaternion virtualEndRotation = m_endRotation * Quaternion.AngleAxis(-theta * Mathf.Rad2Deg * -sign, transform.up);
                    m_virtualEndPoint = m_endPoint + virtualEndRotation * (Vector3.right * CL + Vector3.forward * SL * sign) / a;
                    
                    /*
                    GameObject startMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    startMarker.transform.position = m_virtualStartPoint;
                    startMarker.transform.rotation = m_virtualStartRotation;
                    startMarker.transform.localScale /= 10;
                    
                    GameObject endMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    endMarker.transform.position = m_virtualEndPoint;
                    endMarker.transform.rotation = virtualEndRotation;
                    endMarker.transform.localScale /= 10;
                    */
                }
                else
                {
                    // transition out
                    m_lengthFraction1 = 0;
                    m_lengthFraction2 = fraction;

                    float CL = FresnelMath.FresnelC(m_L1);
                    float SL = FresnelMath.FresnelS(m_L1);
                    float alpha = Mathf.Asin((SL - FresnelMath.FresnelS(Lp)) / (a * distance));
                    Quaternion virtualEndRotation = transform.rotation * Quaternion.AngleAxis(-alpha * Mathf.Rad2Deg * -sign, transform.up);
                    m_virtualEndPoint = transform.position + virtualEndRotation * (Vector3.right * CL + Vector3.forward * SL * sign) / a;

                    m_endRotation = virtualEndRotation * Quaternion.AngleAxis(Lp * Lp * Mathf.Rad2Deg * -sign, transform.up);
                    transform.rotation = virtualEndRotation * Quaternion.AngleAxis(theta * Mathf.Rad2Deg * -sign, transform.up);

                    m_endTrackLink.transform.rotation = m_endRotation;
                    m_startTrackLink.transform.rotation = transform.rotation;

                    m_virtualStartRotation = transform.rotation * Quaternion.AngleAxis(theta * Mathf.Rad2Deg * -sign, transform.up);
                    m_virtualStartPoint = transform.position + m_virtualStartRotation * (Vector3.left * CL + Vector3.forward * SL * sign) / a;

                    /*
                    GameObject startMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    startMarker.transform.position = m_virtualStartPoint;
                    startMarker.transform.rotation = m_virtualStartRotation;
                    startMarker.transform.localScale /= 10;
                    
                    GameObject endMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    endMarker.transform.position = m_virtualEndPoint;
                    endMarker.transform.rotation = virtualEndRotation;
                    endMarker.transform.localScale /= 10;
                    */
                }

            }

        }
        

        return true;
    }

    public bool ShapeTrack()
    {
        Vector3 end = m_endTrackLink.transform.position;
        Vector3 start = m_startTrackLink.transform.position;


        //Debug.Log("Point != EndPoint");
        m_endPoint = end;
        m_endRotation = m_endTrackLink.GetRotation(gameObject) * Quaternion.AngleAxis(180, m_endTrackLink.transform.up);
        transform.position = start;
        transform.rotation = m_startTrackLink.GetRotation(gameObject);

        if (m_startTrackLink != null && m_endTrackLink != null)
        {
            if (m_startTrackLink.CanRotate() && m_endTrackLink.CanRotate())
            {
                transform.rotation = Quaternion.LookRotation(m_endPoint - transform.position) * Quaternion.Euler(0, -90, 0);
                m_endRotation = transform.rotation;

                Debug.Log("rotation after recalibration = " + transform.rotation.eulerAngles);

                m_startTrackLink.transform.rotation = transform.rotation;
                m_endTrackLink.transform.rotation = m_endRotation;
            }
            else
            {
                if (m_startTrackLink.CanRotate())
                {
                    float angle = Vector3.Angle(m_endRotation * Vector3.right, m_endPoint - transform.position);
                    Vector3 rotationAxis = Vector3.Cross(m_endRotation * Vector3.right, m_endPoint - transform.position).normalized;

                    transform.rotation = m_endRotation * Quaternion.AngleAxis(angle * 2, rotationAxis);

                    if (angle > 90)
                    {
                        m_endRotation *= Quaternion.AngleAxis(180, rotationAxis);
                        transform.rotation = m_endRotation * Quaternion.AngleAxis(angle * 2, rotationAxis);
                    }

                    m_startTrackLink.transform.rotation = transform.rotation;
                }
                else if (m_endTrackLink.CanRotate())
                {
                    float angle = Vector3.Angle(transform.rotation * Vector3.right, m_endPoint - transform.position);
                    Vector3 rotationAxis = Vector3.Cross(transform.rotation * Vector3.right, m_endPoint - transform.position).normalized;

                    m_endRotation = transform.rotation * Quaternion.AngleAxis(angle * 2, rotationAxis);

                    if (angle > 90)
                    {
                        transform.rotation *= Quaternion.AngleAxis(180, rotationAxis);
                        m_endRotation = transform.rotation * Quaternion.AngleAxis(angle * 2, rotationAxis);
                    }

                    m_endTrackLink.transform.rotation = m_endRotation;
                    m_endTrackLink.transform.position = m_endPoint;
                }
            }
        }

        if (!CalculateParameters()) return false;

        

        //RestoreTrackSections();
        SetLength();
        CalculateRail();

        if (m_L1 != 0 || m_L2 != 0) Curve();

        //Debug.Log("Setting #" + GetComponent<SaveLoad>().UID + " from " + transform.position + " to " + point + "; " + m_mode);



        return true;
    }

    // cannot be called before SetParameters()
    public void CalculateRail()
    {
        if (m_L1 == 0)
        {
            //Debug.Log("Calculating straight section rail");

            m_rail = new Vector3[2];
            m_rail[0] = transform.position;
            m_rail[1] = m_endPoint;
        }
        else
        {
            // this set of rails is in a straight line to the right, starting at Vector3.zero. It needs to be passed into the vertex bender before it will be bent.
            m_waypoints = RailVectorCreator.CreateRailVectors(m_L1, m_L2, m_A1, m_A2, m_theta1, m_theta2, m_lengthFraction1, m_lengthFraction2, transform.position, m_virtualEndPoint, maxRailAngle);
            m_rail = null;
        }
    }

    public void FinalizeShape()
    {
        // get rid of any existing colliders
        DeleteColliders();

        // if rail is null, we'll have to re-asses the shape to make sure it's correct and generate rails properly
        if (m_rail == null)
        {
            ShapeTrack();
        }

        // create colliders for the section
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
    public void DeleteColliders()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        Control.GetControl().trackPlacer.DeleteCollidersForSection(gameObject);
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

    public void Traverse(ref float distanceAlong, ref float distanceToTravel, out BaubleController reachedBauble)
    {
        distanceAlong += distanceToTravel;

        if (distanceAlong < 0)
        {
            reachedBauble = m_startTrackLink;
            distanceToTravel = distanceAlong;
        }
        else if (distanceAlong > m_currentLength)
        {
            reachedBauble = m_endTrackLink;
            distanceToTravel = distanceAlong - m_currentLength;
        }
        else
        {
            reachedBauble = null;
            distanceToTravel = 0;
        }
        distanceAlong -= distanceToTravel;
    }

    public void GetPositionFromTravelDistance(float distance, out Vector3 position, out Quaternion rotation)
    {
        if (distance <= 0)
        {
            position = transform.position;
            rotation = transform.rotation;
            return;
        }

        if (distance >= m_currentLength)
        {
            position = m_endPoint;
            rotation = m_endRotation;
            return;
        }

        if (m_L1 == 0)
        {
            position = transform.position + transform.right * distance;
            rotation = transform.rotation;
            return;
        }

        float normalisedLength;
        Vector3 unitForward;
        Vector3 unitSideways;
        Vector3 startPosition;
        Vector3 rotationAxis;
        float A;


        unitForward = m_virtualStartRotation * Vector3.right;
        Vector3 toEndPoint = m_endPoint - m_virtualStartPoint;
        unitSideways = (toEndPoint - Vector3.Dot(toEndPoint, unitForward) * unitForward).normalized;

        bool inTransitionIn = distance <= m_L1 / m_A1 * m_lengthFraction1;

        if (inTransitionIn)
        {
            // transition in
            normalisedLength = distance * m_A1 + (1 - m_lengthFraction1) * m_L1;
            startPosition = m_virtualStartPoint;
            A = m_A1;
            rotationAxis = Vector3.Cross(unitForward, unitSideways);
        }
        else
        {
            float virtualLength = m_L1/m_A1 + m_L2/m_A2;
            // transition out
            normalisedLength = (virtualLength - distance - (1 - m_lengthFraction1) * m_L1 / m_A1) * m_A2;
            startPosition = m_virtualEndPoint;
            A = m_A2;

            Vector3 startingUnitForward = unitForward;
            Vector3 startingUnitSideways = unitSideways;

            VertexBenderLogic.UnitVectorsFromRotation(m_theta1+m_theta2, startingUnitForward, startingUnitSideways, out rotationAxis, out unitForward, out unitSideways);
        }

        position = startPosition + (unitForward * (FresnelMath.FresnelC(normalisedLength) / A)) + (unitSideways * (FresnelMath.FresnelS(normalisedLength) / A));

        Vector3 positionForward, PositionSideways;
        VertexBenderLogic.UnitVectorsFromRotation(normalisedLength * normalisedLength, unitForward, unitSideways, out positionForward, out PositionSideways);
        rotation = Quaternion.LookRotation(positionForward, transform.up) * Quaternion.AngleAxis(90, transform.up);

        if (!inTransitionIn) rotation *= Quaternion.AngleAxis(180, transform.up);
    }

    public Vector3 GetEndPoint()
    {
        return m_endPoint;
    }

    public float GetLength()
    {
        return m_currentLength;
    }

    public Quaternion GetEndRotation()
    {
        return m_endRotation;
    }

    public GameObject GetOtherBauble(GameObject bauble)
    {
        if (m_startTrackLink == null || m_endTrackLink == null) return null;

        if (m_startTrackLink.gameObject == bauble) return m_endTrackLink.gameObject;

        if (m_endTrackLink.gameObject == bauble) return m_startTrackLink.gameObject;

        return null;
    }

    // finds the real-world co-ordinates of the middle of the track closest to location
    public void FindTrackCentre(Vector3 location, out Vector3 centre, out Quaternion direction)
    {
        GetPositionFromTravelDistance(GetTravelDistance(location), out centre, out direction);
    }

    // returns the real-world distance along the track for an object on the track nearest location
    public float GetTravelDistance(Vector3 location)
    {
        Vector3 unitForward = transform.rotation * Vector3.right;
        

        // for straight track the equation is simple
        if (m_L1 == 0)
        {
            float d = Vector3.Dot((location - transform.position), unitForward);
            return d;
        }

        // first translate the location such that the curve is starting at zero and beginning in the +ve x direction
        Vector3 translatedLocation = transform.InverseTransformPoint(location);
        translatedLocation *= m_A1 * transform.lossyScale.x;

        unitForward = Vector3.right;
        Vector3 toEndPoint = transform.InverseTransformPoint(m_endPoint);
        Vector3 unitSideways = (toEndPoint - Vector3.Dot(toEndPoint, unitForward) * unitForward).normalized;

        float xl = Vector3.Dot(translatedLocation, unitForward);
        float yl = Vector3.Dot(translatedLocation, unitSideways);

        //Debug.Log("Beginning Newton-Raphson for (xl, yl) = (" + xl + ", " + yl + ")");

        float distance = xl; // this first guess will be pretty accurate for low theta and not too far off otherwise
        float theta = distance * distance;

        int maxIterations = 3;
        float maxError = 0.001f;

        for (int i = 0; i < maxIterations; i++)
        {
            float sinTheta = Mathf.Sin(theta);
            float cosTheta = Mathf.Cos(theta);
            float CRtTheta = FresnelMath.FresnelC(distance);
            float SRtTheta = FresnelMath.FresnelS(distance);
   
            float f = cosTheta * (xl - CRtTheta) + sinTheta * (yl - SRtTheta);
            float dfdt = -1 - (xl - CRtTheta) * sinTheta + (yl - SRtTheta) * cosTheta;

            float error = f / dfdt;

            theta -= error;
            
            if (theta > m_theta1) theta = m_theta1;
            if (theta < 0) theta = 0;
            

            distance = Mathf.Sqrt(theta);

            if (error < maxError && error > -maxError) break;
        }

        if (distance < m_L1 && distance > 0)
        {
            //Debug.Log("theta of " + theta + " matches for (" + FresnelMath.FresnelC(distance) + ", " + FresnelMath.FresnelS(distance) + ")");
            return distance / m_A1;
        }

        // if that didn't work, we're in the second half of the track.
        // translate and scale (xl, yl) and do it again
        translatedLocation = transform.InverseTransformPoint(location) - transform.InverseTransformPoint(m_endPoint);
        translatedLocation *= m_A2 * transform.lossyScale.x;

        VertexBenderLogic.UnitVectorsFromRotation(m_theta1+m_theta2, unitForward, unitSideways, out unitForward, out unitSideways);

        xl = Vector3.Dot(translatedLocation, unitForward);
        yl = Vector3.Dot(translatedLocation, unitSideways);

        //Debug.Log("Beginning Newton-Raphson for (xl, yl) = (" + xl + ", " + yl + ")");

        distance = xl; // this first guess will be pretty accurate for low theta and not too far off otherwise
        theta = distance * distance;

        for (int i = 0; i < maxIterations; i++)
        {
            float sinTheta = Mathf.Sin(theta);
            float cosTheta = Mathf.Cos(theta);
            float CRtTheta = FresnelMath.FresnelC(distance);
            float SRtTheta = FresnelMath.FresnelS(distance);

            float f = cosTheta * (xl - CRtTheta) + sinTheta * (yl - SRtTheta);
            float dfdt = -1 - (xl - CRtTheta) * sinTheta + (yl - SRtTheta) * cosTheta;

            float error = f / dfdt;

            theta -= error;

            if (theta > m_theta2) theta = m_theta2;
            if (theta < 0) theta = 0;


            distance = Mathf.Sqrt(theta);

            if (error < maxError && error > -maxError) break;
        }

        if (distance < m_L2 && distance > 0)
        {
            //Debug.Log("theta of " + theta + " matches for (" + FresnelMath.FresnelC(distance) + ", " + FresnelMath.FresnelS(distance) + ")");
            return m_currentLength - (distance / m_A2);
        }
        
        //Debug.Log("Returning end of track");
        return m_currentLength;
    }

    private void GetRailVectorsFromLocation(Vector3 location, out int firstRailIndex, out float fractionAlongRail)
    {
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

        firstRailIndex = minimumDistanceIndex - 1;
        fractionAlongRail = d;
    }
}

public static class RailVectorCreator
{
    public static float[] CreateRailVectors(float L1, float L2, float A1, float A2, float theta1, float theta2, 
                                            Vector3 startPosition, 
                                            Vector3 endPosition,
                                            float maxAngleDeg)
    {
        return CreateRailVectors(L1, L2, A1, A2, theta1, theta2, 1, 1, startPosition, endPosition, maxAngleDeg);
    }

    public static float[] CreateRailVectors(float L1, float L2, float A1, float A2, float theta1, float theta2, float fraction1, float fraction2,
                                            Vector3 startPosition,
                                            Vector3 endPosition,
                                            float maxAngleDeg)
    {
        float[] waypoints;

        float length = L1/A1 + L2/A2;
        float maxAngle = maxAngleDeg * Mathf.Deg2Rad;

        float usedTheta1 = theta1 * (1 - (1-fraction1) * (1-fraction1));
        float usedTheta2 = theta2 * (1 - (1-fraction2) * (1-fraction2));

        float usedPhi = usedTheta1 + usedTheta2;
        float phi = theta1 + theta2;
        float startingTheta = theta1 - usedTheta1;

        int nVectors = Mathf.FloorToInt(usedPhi / maxAngle) + 2;

        waypoints = new float[nVectors];

        float anglePerSection = usedPhi / (nVectors - 1);

        for (int i = 0; i < waypoints.Length; i++)
        {
            float angleForVector = i * anglePerSection + startingTheta;

            if (angleForVector < theta1)
            {
                waypoints[i] = Mathf.Sqrt(angleForVector) / A1;
            }
            else
            {
                if (angleForVector > phi) angleForVector = phi;
                waypoints[i] = length - Mathf.Sqrt(phi - angleForVector) / A2;
            }

            //Debug.Log("rail " + i + ": " + waypoints[i]);
        }

        return waypoints;
    }

}