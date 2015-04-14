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

        if (dialog.FileName != null)
        {
            try
            {
                file = (FileStream)dialog.OpenFile();
            }
            catch (Exception e)
            {
                Debug.Log("dialog.OpenFile() failed with exception " + e.Message);
            }
        }

        if (file != null)
        {
            DeleteSavables();

            while (file.Position < file.Length)
            {
                IDataObject dataObject = formatter.Deserialize(file) as IDataObject;
                System.Type type = dataObject.GetType();

                Debug.Log("found a " + dataObject.GetType());

                if (type == typeof(SerializableTerrainData))
                {
                    if (c_terrainSaveLoad == null)
                    {
                        Debug.Log("SaveLoad == NULL!!!!");
                    }
                    c_terrainSaveLoad.LoadFromDataObject(dataObject);
                }
                else
                {
                    InstantiateObject(dataObject as DataObjectWithUID);
                }
            }

            filename = file.Name;

            file.Close();

            LinkBaublesFromUIDs();

            levelHasChanged = false;
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
                GameObject.Destroy(sl.gameObject);
            }
        }

        Control.GetControl().trackPlacer.ResetLists();
    }


    private void LinkBaublesFromUIDs()
    {
        foreach (GameObject trackSection in Control.GetControl().GetTrackSections())
        {
            TrackSectionShapeController tssc = trackSection.GetComponent<TrackSectionShapeController>();
            TrackSectionSaveLoad tssl = trackSection.GetComponent<TrackSectionSaveLoad>();
            int startUID = tssl.GetStartBaubleUID();
            int endUID = tssl.GetEndBaubleUID();

            //Debug.Log("Finding links for track section #" + tssl.UID + ": " + startUID + ", " + endUID);

            foreach (GameObject bauble in Control.GetControl().GetBaubles())
            {
                int uid = bauble.GetComponent<SaveLoad>().UID;

                //Debug.Log("Checking bauble #" + uid);

                if (uid == startUID)
                {
                    Debug.Log("Linking start of #" + tssl.UID + " to #" + uid);
                    tssc.LinkStart(bauble);
                    startUID = -1;
                }
                else if (uid == endUID)
                {
                    Debug.Log("Linking end of #" + tssl.UID + " to #" + uid);
                    tssc.LinkEnd(bauble);
                    endUID = -1;
                }

                if (startUID < 0 && endUID < 0) break;
            }

            //Debug.Log("After linking, my endpoint is " + tssc.GetEndPoint());
            //Debug.Log("After linking, my rotation is " + trackSection.transform.rotation);

            tssc.FinalizeShape();
        }

        foreach (GameObject bufferStop in Control.GetControl().GetBufferStops())
        {
            int targetUID = bufferStop.GetComponent<BufferStopSaveLoad>().GetBaubleUID();

            foreach (GameObject bauble in Control.GetControl().GetBaubles())
            {
                int uid = bauble.GetComponent<SaveLoad>().UID;
                if (uid == targetUID)
                {
                    bufferStop.GetComponent<BufferStopController>().Link(bauble);
                    break;
                }
            }
        }


        foreach (GameObject locomotive in Control.GetControl().GetLocomotives())
        {
            int targetUID = locomotive.GetComponent<LocomotiveSaveLoad>().GetTrackUID();

            Debug.Log("Locomotive #" + locomotive.GetComponent<SaveLoad>().UID + " searching for track #" + targetUID);

            foreach (GameObject trackSection in Control.GetControl().GetTrackSections())
            {
                int uid = trackSection.GetComponent<SaveLoad>().UID;
                if (uid == targetUID)
                {
                    Debug.Log("#" + targetUID + " found");
                    LocomotiveController lc = locomotive.GetComponent<LocomotiveController>();
                    lc.currentTrackSection = trackSection.GetComponent<TrackSectionShapeController>();
                    lc.ResumeSimulation();
                    break;
                }
            }
        }
    }

    private void InstantiateObject(DataObjectWithUID data)
    {
        GameObject newObject = (GameObject)Instantiate(data.GetPrefab());
        SaveLoad sl = newObject.GetComponent<SaveLoad>();
        sl.LoadFromDataObject(data);
    }
}

