using UnityEngine;
using System.Collections;

//TODO: LERP camera movements for smoothness

public class CameraController : MonoBehaviour , ICameraView
{

    public float zoomSensitivity = 2f;
    public float zoomSmoothing = 4f;

    public float maxCameraTilt, minCameraTilt;

    public float visibilityAroundTable;

    public float minHeightAboveTerrain = 0.1f;

    private Vector3 m_targetPosition;
    private bool m_isPanning;
    private bool m_isTilting;
    private Vector3 m_panAnchor;
    private Vector3 m_tiltAnchor;

    private Vector3 m_terrainMin, m_terrainMax;
    private static Terrain m_sTerrain;
   
    private Clamper c_clamper;

    void Awake()
    {
        m_isPanning = false;
    }

    void Start()
    {
        m_sTerrain = GameObject.FindGameObjectWithTag("Terrain").GetComponent<Terrain>();

        m_terrainMin = m_sTerrain.transform.position;
        m_terrainMax = m_terrainMin + m_sTerrain.terrainData.size;
        m_terrainMax.y = 0;

        m_terrainMin.x -= visibilityAroundTable;
        m_terrainMin.z -= visibilityAroundTable;
        m_terrainMax.x += visibilityAroundTable;
        m_terrainMax.z += visibilityAroundTable;

        m_targetPosition = transform.position;

        c_clamper = new Clamper(m_terrainMin, m_terrainMax, minHeightAboveTerrain);
        c_clamper.cameraView = this;
    }

    void Update()
    {
        if (!m_isPanning && Input.GetMouseButtonDown(2))
        {
            StartPanning();
        }

        if (m_isPanning && Input.GetMouseButtonUp(2))
        {
            StopPanning();
        }

        if (m_isPanning)
        {
            Pan();
        }

        AdjustZoom();

        if (!m_isTilting && Input.GetButtonDown("Tilt"))
        {
            StartTilting();
        }

        if (m_isTilting && Input.GetButtonUp("Tilt"))
        {
            StopTilting();
        }

        if (m_isTilting)
        {
            Tilt();
        }

        MoveCamera(Vector3.Lerp(transform.position, m_targetPosition, Time.deltaTime * zoomSmoothing));
    }

    // this function checks whether we'd be inside something before moving
    private void MoveCamera(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        Ray ray = new Ray(transform.position, direction);
        if (!Physics.Raycast(ray, direction.magnitude))
        {
            transform.position = targetPosition;
        }
    }

    private void StartPanning()
    {
        if (m_isTilting) return;

        m_isPanning = GetMouseHitLocation(out m_panAnchor);
        if (m_isPanning)
        {
//            Debug.Log("Panning Started");
            
        }
    }

    private void StopPanning()
    {
//        Debug.Log("Panning Stopped");
        m_isPanning = false;
        /*
        Vector3 newPosition = transform.position;
        c_clamper.ClampPosition(ref newPosition);
        transform.position = newPosition;
        */
    }

    private void Pan()
    {
        //move the camera such that the mouse is over the anchor
        Vector3 currentMouseLocation;
        if (GetMouseHitLocation(out currentMouseLocation))
        {
            Vector3 movement = m_panAnchor - currentMouseLocation;

            movement.y = 0f;

            Vector3 newPosition = transform.position + movement;
            c_clamper.ClampPosition(ref newPosition);

            Vector3 oldPosition = transform.position;
            MoveCamera(newPosition);
            m_targetPosition += transform.position - oldPosition;
        }
        else
        {
            //I guess we've suddenly gone way off terrain; do nothing for now
            //maybe make an approximation
        }        
    }

    public static Collider GetMouseHit(out Vector3 location)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            location = hit.point;
            return hit.collider;
        }
        location = Vector3.zero;
        return null;
    }

    public static bool GetMouseHitLocation(out Vector3 location)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        return GetLocationFromRay(ray, out location);
    }

    public static bool GetScreenCentreLocation(out Vector3 location)
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        return GetLocationFromRay(ray, out location);
    }

    public static bool GetMouseHitTerrainLocation(out Vector3 location)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (m_sTerrain.collider.Raycast(ray, out hit, 100))
        {
            location = hit.point;
            return true;
        }
        location = Vector3.zero;
        return false;
    }

    private static bool GetLocationFromRay(Ray ray, out Vector3 location)
    {
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            location = hit.point;
            return true;
        }
        location = Vector3.zero;
        return false;
    }

    private void AdjustZoom()
    {
        Vector3 mouseLocation;
        Vector3 mouseDirection = transform.forward;

        if (Input.mouseScrollDelta != Vector3.zero)
        {
            if (GetMouseHitLocation(out mouseLocation))
            {
                mouseDirection = (mouseLocation - transform.position).normalized;
            }

            m_targetPosition += mouseDirection * Input.mouseScrollDelta.y * zoomSensitivity;
            c_clamper.ClampPosition(ref m_targetPosition);
        }

    }

    private void StartTilting()
    {
        if (m_isPanning) return;

        if (GetScreenCentreLocation(out m_tiltAnchor))
        {
            m_isTilting = true;
            Screen.lockCursor = true;

        }
    }

    private void StopTilting()
    {
        m_isTilting = false;
        Screen.lockCursor = false;

        c_clamper.ClampPosition(ref m_targetPosition);
    }

    private void Tilt()
    {
        Vector3 PosRel = transform.position - m_tiltAnchor;

        //convert to polar coords and rotate by mouse input
        Polar PolarPosRel = new Polar(PosRel);

        PolarPosRel.elevation -= Input.GetAxis("Mouse Y");
        PolarPosRel.elevation = Mathf.Min(maxCameraTilt, Mathf.Max(minCameraTilt, PolarPosRel.elevation));

        PolarPosRel.rotation -= Input.GetAxis("Mouse X");

        //convert back to cartesian and move camera
        Vector3 newPosition = PolarPosRel.ToVector3() + m_tiltAnchor;
        Vector3 oldPosition = transform.position;
        MoveCamera(newPosition);
        m_targetPosition += transform.position - oldPosition;

        //make sure the camera is looking at the tilt anchor
        transform.LookAt(m_tiltAnchor);

        /*
        if (c_clamper.ClampPosition(ref newPosition))
        {
            transform.position = OldPosition;
            transform.LookAt(m_tiltAnchor);

            m_targetPosition = OldTargetPosition;
        }
        */
    }

    public float GetTerrainHeight(Vector3 position)
    {
        return m_sTerrain.SampleHeight(position);
    }

    public Vector3 ViewportToWorldPoint(Vector3 position, Vector3 viewPort)
    {
        if (position == transform.position)
            return camera.ViewportToWorldPoint(viewPort);

        Vector3 actualCameraPosition = transform.position;
        transform.position = position;
        Vector3 worldPoint= camera.ViewportToWorldPoint(viewPort);
        transform.position = actualCameraPosition;
        return worldPoint;
    }

    public float AngleFromVertical()
    {
        return 90 - transform.eulerAngles.x;
    }

    public float FieldOfView()
    {
        return camera.fieldOfView;
    }
}

public struct Polar
{
    public float radius;
    public float rotation;
    public float elevation;

    public Polar(Vector3 v)
    {
        radius = v.magnitude;
        elevation = Mathf.Asin(v.y / v.magnitude) * Mathf.Rad2Deg;
        rotation = Mathf.Acos(v.x / Mathf.Sqrt(Mathf.Pow(v.x, 2) + Mathf.Pow(v.z, 2))) * Mathf.Rad2Deg;
        if (v.z < 0) rotation = 360 - rotation;
    }

    public Vector3 ToVector3()
    {
        Vector3 v;
        float xzRadius;
        v.y = radius * Mathf.Sin(elevation * Mathf.Deg2Rad);

        if (elevation > 89.9f || elevation < -89.9f)
        {
            v.x = 0;
            v.z = 0;
        }
        else
        {
            xzRadius = Mathf.Sqrt(Mathf.Pow(radius, 2) - Mathf.Pow(v.y, 2));
            v.z = xzRadius * Mathf.Sin(rotation * Mathf.Deg2Rad);
            v.x = xzRadius * Mathf.Cos(rotation * Mathf.Deg2Rad);
        }

        return v;
    }

}

public class Clamper
{
    private Vector3 m_terrainMin, m_terrainMax;
    private float m_yMin;

    public ICameraView cameraView { set; private get; }

    public Clamper(Vector3 min, Vector3 max, float yMin)
    {
        m_terrainMin = min;
        m_terrainMax = max;
        m_yMin = yMin;
    }

    public bool ClampPosition(ref Vector3 cameraPosition)
    {
        //float cosFOV = Mathf.Cos(cameraView.FieldOfView() / 2f * Mathf.Deg2Rad);
        float distToMid = cameraPosition.y / Mathf.Cos(cameraView.AngleFromVertical() * Mathf.Deg2Rad);
        //float distToBottomPlane = cameraPosition.y / Mathf.Cos((cameraView.AngleFromVertical() - cameraView.FieldOfView() / 2f) * Mathf.Deg2Rad) * cosFOV;

        //Vector3 bottomLeft = cameraView.ViewportToWorldPoint(cameraPosition, new Vector3(0, 0f, distToBottomPlane));
        //Vector3 bottomRight = cameraView.ViewportToWorldPoint(cameraPosition, new Vector3(1, 0f, distToBottomPlane));
        Vector3 middle = cameraView.ViewportToWorldPoint(cameraPosition, new Vector3(0.5f, 0.5f, distToMid));
        //Vector3 middle = cameraView.RaycastHit(cameraPosition);

        //Debug.Log(middle.ToString());

        //ApplyClampForLocation(bottomLeft, ref cameraPosition);
        //ApplyClampForLocation(bottomRight, ref cameraPosition);
        return ApplyClampForLocation(middle, ref cameraPosition);
    }

    private bool ApplyClampForLocation(Vector3 testLocation, ref Vector3 newCameraPos)
    {
        bool clamped = false;

        Vector3 min, max;

        float tanFOV = Mathf.Tan(cameraView.FieldOfView() / 2 * Mathf.Deg2Rad);

        float maxY = (Mathf.Min((m_terrainMax.x - m_terrainMin.x), (m_terrainMax.z - m_terrainMin.z)) / (2 * tanFOV)) * Mathf.Cos(cameraView.AngleFromVertical() * Mathf.Deg2Rad) - 0.2f;

        //Debug.Log(maxY);

        if (newCameraPos.y > maxY)
        {
            newCameraPos.y = maxY;
            clamped = true;
        }
        float terrainHeight = cameraView.GetTerrainHeight(newCameraPos);
        if (newCameraPos.y - terrainHeight < m_yMin)
        {
            newCameraPos.y = terrainHeight + m_yMin;
            clamped = true;
        }

        float margin = newCameraPos.y * tanFOV;

        min = m_terrainMin + new Vector3(margin, 0, margin);
        max = m_terrainMax - new Vector3(margin, 0, margin);

        if (testLocation.x > max.x)
        {
            newCameraPos.x += max.x - testLocation.x;
            clamped = true;
        }
        else if (testLocation.x < min.x)
        {
            newCameraPos.x += min.x - testLocation.x;
            clamped = true;
        }

        if (testLocation.z > max.z)
        {
            newCameraPos.z += max.z - testLocation.z;
            clamped = true;
        }
        else if (testLocation.z < min.z)
        {
            newCameraPos.z += min.z - testLocation.z;
            clamped = true;
        }
        return clamped;
    }
}

public interface ICameraView
{
    Vector3 ViewportToWorldPoint(Vector3 position, Vector3 viewPort);
    float GetTerrainHeight(Vector3 position);
    float AngleFromVertical();
    float FieldOfView();
}