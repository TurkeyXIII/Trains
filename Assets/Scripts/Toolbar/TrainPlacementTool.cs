using UnityEngine;
using System.Collections;

public class TrainPlacementTool : Tool
{
    private GameObject m_currentTrackVehicle;
    private bool m_isDragging;
    private TrackSectionShapeController m_trackSection;

    public float minDragDistance;

    public override Effect GetDefaultEffect()
    {
        return Effect.Locomotive;
    }

    public override void UpdateWhenSelected()
    {
        Vector3 hitLocation;
        Vector3 actionLocation;
        Quaternion actionRotation;
        Collider hit = CameraController.GetMouseHit(out hitLocation);
        GameObject hitObject = null;
        if (hit != null)
        {
            hitObject = hit.gameObject;
            if (hitObject.transform.parent != null)
                hitObject = hitObject.transform.parent.gameObject;
        }

        if (!m_isDragging)
        {
            if (hitObject != null)
            {
                m_trackSection = hitObject.GetComponent<TrackSectionShapeController>();
                if (m_trackSection != null)
                {
                    m_trackSection.FindTrackCentre(hitLocation, out actionLocation, out actionRotation);
                    Control.GetControl().CreateCursorLight();
                    Control.GetControl().SnapCursorLight(actionLocation);

                    if (m_currentTrackVehicle == null)
                    {
                        m_currentTrackVehicle = (GameObject)Instantiate(Control.GetControl().prefabLocomotive);
                        m_currentTrackVehicle.GetComponent<TrackVehicleController>().DisablePhysics();
                    }

                    m_currentTrackVehicle.transform.position = actionLocation;
                    m_currentTrackVehicle.transform.rotation = actionRotation;

                    if (Input.GetMouseButtonDown(0)) m_isDragging = true;
                }
                else
                {
                    Control.GetControl().DestroyCursorLight();
                    if (m_currentTrackVehicle != null) Destroy(m_currentTrackVehicle);
                }
            }
        }
        else
        {
            Vector3 dragDirection = hitLocation - m_currentTrackVehicle.transform.position;
            if (dragDirection.magnitude > minDragDistance && Vector3.Dot(m_currentTrackVehicle.transform.right, dragDirection) < 0)
            {
                m_currentTrackVehicle.transform.rotation *= Quaternion.AngleAxis(180, m_currentTrackVehicle.transform.up);
            }

            if (Input.GetMouseButtonUp(0))
            {
                m_isDragging = false;
                
                LocomotiveController lc = m_currentTrackVehicle.GetComponent<LocomotiveController>();

                lc.currentTrackSection = m_trackSection;
                lc.BeginSimulation();
                lc.EnablePhysics();

                m_currentTrackVehicle = null;
            }
        }
    }

    public override void OnDeselect()
    {
        Control.GetControl().DestroyCursorLight();
        if (m_currentTrackVehicle != null) Destroy(m_currentTrackVehicle);
    }

    public override void OnEffectChange()
    {
        OnDeselect();
    }
}
