using UnityEngine;
using System;
using System.Collections;


public abstract class SaveLoad : MonoBehaviour
{
    private static int UIDcounter = 0;

    public int publicID;

    public int UID {get; private set;}


    // subclasses must call base.Awake() from their awake function
    public void Awake()
    {
        UID = UIDcounter++;
        publicID = UID;
        Control.GetControl().AddToLists(this);
    }

    // subclasses must call base.OnDestroy from their onDestroy function
    public void OnDestroy()
    {
        Control control = Control.GetControl();
        if (control != null)
        {
            control.RemoveFromLists(this);
        }
    }

    protected void LoadUID(DataObjectWithUID data)
    {
        UID = data.UID;
        if (UID + 1 > UIDcounter)
            UIDcounter = UID + 1;
        publicID = UID;
    }

    public abstract IDataObject GetDataObject();
    public abstract void LoadFromDataObject(IDataObject data);
}

public interface IDataObject
{
}

[Serializable()]
public abstract class DataObjectWithUID : IDataObject
{
    public int UID;

    public abstract GameObject GetPrefab();
}