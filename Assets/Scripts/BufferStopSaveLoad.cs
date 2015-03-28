using UnityEngine;
using System;
using System.Collections;

public class BufferStopSaveLoad : MonoBehaviour, ISaveLoadable
{
    private int m_baubleUID;

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
        BufferStopData data = new BufferStopData();

        data.UID = GetComponent<ObjectUID>().UID;

        data.positionX = transform.position.x;
        data.positionY = transform.position.y;
        data.positionZ = transform.position.z;

        data.rotationX = transform.rotation.x;
        data.rotationY = transform.rotation.y;
        data.rotationZ = transform.rotation.z;
        data.rotationW = transform.rotation.w;

        data.baubleUID = GetComponent<BufferStopController>().GetBaubleUID();

        return data;
    }

    public void LoadFromDataObject(IDataObject data)
    {
        BufferStopData bsData = (BufferStopData)data;

        transform.position = new Vector3(bsData.positionX, bsData.positionY, bsData.positionZ);
        transform.rotation = new Quaternion(bsData.rotationX, bsData.rotationY, bsData.rotationZ, bsData.rotationW);

        m_baubleUID = bsData.baubleUID;

        GetComponent<ObjectUID>().LoadFromDataObject(bsData);
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public int GetBaubleUID()
    {
        return m_baubleUID;
    }
}

[Serializable()]
public class BufferStopData : DataObjectWithUID, IDataObject
{
    public float positionX, positionY, positionZ;
    public float rotationX, rotationY, rotationZ, rotationW;

    public int baubleUID;

    public Type GetLoaderType()
    {
        return typeof(BufferStopSaveLoad);
    }
}