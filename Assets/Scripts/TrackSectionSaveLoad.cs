using UnityEngine;
using System.Collections;
using System;

public class TrackSectionSaveLoad : SaveLoad
{
    private int startBaubleUID = -1;
    private int endBaubleUID = -1;


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
        TrackSectionData data = new TrackSectionData();
        var tssc = GetComponent<TrackSectionShapeController>();
        
        data.UID = UID;

        data.startBaubleUID = tssc.GetStartBauble().GetComponent<SaveLoad>().UID;
        data.endBaubleUID = tssc.GetEndBauble().GetComponent<SaveLoad>().UID;

        return data;
    }

    public override void LoadFromDataObject(IDataObject data)
    {
        TrackSectionData tsData = (TrackSectionData)data;
        
        base.LoadFromDataObject(tsData);

        startBaubleUID = tsData.startBaubleUID;
        endBaubleUID = tsData.endBaubleUID;
    }

    public int GetStartBaubleUID()
    {
        return startBaubleUID;
    }

    public int GetEndBaubleUID()
    {
        return endBaubleUID;
    }
}

[Serializable()]
public class TrackSectionData : DataObjectWithUID
{
    public int startBaubleUID;
    public int endBaubleUID;

    public override Type GetLoaderType()
    {
        return typeof(TrackSectionSaveLoad);
    }
}