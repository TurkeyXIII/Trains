using UnityEngine;
using System;
using System.Collections;

public class BufferStopSaveLoad : SaveLoad
{
    private int m_baubleUID;

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
        BufferStopData data = new BufferStopData();

        data.UID = UID;

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

    public override void LoadFromDataObject(IDataObject data)
    {
        Debug.Log("Loading a Buffer Stop");
        BufferStopData bsData = (BufferStopData)data;

        transform.position = new Vector3(bsData.positionX, bsData.positionY, bsData.positionZ);
        transform.rotation = new Quaternion(bsData.rotationX, bsData.rotationY, bsData.rotationZ, bsData.rotationW);

        m_baubleUID = bsData.baubleUID;

        base.LoadUID(bsData);
    }

    public int GetBaubleUID()
    {
        return m_baubleUID;
    }
}

[Serializable()]
public class BufferStopData : DataObjectWithUID
{
    // position data should be redundant as it's stored in the bauble
    public float positionX, positionY, positionZ;
    public float rotationX, rotationY, rotationZ, rotationW;

    public int baubleUID;

    public override GameObject GetPrefab()
    {
        return Control.GetControl().prefabBufferStop;
    }
}