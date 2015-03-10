using UnityEngine;
using System;
using System.Collections;

public class TerrainSaveLoad : MonoBehaviour, ISaveLoadable
{
    public bool newTerrainOnAwake = true;
    public float newTerrainHeight;
    private TerrainData terrainData;
    private SerializableTerrainData serializable;

    void Awake()
    {
        terrainData = GetComponent<Terrain>().terrainData;

        if (newTerrainOnAwake)
            InitialiseTerrain();

        Control.GetControl().GetComponent<FileHandler>().AddToSaveableObjects(this);
    }

    public void InitialiseTerrain()
    {
        float[,] heightmap = new float[terrainData.heightmapHeight, terrainData.heightmapWidth];

        for (int i = 1; i < terrainData.heightmapHeight - 1; i++)
        {
            for (int j = 1; j < terrainData.heightmapWidth - 1; j++)
            {
                heightmap[i,j] = newTerrainHeight;
            }
        }

        terrainData.SetHeights(0, 0, heightmap);

        float[,,] alphamap = new float[terrainData.alphamapHeight, terrainData.alphamapWidth, terrainData.splatPrototypes.Length];
        for (int i = 0; i < terrainData.alphamapHeight; i++)
        {
            for (int j = 0; j < terrainData.alphamapWidth; j++)
            {
                if (i < 2 || j < 2 || i > terrainData.alphamapHeight - 3 || j > terrainData.alphamapWidth - 3)
                {
                    alphamap[i,j,2] = 1f;
                }
                else
                {
                    alphamap[i,j,0] = 1f;
                }
            }
        }
        terrainData.SetAlphamaps(0, 0, alphamap);
    }

    void OnDestroy()
    {
        Control control = Control.GetControl();
        if (control != null)
        {
            control.GetComponent<FileHandler>().RemoveFromSaveableObjects(this);
        }
    }

    public IDataObject GetDataObject()
    {
        SerializableTerrainData std = new SerializableTerrainData();
        std.heightmap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);
        std.alphamap = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);

        return std as IDataObject;
    }


    public void LoadFromDataObject(IDataObject data)
    {
        SerializableTerrainData std = (SerializableTerrainData) data;

        terrainData.SetHeights(0, 0, std.heightmap);
        terrainData.SetAlphamaps(0, 0, std.alphamap);
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

}

[Serializable()]
internal class SerializableTerrainData : IDataObject
{
    public float[,] heightmap;
    public float[, ,] alphamap;

    public System.Type GetLoaderType()
    {
        return typeof(TerrainSaveLoad);
    }
}