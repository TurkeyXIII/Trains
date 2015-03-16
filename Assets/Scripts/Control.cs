using UnityEngine;
using System.Collections;

public class Control : MonoBehaviour {
    private static Control control;

    public GameObject gameController;

    private FileHandler fileHandler;
    private TerrainController terrainController;

    public TrackPlacementTool trackPlacer;

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

        
        float L = Mathf.Sqrt(Mathf.PI / 4f);
        Debug.Log("C(0.886) = " + FresnelMath.FresnelC(L));
        Debug.Log("S(0.886) = " + FresnelMath.FresnelS(L));
        
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
