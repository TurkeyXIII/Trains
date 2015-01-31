using UnityEngine;
using System.Collections;

public class MenuController : MonoBehaviour {

    public void OnQuit()
    {
        Application.Quit();
    }

    public void OnFreePlay()
    {
        Application.LoadLevel(1);
    }
}
