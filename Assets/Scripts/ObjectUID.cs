using UnityEngine;
using System.Collections;

public class ObjectUID : MonoBehaviour {

	private static int UIDcounter = 0;

    public int UID { get; private set; }

    void Awake()
    {
        UID = UIDcounter++;
    }

    public void LoadFromDataObject(DataObjectWithUID data)
    {
        UID = data.UID;
        if (UID + 1 > UIDcounter)
            UIDcounter = UID + 1;
    }
}
