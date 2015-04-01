using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class FileHandler : MonoBehaviour {

    private TerrainSaveLoad c_terrainSaveLoad;
    private List<SaveLoad> saveableObjects;

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

        DeleteSavables();

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
            file = new FileStream(filename, FileMode.Create);
        }

        if (file != null)
        {
            foreach (SaveLoad loadable in saveableObjects)
            {
                Debug.Log("Saving a " + loadable.GetType());
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

        

        FileStream file = null;
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

        /*
        try
        {
        */
            if (dialog.FileName != null)
                file = (FileStream)dialog.OpenFile();

            if (file != null)
            {
                DeleteSavables();

                while (file.Position < file.Length)
                {
                    IDataObject dataObject = formatter.Deserialize(file) as IDataObject;
                    System.Type type = dataObject.GetLoaderType();

                    Debug.Log("found a " + dataObject.GetLoaderType());

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
                    else if (type == typeof(BaubleSaveLoad))
                    {
                        Control.GetControl().trackPlacer.InstantiateBauble(dataObject);
                    }
                    else if (type == typeof(BufferStopSaveLoad))
                    {
                        Control.GetControl().trackPlacer.InstantiateBufferStop(dataObject);
                    }
                }

                filename = file.Name;

                file.Close();

                Control.GetControl().trackPlacer.LinkBaublesFromUIDs();

                levelHasChanged = false;
            }
        /*
        }
        catch (Exception e)
        {
            Debug.Log("Caught exception loading file: " + e.Message);
        }
        */
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

    public void AddToSaveableObjects(SaveLoad s)
    {
        if (saveableObjects == null)
        {
            saveableObjects = new List<SaveLoad>();
        }

        if (!saveableObjects.Contains(s))
        {
            saveableObjects.Add(s);
        }
    }

    public void RemoveFromSaveableObjects(SaveLoad s)
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

    private void DeleteSavables()
    {
        foreach (SaveLoad sl in saveableObjects)
        {
            if (sl.GetType() != typeof(TerrainSaveLoad))
            {
                GameObject.Destroy(sl.GetGameObject());
            }
        }

        Control.GetControl().trackPlacer.ResetLists();
    }
}

