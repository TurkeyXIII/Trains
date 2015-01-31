using UnityEngine;
using System.Collections;

public class Control : MonoBehaviour {
    private static Control control;

    public GameObject gameController;
    public GameObject prefabTrackSection;

    private FileHandler fileHandler;

    void Awake()
    {
        if (control == null)
        {
            DontDestroyOnLoad(this);
            control = this;

            fileHandler = GetComponent<FileHandler>();
        }
        else if (control != this)
        {
            Destroy(this);
        }
    }

    public static Control GetControl()
    {
        return control;
    }

    public FileHandler GetFileHandler()
    {
        return fileHandler;
    }
}
