using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public enum EffectSize
{
    Small,
    Medium,
    Large
}

public class ToolSelector : MonoBehaviour {

    public Texture2D[] toolCursors;
    public GameObject[] subToolBars;

    public Color selectedColor, normalColor;

    private EffectSize currentEffect;

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
        currentEffect = EffectSize.Small;
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
        OnEffectSelect(EffectSize.Small);
    }

    public void OnSelectMedium()
    {
        OnEffectSelect(EffectSize.Medium);
    }

    public void OnSelectLarge()
    {
        OnEffectSelect(EffectSize.Large);
    }

    public EffectSize GetEffectSize()
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
                    ToggleHighlightOnCurrentEffect();
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

    private void OnEffectSelect(EffectSize effect)
    {
        ToggleHighlightOnCurrentEffect();

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

    private void ToggleHighlightOnCurrentEffect()
    {
        ButtonHighlighter[] highlighters = currentToolButton.GetComponentsInChildren<ButtonHighlighter>();

        if (highlighters.Length > (int)currentEffect+1)
        {
            highlighters[(int)currentEffect+1].ToggleHighlight();
        }
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
    void UpdateWhenSelected();
    void OnSelect();
    void OnDeselect();
}