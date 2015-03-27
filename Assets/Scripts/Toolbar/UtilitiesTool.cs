using UnityEngine;
using System.Collections;

public class UtilitiesTool : MonoBehaviour, ITool
{

    public void UpdateWhenSelected()
    {
        // do nothing
    }


    public void OnDeselect()
    {
    }


    public void OnSelect()
    {
    }

    public void OnEffectChange()
    {
    }

    public Effect GetDefaultEffect()
    {
        return Effect.None;
    }
}
