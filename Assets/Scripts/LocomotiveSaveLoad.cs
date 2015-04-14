using UnityEngine;
using System;
using System.Collections;

public class LocomotiveSaveLoad : TrackVehicleSaveLoad
{
    int trackUID = -1;

    new void Awake()
    {
        base.Awake();
    }

    new void OnDestroy()
    {
        base.OnDestroy();
    }

    public override IDataObject GetDataObject()
    {
        LocomotiveController lc = GetComponent<LocomotiveController>();
        LocomotiveData data = new LocomotiveData();

        TrackVehicleData tvd = data;

        lc.FillTrackVehicleData(ref tvd);

        Debug.Log("SL: distance = " + data.distanceAlongTrack);
        
        data.power = lc.powerFraction;

        data.trackUID = lc.currentTrackSection.GetComponent<SaveLoad>().UID;

        data.UID = UID;

        return data;
    }

    public override void LoadFromDataObject(IDataObject data)
    {
        Debug.Log("Loading a Locomotive");
        LocomotiveController lc = GetComponent<LocomotiveController>();
        LocomotiveData ld = (LocomotiveData)data;

        LoadUID(ld);

        trackUID = ld.trackUID;

        lc.powerFraction = ld.power;

        lc.RestoreFromTrackVehicleData(ld);
    }

    internal int GetTrackUID()
    {
        return trackUID;
    }
}

[Serializable]
public class LocomotiveData : TrackVehicleData
{
    public float power;

    public override GameObject GetPrefab()
    {
        return Control.GetControl().prefabLocomotive;
    }
}