using UnityEngine;
using System.Collections;

public class LocomotiveController : TrackVehicleController
{
    [Range(-1f, 1f)]
    public float powerPercentage;
    public float maxPower;
    public float drag;

    private bool m_isSimulating;

    void Awake()
    {
        m_isSimulating = false;
    }

    void FixedUpdate()
    {
        if (m_isSimulating)
        {
            float acceleration = maxPower * powerPercentage - drag * m_velocity;

            m_velocity += acceleration * Time.deltaTime;

            TraverseTrack();
        }
    }

    public void BeginSimulation()
    {
        m_isSimulating = true;

        m_velocity = 0;
        m_distanceAlongTrack = currentTrackSection.GetTravelDistance(transform.position); 

        Vector3 dummyPosition;
        Quaternion trackRotation;
        currentTrackSection.GetPositionFromTravelDistance(m_distanceAlongTrack, out dummyPosition, out trackRotation);
        m_isForward = Quaternion.Angle(trackRotation, transform.rotation) < 90;
    }
}
