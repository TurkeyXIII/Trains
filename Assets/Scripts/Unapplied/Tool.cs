using UnityEngine;
using System.Collections;

public class Tool : MonoBehaviour
{
    public virtual Effect GetDefaultEffect()
    {
        return Effect.None;
    }

    public virtual void UpdateWhenSelected()
    {
        // do nothing
    }


    public virtual void OnDeselect()
    {
    }


    public virtual void OnSelect()
    {
    }

    public virtual void OnEffectChange()
    {
    }
}
