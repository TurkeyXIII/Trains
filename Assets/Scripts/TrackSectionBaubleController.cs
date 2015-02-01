using UnityEngine;
using System.Collections;

public class TrackSectionBaubleController : MonoBehaviour {

	private bool m_mouseIsOver;
    private bool m_isHightlighted;

    public Material hightlightMaterial;
    private Material m_normalMaterial;

    void Awake()
    {
        m_isHightlighted = false;
        m_mouseIsOver = false;
        m_normalMaterial = gameObject.renderer.material;
    }

    void Update()
    {
        if (m_mouseIsOver != m_isHightlighted)
        {
            if (m_mouseIsOver)
            {
                gameObject.renderer.material = hightlightMaterial;
            }
            else
            {
                gameObject.renderer.material = m_normalMaterial;
            }

            m_isHightlighted = m_mouseIsOver;
        }

        m_mouseIsOver = false;
    }

    public void OnMouseover()
    {
        m_mouseIsOver = true;
    }
}
