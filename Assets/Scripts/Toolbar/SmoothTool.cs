using UnityEngine;
using System.Collections;

public class SmoothTool : MonoBehaviour, ITool {
    private TerrainController c_terrainController;

    void Awake()
    {
        c_terrainController = GameObject.FindGameObjectWithTag("Terrain").GetComponent<TerrainController>();
    }

    public void UpdateWhenSelected()
    {
        c_terrainController.UpdateSmooth();
    }


    public void OnDeselect()
    {
    }
}
