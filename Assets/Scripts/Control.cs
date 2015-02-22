using UnityEngine;
using System.Collections;

public class Control : MonoBehaviour {
    private static Control control;

    public GameObject gameController;
    public GameObject prefabTrackSection;

    private FileHandler fileHandler;
    private TerrainController terrainController;

    void Awake()
    {
        if (control == null)
        {
            DontDestroyOnLoad(this);
            control = this;

            fileHandler = GetComponent<FileHandler>();
        }
        else if (control != this)
        {
            Destroy(this);
        }
    }

    void Start()
    {
        OnLevelWasLoaded(Application.loadedLevel);
    }

    void OnLevelWasLoaded(int level)
    {
        if (terrainController == null)
        {
            GameObject terrain =  GameObject.FindGameObjectWithTag("Terrain");
            if (terrain != null)
                terrainController = terrain.GetComponent<TerrainController>();
        }
    }

    public static Control GetControl()
    {
        return control;
    }

    public FileHandler GetFileHandler()
    {
        return fileHandler;
    }

    public TerrainController GetTerrainController()
    {
        return terrainController;
    }
}
