using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public enum Effect
{
    None,
    Small,
    Medium,
    Large,
    Track,
    BufferStop
}

public class ToolSelector : MonoBehaviour
{

    private string[] effectNames = 
    {
        "",
        "Small Brush",
        "Medium Brush",
        "Large Brush",
        "Track",
        "Buffer Stop"
    };

    public Texture2D[] toolCursors;
    public GameObject[] subToolBars;

    public Color selectedColor, normalColor;

    private Effect currentEffect;

    private GameObject currentToolButton;
    private ITool currentToolBehaviour;

    public static ToolSelector toolSelector { get; private set;}

    void Awake()
    {
        if (toolSelector == null) toolSelector = this;
        else Destroy(this);

        foreach (GameObject go in subToolBars)
        {
            if (go != null) go.SetActive(false);
        }
        currentEffect = Effect.None;
    }

    void LateUpdate()
    {
        // this is called in lateUpdate to make sure it is always after updates on the items it is called on.
        if (currentToolBehaviour != null && !EventSystem.current.IsPointerOverGameObject())
        {
            currentToolBehaviour.UpdateWhenSelected();
        }
    }

    public void OnSelectUtilities()
    {
        OnToolSelect<UtilitiesTool>();
    }
	
    public void OnSelectRaiseLower()
    {
        OnToolSelect<RaiseLowerTool>();
    }

    public void OnSelectLevel()
    {
        OnToolSelect<LevelTool>();
    }

    public void OnSelectSmooth()
    {
        OnToolSelect<SmoothTool>();
    }

    public void OnSelectTrack()
    {
        OnToolSelect<TrackPlacementTool>();
    }

    public void OnSelectSmall()
    {
        OnEffectSelect(Effect.Small);
    }

    public void OnSelectMedium()
    {
        OnEffectSelect(Effect.Medium);
    }

    public void OnSelectLarge()
    {
        OnEffectSelect(Effect.Large);
    }

    public void OnSelectLayTrack()
    {
        OnEffectSelect(Effect.Track);
    }

    public void OnSelectBufferStop()
    {
        OnEffectSelect(Effect.BufferStop);
    }

    public Effect GetEffect()
    {
        return currentEffect;
    }

    private void OnToolSelect(GameObject button, ITool tool)
    {
        if (currentToolButton == button)
        {
            Deselect();
        }
        else
        {
            Deselect();

            tool.OnSelect();

            currentToolButton = button;
            currentToolBehaviour = tool;

            ToggleHighlightOnCurrentTool();

            button.GetComponent<CursorChanger>().Set();

            //Toggle the sub-toolbar for this tool if there is one
            SubToolbarToggle toggle;
            foreach (Transform t in button.transform)
            {
                toggle = t.gameObject.GetComponent<SubToolbarToggle>();
                if (toggle != null)
                {
                    toggle.Toggle();
                    if (!ToggleHighlightOnCurrentEffect())
                    {
                        currentEffect = tool.GetDefaultEffect();
                        ToggleHighlightOnCurrentEffect();
                    }
                    break;
                }
            }
        }
    }

    private void OnToolSelect<T>()
        where T: MonoBehaviour, ITool
    {
        T tool;

        tool = GetComponentInChildren<T>();

        if (tool == null)
        {
            Debug.Log("Error: " + typeof(T).ToString() + " not found");
        }
        else
        {
            OnToolSelect(tool.gameObject, tool);
        }
    }

    private void OnEffectSelect(Effect effect)
    {
        ToggleHighlightOnCurrentEffect();

        currentToolBehaviour.OnEffectChange();

        currentEffect = effect;

        ToggleHighlightOnCurrentEffect();
    }

    private void Deselect()
    {
        if (currentToolButton == null) return;

        SubToolbarToggle toggle = currentToolButton.GetComponentInChildren<SubToolbarToggle>();
        if (toggle != null)
        {
            ToggleHighlightOnCurrentEffect();
            toggle.Toggle();
        }

        ToggleHighlightOnCurrentTool();

        currentToolBehaviour.OnDeselect();

        currentToolButton = null;
        currentToolBehaviour = null;

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
     // returns true if a highlight was successfully toggled
    private bool ToggleHighlightOnCurrentEffect()
    {
        ButtonHighlighter[] highlighters = currentToolButton.GetComponentsInChildren<ButtonHighlighter>();

        for (int i = 1; i < highlighters.Length; i++)
        {
            if (highlighters[i].gameObject.name == effectNames[(int)currentEffect])
            {
                highlighters[i].ToggleHighlight();

                return true;
            }

        }
        return false;
    }

    private void ToggleHighlightOnCurrentTool()
    {
        if (currentToolButton == null) return;

        ButtonHighlighter highlighter = currentToolButton.GetComponent<ButtonHighlighter>();

        highlighter.ToggleHighlight();
    }

    void OnDestroy()
    {
        if (toolSelector == this) toolSelector = null;
    }

}

public interface ITool
{
    Effect GetDefaultEffect();
    void UpdateWhenSelected();
    void OnSelect();
    void OnDeselect();
    void OnEffectChange();
}