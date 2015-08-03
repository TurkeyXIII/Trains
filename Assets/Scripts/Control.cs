using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Control : MonoBehaviour {
    private static Control control;

    private FileHandler m_fileHandler;
    private TerrainController m_terrainController;

    private LinkedList<GameObject> m_trackSections;
    private LinkedList<GameObject> m_bufferStops;
    private LinkedList<GameObject> m_baubles;
    private LinkedList<GameObject> m_Locomotives;
    
    private GameObject m_cursorLight;

    internal TrackPlacementTool trackPlacer;

    public GameObject prefabTrackSection;
    public GameObject prefabBufferStop;
    public GameObject prefabBauble;
    public GameObject prefabLocomotive;
    public GameObject prefabCursorLight;

    public float cursorLightHeight;

    void Awake()
    {
        if (control == null)
        {
            DontDestroyOnLoad(this);
            control = this;

            m_fileHandler = GetComponent<FileHandler>();
        }
        else if (control != this)
        {
            Destroy(this);
        }

        
        float L = Mathf.Sqrt(Mathf.PI/2);
        Debug.Log("C(sqrt(PI/2)) = " + FresnelMath.FresnelC(L));
        Debug.Log("S(sqrt(PI/2)) = " + FresnelMath.FresnelS(L));
        
    }

    void Start()
    {
        OnLevelWasLoaded(Application.loadedLevel);
    }

    void Update()
    {
        if (m_cursorLight != null) AdjustCursorLight();
    }

    void OnLevelWasLoaded(int level)
    {
        if (m_terrainController == null)
        {
            GameObject terrain =  GameObject.FindGameObjectWithTag("Terrain");
            if (terrain != null)
                m_terrainController = terrain.GetComponent<TerrainController>();
        }
    }

    public static Control GetControl()
    {
        return control;
    }

    public FileHandler GetFileHandler()
    {
        return m_fileHandler;
    }

    public TerrainController GetTerrainController()
    {
        return m_terrainController;
    }

    // This function is redundant now
    public ToolSelector GetToolSelector()
    {
        return ToolSelector.toolSelector;
    }

    public void AddToLists(SaveLoad sl)
    {
        m_fileHandler.AddToSaveableObjects(sl);
        
        LinkedList<GameObject> list = GetListForSaveLoad(sl);
        if (list != null)
        {
            list.AddLast(sl.gameObject);
        }
    }

    public void RemoveFromLists(SaveLoad sl)
    {
        m_fileHandler.RemoveFromSaveableObjects(sl);

        LinkedList<GameObject> list = GetListForSaveLoad(sl);
        if (list != null)
        {
            list.Remove(list.FindLast(sl.gameObject));
        }
    }

    private LinkedList<GameObject> GetListForSaveLoad(SaveLoad sl)
    {
        Type type = sl.GetType();
        if (type == typeof(TrackSectionSaveLoad)) return InstantiatedList(ref m_trackSections);
        if (type == typeof(BaubleSaveLoad)) return InstantiatedList(ref m_baubles);
        if (type == typeof(BufferStopSaveLoad)) return InstantiatedList(ref m_bufferStops);
        if (type == typeof(LocomotiveSaveLoad)) return InstantiatedList(ref m_Locomotives);

        return null;
    }

    private LinkedList<GameObject> InstantiatedList(ref LinkedList<GameObject> list)
    {
        if (list == null) list = new LinkedList<GameObject>();

        return list;
    }

    public IEnumerable<GameObject> GetBaubles()
    {
        return InstantiatedList(ref m_baubles);
    }

    public IEnumerable<GameObject> GetBufferStops()
    {
        return InstantiatedList(ref m_bufferStops);
    }

    public IEnumerable<GameObject> GetTrackSections()
    {
        return InstantiatedList(ref m_trackSections);
    }

    public IEnumerable<GameObject> GetLocomotives()
    {
        return InstantiatedList(ref m_Locomotives);
    }

    public void CreateCursorLight(float size = 0.2f)
    {
        if (m_cursorLight == null)
        {
            m_cursorLight = (GameObject)Instantiate(prefabCursorLight);
            Light l = m_cursorLight.GetComponent<Light>();
            l.range = size;

            AdjustCursorLight();
        }
    }

    public void DestroyCursorLight()
    {
        if (m_cursorLight != null)
        {
            Destroy(m_cursorLight);
            m_cursorLight = null;
        }
    }

    public void SnapCursorLight(Vector3 position)
    {
        if (m_cursorLight != null)
        {
            m_cursorLight.transform.position = position + cursorLightHeight * Vector3.up;
        }
    }

    private void AdjustCursorLight()
    {
        Vector3 mousePosition;
        if (CameraController.GetMouseHitTerrainLocation(out mousePosition))
            m_cursorLight.transform.position = mousePosition + cursorLightHeight * Vector3.up;
    }
}
