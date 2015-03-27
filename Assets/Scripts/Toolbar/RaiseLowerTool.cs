using UnityEngine;
using System.Collections;

public class RaiseLowerTool : MonoBehaviour, ITool {

    private TerrainController c_terrainController;

    void Awake()
    {
        c_terrainController = GameObject.FindGameObjectWithTag("Terrain").GetComponent<TerrainController>();
    }

    public void UpdateWhenSelected()
    {
        c_terrainController.UpdateRaiseLower();
    }

    public void OnDeselect()
    {
    }

    public void OnSelect()
    {
    }

    public void OnEffectChange()
    {
    }

    public Effect GetDefaultEffect()
    {
        return Effect.Small;
    }
}
