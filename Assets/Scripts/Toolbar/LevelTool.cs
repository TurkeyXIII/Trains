using UnityEngine;
using System.Collections;

public class LevelTool : MonoBehaviour, ITool {
    private TerrainController c_terrainController;

    void Awake()
    {
        c_terrainController = GameObject.FindGameObjectWithTag("Terrain").GetComponent<TerrainController>();
    }

    public void UpdateWhenSelected()
    {
        c_terrainController.UpdateLevel();
    }


    public void OnDeselect()
    {
    }
}
