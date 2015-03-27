using UnityEngine;
using System.Collections;

public class BufferStopController : MonoBehaviour {
    
	void Start () 
    {
	    VertexCropper trackVertCropper = GetComponentInChildren<VertexCropper>();
        if (trackVertCropper != null)
        {
            Bounds b = new Bounds(new Vector3(3.5f, 0, 0), new Vector3(3, 5, 5));
            trackVertCropper.Crop(b);
        }
	}
}
