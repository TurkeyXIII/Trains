using UnityEngine;
using System.Collections;

public class TrainPlacementTool : Tool
{
    private GameObject m_currentTrainCar;
    private bool m_isDragging;

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
                TrackSectionShapeController hoveringtssc = hitObject.GetComponent<TrackSectionShapeController>();
                if (hoveringtssc != null)
                {
                    hoveringtssc.FindTrackCenter(hitLocation, out actionLocation, out actionRotation);
                    Control.GetControl().CreateCursorLight();
                    Control.GetControl().SnapCursorLight(actionLocation);

                    if (m_currentTrainCar == null)
                    {
                        m_currentTrainCar = (GameObject)Instantiate(Control.GetControl().prefabLocomotive);
                    }

                    m_currentTrainCar.transform.position = actionLocation;
                    m_currentTrainCar.transform.rotation = actionRotation * Quaternion.Euler(0, -90, 0);

                    if (Input.GetMouseButtonDown(0)) m_isDragging = true;
                    
                }
                else
                {
                    Control.GetControl().DestroyCursorLight();
                    if (m_currentTrainCar != null) Destroy(m_currentTrainCar);
                }
            }
            
        }
        else
        {
            Vector3 dragDirection = hitLocation - m_currentTrainCar.transform.position;
            if (dragDirection.magnitude > minDragDistance && Vector3.Dot(m_currentTrainCar.transform.right, dragDirection) < 0)
            {
                m_currentTrainCar.transform.rotation *= Quaternion.AngleAxis(180, m_currentTrainCar.transform.up);
            }

            if (Input.GetMouseButtonUp(0))
            {
                m_isDragging = false;
                // finalisationg of train car should be done here
                m_currentTrainCar = null;
            }
        }
    }
}
