using UnityEngine;
using System;
using System.Collections;

public class BaubleSaveLoad : SaveLoad
{

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
        BaubleData data = new BaubleData();

        data.UID = UID;

        data.positionX = transform.position.x;
        data.positionY = transform.position.y;
        data.positionZ = transform.position.z;

        data.rotationX = transform.rotation.x;
        data.rotationY = transform.rotation.y;
        data.rotationZ = transform.rotation.z;
        data.rotationW = transform.rotation.w;

        return data;
    }

    public override void LoadFromDataObject(IDataObject data)
    {
        Debug.Log("Loading a Bauble");
        BaubleData bData = (BaubleData) data;

        transform.position = new Vector3(bData.positionX, bData.positionY, bData.positionZ);
        transform.rotation = new Quaternion(bData.rotationX, bData.rotationY, bData.rotationZ, bData.rotationW);

        LoadUID(bData);

        gameObject.SetActive(false);
    }
}

[Serializable()]
public class BaubleData : DataObjectWithUID
{
    public float positionX, positionY, positionZ;
    public float rotationX, rotationY, rotationZ, rotationW;

    public override Type GetLoaderType()
    {
        return typeof(BaubleSaveLoad);
    }

    public override GameObject GetPrefab()
    {
        return Control.GetControl().prefabBauble;
    }
}