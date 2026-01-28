using UnityEngine;

public abstract class ScriptableSingletonBase : DescriptionSO
{
    [Header("Singleton Settings")]
    public bool resetOnPlayStart = true;

    protected virtual void OnEnable()
    {
        if (resetOnPlayStart)
        {
            ResetData();
        }

        Debug.Log($"[{this.GetType().Name}] OnEnable() called. resetOnPlayStart={resetOnPlayStart}");
    }

    public abstract void ResetData();
}
