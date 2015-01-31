using UnityEngine;
using System.Collections;

public class CursorChanger : MonoBehaviour {

	public Texture2D cursor;

    public void Set()
    {
        Cursor.SetCursor(cursor, Vector2.zero, CursorMode.Auto);
    }
}
