﻿using UnityEngine;
using System.Collections;
using System;

public class TrackSectionSaveLoad : MonoBehaviour, ISaveLoadable
{
    private int startBaubleUID = -1;
    private int endBaubleUID = -1;


    void Awake()
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

        var tssc = GetComponent<TrackSectionShapeController>();

        Vector3 endPoint = tssc.GetEndPoint();

        data.endPointX = endPoint.x;
        data.endPointY = endPoint.y;
        data.endPointZ = endPoint.z;

        Debug.Log("Saving endpoint as " + endPoint);

        if (tssc.IsStraight())
        {
            data.startOrientationX = 0;
            data.startOrientationY = 0;
            data.startOrientationZ = 0;
            data.startOrientationW = 0;
        }
        else
        {
            data.startOrientationX = transform.rotation.x;
            data.startOrientationY = transform.rotation.y;
            data.startOrientationZ = transform.rotation.z;
            data.startOrientationW = transform.rotation.w;

            Quaternion endRotation = tssc.GetEndRotation();

            data.endOrientationX = endRotation.x;
            data.endOrientationY = endRotation.y;
            data.endOrientationZ = endRotation.z;
            data.endOrientationW = endRotation.w;
        }

            Debug.Log("Saving rotation as " + transform.rotation);

        data.UID = GetComponent<ObjectUID>().UID;

        data.startBaubleUID = tssc.GetStartBauble().GetComponent<ObjectUID>().UID;
        data.endBaubleUID = tssc.GetEndBauble().GetComponent<ObjectUID>().UID;

        return data;
    }

    public void LoadFromDataObject(IDataObject data)
    {
        TrackSectionData tsData = (TrackSectionData)data;
        transform.position = new Vector3(tsData.startPointX, tsData.startPointY, tsData.startPointZ);
        TrackSectionShapeController tssc = GetComponent<TrackSectionShapeController>();
        tssc.Initialize();
        if (tsData.startOrientationW != 0 || tsData.startOrientationX != 0 ||
            tsData.startOrientationY != 0 || tsData.startOrientationZ != 0)
        {
            transform.rotation = new Quaternion(tsData.startOrientationX, tsData.startOrientationY, tsData.startOrientationZ, tsData.startOrientationW);

            tssc.SetCompound();

            Quaternion endRotation = new Quaternion(tsData.endOrientationX, tsData.endOrientationY, tsData.endOrientationZ, tsData.endOrientationW);
            tssc.SetEndRotation(endRotation);
        }
        else
        {
            tssc.SetStraight();
        }

        Debug.Log("Loading rotation as " + transform.rotation);

        Vector3 endpoint = new Vector3(tsData.endPointX, tsData.endPointY, tsData.endPointZ);

        Debug.Log("Loading endpoint as " + endpoint);
        
        tssc.SetEndPoint(endpoint);

        tssc.FinalizeShape();

        Debug.Log("After shape finalization, rotation is " + transform.rotation);

        GetComponent<ObjectUID>().LoadFromDataObject(tsData);

        startBaubleUID = tsData.startBaubleUID;
        endBaubleUID = tsData.endBaubleUID;

        Debug.Log("My endpoint is " + tssc.GetEndPoint());
    }

    public GameObject GetGameObject()
    {
        return gameObject;
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
public class TrackSectionData : DataObjectWithUID, IDataObject
{
    // All this data might be redundant if we're already saving it in the baubles.
    // ***
    public float startPointX;
    public float startPointY;
    public float startPointZ;
    public float endPointX;
    public float endPointY;
    public float endPointZ;

    public float startOrientationX;
    public float startOrientationY;
    public float startOrientationZ;
    public float startOrientationW;

    public float endOrientationX;
    public float endOrientationY;
    public float endOrientationZ;
    public float endOrientationW;
    // ***

    public int startBaubleUID;
    public int endBaubleUID;

    public Type GetLoaderType()
    {
        return typeof(TrackSectionSaveLoad);
    }
}