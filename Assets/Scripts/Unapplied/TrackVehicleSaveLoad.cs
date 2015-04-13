using UnityEngine;
using System;
using System.Collections;

// not sure if this class has a use atm
public abstract class TrackVehicleSaveLoad : SaveLoad
{
    new void Awake()
    {
        base.Awake();
    }

    new void OnDestroy()
    {
        base.OnDestroy();
    }
}

[Serializable]
public abstract class TrackVehicleData : DataObjectWithUID
{
    public int trackUID;

    public float distanceAlongTrack;
    public float velocity;
    public bool isForward;
}