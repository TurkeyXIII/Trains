using UnityEngine;
using System.Collections;

public class Tool : MonoBehaviour
{
    // these virtual functions provide default behaviour
    // overrides do not need to call base.
    public virtual Effect GetDefaultEffect()
    {
        return Effect.None;
    }

    public virtual void UpdateWhenSelected()
    {
    }

    public virtual void OnDeselect()
    {
        Control.GetControl().DestroyCursorLight();
    }

    public virtual void OnSelect()
    {
    }

    public virtual void OnEffectChange()
    {
    }
}
