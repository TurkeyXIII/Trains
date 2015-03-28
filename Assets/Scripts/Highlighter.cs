using UnityEngine;
using System.Collections;

public class Highlighter : MonoBehaviour {

    public Material highlightMaterial;

    private Material m_defaultMaterial;
    private float m_endTime;

    void Awake()
    {
        m_defaultMaterial = renderer.material;
        m_endTime = -1;
    }

    void Update()
    {
        if (m_endTime != -1 && m_endTime <= Time.time)
        {
            RemoveHighlight();
        }
    }

    public void ToggleHighlight()
    {
        if (renderer.material == m_defaultMaterial)
            Highlight();
        else
            RemoveHighlight();
    }

    public void Highlight(float duration)
    {
        Highlight();
        m_endTime = Time.time + duration;
    }

    public void Highlight()
    {
        renderer.material = highlightMaterial;
    }

    public void RemoveHighlight()
    {
        renderer.material = m_defaultMaterial;
        m_endTime = -1;
    }
}
