using UnityEngine;
using System.Collections;
using System;

public class TrackSectionSaveLoad : MonoBehaviour, ISaveLoadable
{
    void Start()
    {
        Control.GetControl().GetComponent<FileHandler>().AddToSaveableObjects(this);
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
        TrackSectionData data = new TrackSectionData();

        data.startPointX = transform.position.x;
        data.startPointY = transform.position.y;
        data.startPointZ = transform.position.z;

        Vector3 endPoint = GetComponent<TrackSectionShapeController>().GetEndPoint();

        data.endPointX = endPoint.x;
        data.endPointY = endPoint.y;
        data.endPointZ = endPoint.z;

        return data;
    }

    public void LoadFromDataObject(IDataObject data)
    {
        TrackSectionData tsData = (TrackSectionData)data;
        transform.position = new Vector3(tsData.startPointX, tsData.startPointY, tsData.startPointZ);
        TrackSectionShapeController tslc = GetComponent<TrackSectionShapeController>();
        tslc.Initialize();
        tslc.SetEndPoint(new Vector3(tsData.endPointX, tsData.endPointY, tsData.endPointZ));
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }
}

[Serializable()]
public class TrackSectionData : IDataObject
{
    public float startPointX;
    public float startPointY;
    public float startPointZ;
    public float endPointX;
    public float endPointY;
    public float endPointZ;

    public Type GetLoaderType()
    {
        return typeof(TrackSectionSaveLoad);
    }
}