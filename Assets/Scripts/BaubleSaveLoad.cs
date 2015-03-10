using UnityEngine;
using System;
using System.Collections;

public class BaubleSaveLoad : MonoBehaviour, ISaveLoadable {

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
        BaubleData data = new BaubleData();

        data.UID = GetComponent<ObjectUID>().UID;

        data.positionX = transform.position.x;
        data.positionY = transform.position.y;
        data.positionZ = transform.position.z;

        data.rotationX = transform.rotation.x;
        data.rotationY = transform.rotation.y;
        data.rotationZ = transform.rotation.z;
        data.rotationW = transform.rotation.w;

        return data;
    }

    public void LoadFromDataObject(IDataObject data)
    {
        BaubleData bData = (BaubleData) data;

        transform.position = new Vector3(bData.positionX, bData.positionY, bData.positionZ);
        transform.rotation = new Quaternion(bData.rotationX, bData.rotationY, bData.rotationZ, bData.rotationW);

        GetComponent<ObjectUID>().LoadFromDataObject(bData);
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }
}

[Serializable()]
public class BaubleData : DataObjectWithUID, IDataObject
{
    public float positionX, positionY, positionZ;
    public float rotationX, rotationY, rotationZ, rotationW;

    public Type GetLoaderType()
    {
        return typeof(BaubleSaveLoad);
    }
}