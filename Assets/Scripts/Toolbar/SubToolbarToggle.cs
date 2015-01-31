using UnityEngine;
using System.Collections;

public class SubToolbarToggle : MonoBehaviour {

	void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Toggle()
    {
        Debug.Log("Toggling Subtoolbar " + gameObject.name);
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
