using UnityEngine;
using System.Collections;

public class Highlighter : MonoBehaviour {

    public enum HighlightMaterial
    {
        InvalidRed
    }

    public Material[] highlightMaterials; // must be the same order as enum above

    private Material m_defaultMaterial;
    private float m_endTime;

    void Awake()
    {
        m_defaultMaterial = GetComponent<Renderer>().material;
        m_endTime = -1;
    }

    void Update()
    {
        if (m_endTime != -1 && m_endTime <= Time.time)
        {
            RemoveHighlight();
        }
    }

    public void ToggleHighlight(HighlightMaterial mat)
    {
        if (GetComponent<Renderer>().material == m_defaultMaterial)
            Highlight(mat);
        else
            RemoveHighlight();
    }

    public void Highlight(HighlightMaterial mat, float duration)
    {
        Highlight(mat);
        m_endTime = Time.time + duration;
    }

    public void Highlight(HighlightMaterial mat)
    {
        GetComponent<Renderer>().material = highlightMaterials[(int)mat];
        m_endTime = -1;
    }

    public void RemoveHighlight()
    {
        GetComponent<Renderer>().material = m_defaultMaterial;
        m_endTime = -1;
    }
}
