using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonHighlighter : MonoBehaviour {
    public Color selectedColor = Color.cyan;
    public Color normalColor = Color.white;

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
    }

    public void ToggleHighlight()
    {
        ColorBlock cb = button.colors;

        if (cb.normalColor == selectedColor)
        {
            cb.normalColor = normalColor;
            cb.highlightedColor = normalColor;
        }
        else
        {
            cb.normalColor = selectedColor;
            cb.highlightedColor = selectedColor;
        }

        button.colors = cb;
    }
}
