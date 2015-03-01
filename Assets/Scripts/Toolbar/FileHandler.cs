using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

public class FileHandler : MonoBehaviour {

    private TerrainSaveLoad c_terrainSaveLoad;
    private List<ISaveLoadable> saveableObjects;

    private string filename;

    private bool levelHasChanged;

    void OnLevelWasLoaded(int level)
    {
        if (level > 0)
        {
            c_terrainSaveLoad = GameObject.FindGameObjectWithTag("Terrain").GetComponent<TerrainSaveLoad>();
            Debug.Log("Assigning Terrain loader");

            //saveLoad.InitialiseTerrain();
            levelHasChanged = false;
            filename = null;
        }
    }

    void Awake()
    {
        OnLevelWasLoaded(1);
    }

    public void OnSelectNew()
    {
        if (!YesIAmSure()) return;

        c_terrainSaveLoad.InitialiseTerrain();

        foreach (ISaveLoadable sl in saveableObjects)
        {
            if (sl.GetType() != typeof(TerrainSaveLoad))
            {
                GameObject.Destroy(sl.GetGameObject());
            }
        }

        levelHasChanged = false;
        filename = null;
    }

    public void OnSelectSave()
    {
        FileStream file;
        BinaryFormatter formatter = new BinaryFormatter();

        if (filename == null)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.RestoreDirectory = true;
            dialog.InitialDirectory = UnityEngine.Application.persistentDataPath;
            dialog.DefaultExt = "fpm";
            dialog.Title = "Save Free Play Map";
            dialog.OverwritePrompt = true;
            dialog.Filter = "Free Play maps (*.fpm) | *.fpm";
            dialog.FilterIndex = 1;
            dialog.AddExtension = true;
            dialog.ShowDialog();

            file = (FileStream)dialog.OpenFile();
        }
        else
        {
            file = new FileStream(filename, FileMode.OpenOrCreate);
        }

        if (file != null)
        {
            foreach (ISaveLoadable loadable in saveableObjects)
            {
                formatter.Serialize(file, loadable.GetDataObject());
            }
            filename = file.Name;

            file.Close();

            levelHasChanged = false;
        }

    }

    public void OnSelectLoad()
    {
        if (!YesIAmSure()) return;

        FileStream file;
        BinaryFormatter formatter = new BinaryFormatter();

        OpenFileDialog dialog = new OpenFileDialog();
        dialog.RestoreDirectory = true;
        dialog.InitialDirectory = UnityEngine.Application.persistentDataPath;
        dialog.DefaultExt = "fpm";
        dialog.Title = "Load Free Play Map";
        dialog.Filter = "Free Play maps (*.fpm) | *.fpm";
        dialog.FilterIndex = 1;
        dialog.AddExtension = true;
        dialog.ShowDialog();

        try
        {

            file = (FileStream)dialog.OpenFile();

            if (file != null)
            {
                while (file.Position < file.Length)
                {
                    IDataObject dataObject = formatter.Deserialize(file) as IDataObject;
                    System.Type type = dataObject.GetLoaderType();

                    Debug.Log("found something");

                    if (type == typeof(TerrainSaveLoad))
                    {
                        if (c_terrainSaveLoad == null)
                        {
                            Debug.Log("SaveLoad == NULL!!!!");
                        }
                        c_terrainSaveLoad.LoadFromDataObject(dataObject);
                    }
                    else if (type == typeof(TrackSectionSaveLoad))
                    {
                        Control.GetControl().trackPlacer.InstantiateTrackSection(dataObject);
                    }
                }

                filename = file.Name;

                file.Close();

                levelHasChanged = false;
            }
        }
        catch
        {
            //meh
        }
    }

    public void OnSelectQuit()
    {
        if (!YesIAmSure()) return;

        if (UnityEngine.Application.loadedLevel != 0)
        {
            UnityEngine.Application.LoadLevel(0);
        }
        else
        {
            UnityEngine.Application.Quit();
        }
    }

    public void AddToSaveableObjects(ISaveLoadable s)
    {
        if (saveableObjects == null)
        {
            saveableObjects = new List<ISaveLoadable>();
        }

        if (!saveableObjects.Contains(s))
        {
            saveableObjects.Add(s);
        }
    }

    public void RemoveFromSaveableObjects(ISaveLoadable s)
    {
        saveableObjects.Remove(s);
    }

    public void LevelHasChanged()
    {
        levelHasChanged = true;
    }

    private bool YesIAmSure()
    {
        if (levelHasChanged)
        {
            DialogResult dr;
            dr = MessageBox.Show("Do you want to save your progress before continuing?", "Save?", MessageBoxButtons.YesNoCancel);

            if (dr == DialogResult.Yes)
            {
                OnSelectSave();
            }
            else if (dr == DialogResult.Cancel)
            {
                return false;
            }
        }
        return true;
    }
}

public interface ISaveLoadable
{
    IDataObject GetDataObject();
    void LoadFromDataObject(IDataObject data);
    GameObject GetGameObject();
}

public interface IDataObject
{
    System.Type GetLoaderType();
}

[Serializable()]
public class DataObjectWithUID
{
    public int UID;
}