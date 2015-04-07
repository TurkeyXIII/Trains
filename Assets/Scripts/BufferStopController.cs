using UnityEngine;
using System.Collections;

public class BufferStopController : MonoBehaviour {
    private GameObject m_bauble;

	void Start () 
    {
	    VertexCropper trackVertCropper = GetComponentInChildren<VertexCropper>();
        if (trackVertCropper != null)
        {
            Bounds b = new Bounds(new Vector3(3.5f, 0, 0), new Vector3(3, 5, 5));
            trackVertCropper.Crop(b);
        }
	}

    public void Link(GameObject bauble)
    {
        m_bauble = bauble;
        bauble.GetComponent<BaubleController>().AddBufferStop(gameObject);
    }

    public int GetBaubleUID()
    {
        return m_bauble.GetComponent<SaveLoad>().UID;
    }
}
