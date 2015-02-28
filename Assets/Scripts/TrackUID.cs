using UnityEngine;
using System.Collections;

public class TrackUID : MonoBehaviour {

	private static int UIDcounter = 0;

    public int UID { get; private set; }

    void Awake()
    {
        UID = UIDcounter++;
    }
}
