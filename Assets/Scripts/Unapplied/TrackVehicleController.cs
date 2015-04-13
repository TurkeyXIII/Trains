using UnityEngine;
using System.Collections;

public class TrackVehicleController : MonoBehaviour {

    public TrackSectionShapeController currentTrackSection;

    protected float m_velocity; // +ve for forward, -ve for backward
    protected float m_distanceAlongTrack;
    protected bool m_isForward; // relative to the track orientation

    protected void TraverseTrack()
    {
        Vector3 correctPosition;
        Quaternion correctRotation;

        float travelDistance;
        

        if (m_isForward)
            travelDistance = m_velocity;
        else
            travelDistance = -m_velocity;

        while (travelDistance != 0)
        {
            BaubleController reachedBauble;
            currentTrackSection.Traverse(ref m_distanceAlongTrack, ref travelDistance, out reachedBauble);

            // move the vehicle to the end of the track section
            currentTrackSection.GetPositionFromTravelDistance(m_distanceAlongTrack, out correctPosition, out correctRotation);
            transform.position = correctPosition;
            if (!m_isForward)
                transform.rotation = correctRotation * Quaternion.AngleAxis(180, currentTrackSection.transform.up);
            else
                transform.rotation = correctRotation;

            // find the next track section if necessary
            if (reachedBauble != null)
            {
                // use the bauble to find the next track, 
                // find whether we're forward and if not 
                // our position (the length of the section)
                if (m_velocity > 0)
                    currentTrackSection = reachedBauble.GetTrack(transform.rotation);
                else
                    currentTrackSection = reachedBauble.GetTrack(transform.rotation * Quaternion.AngleAxis(180, transform.up));

                if (currentTrackSection.GetEndBauble() == reachedBauble.gameObject)
                {
                    // we are at the end of the track section.
                    m_isForward = (m_velocity < 0);
                    if (travelDistance > 0) travelDistance = -travelDistance;

                    m_distanceAlongTrack = currentTrackSection.GetLength();
                }
                else
                {
                    // we are at the beginning of the track section.
                    m_isForward = (m_velocity > 0);
                    if (travelDistance < 0) travelDistance = -travelDistance;

                    m_distanceAlongTrack = 0;
                }
            }
            else
            {
                travelDistance = 0; // should already be zero, but set it for equality check
            }
        }
    }

    internal void DisablePhysics()
    {
        GetComponent<Rigidbody>().Sleep();
        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Collider>().enabled = false;
    }

    internal void EnablePhysics()
    {
        GetComponent<Rigidbody>().useGravity = true;
        GetComponent<Collider>().enabled = true;
    }

    internal void FillTrackVehicleData(ref TrackVehicleData tvd)
    {
        tvd.distanceAlongTrack = m_distanceAlongTrack;
        tvd.isForward = m_isForward;
        tvd.velocity = m_velocity;

        Debug.Log("VC: distance = " + tvd.distanceAlongTrack);
    }

    internal void RestoreFromTrackVehicleData(TrackVehicleData tvd)
    {
        m_distanceAlongTrack = tvd.distanceAlongTrack;
        m_isForward = tvd.isForward;
        m_velocity = tvd.velocity;
    }
}
