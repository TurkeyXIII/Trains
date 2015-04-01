using UnityEngine;
using System.Collections;

public class RaiseLowerTool : Tool {

    private TerrainController c_terrainController;

    void Awake()
    {
        c_terrainController = GameObject.FindGameObjectWithTag("Terrain").GetComponent<TerrainController>();
    }

    public override void UpdateWhenSelected()
    {
        c_terrainController.UpdateRaiseLower();
    }

    public override Effect GetDefaultEffect()
    {
        return Effect.Small;
    }
}
