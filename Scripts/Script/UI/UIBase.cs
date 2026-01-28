using UnityEngine;

public abstract class UIBase : MonoBehaviour
{

    protected bool m_isOpenPanel = false;

    public bool IsOpenPanel => gameObject.activeSelf;

    public virtual void TogglePanel()
    {
        if (IsOpenPanel) ClosePanel();
        else OpenPanel();
    }

    public virtual void OpenPanel()
    {
        if (IsOpenPanel) return; // 이미 열려있으면 무시
        gameObject.SetActive(true);
    }

    public virtual void ClosePanel()
    {
        if (!IsOpenPanel) return;

        m_isOpenPanel = false;
    }
}
