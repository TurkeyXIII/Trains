using UnityEngine;
using System.Collections;

public class LevelTool : Tool
{
    private TerrainController c_terrainController;

    void Awake()
    {
        c_terrainController = GameObject.FindGameObjectWithTag("Terrain").GetComponent<TerrainController>();
    }

    public override void UpdateWhenSelected()
    {
        c_terrainController.UpdateLevel();
    }
}
