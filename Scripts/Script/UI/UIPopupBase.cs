using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPopupBase : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Title;
    [SerializeField] private TextMeshProUGUI Content;
    [SerializeField] private Button Button;

    private Action m_OnShow = null;
    private Action m_OnClose = null;

    private void Awake()
    {
        Button.onClick.AddListener(ClosePopup);
    }

    private void OnDestroy()
    {
        Button.onClick.RemoveListener(ClosePopup);
    }

    public void Setup(string title, string content, Action onShow = null, Action onClose = null)
    {
        Title.text = title;
        Content.text = content;
        m_OnShow = onShow;
        m_OnClose = onClose;

        m_OnShow?.Invoke();
        gameObject.SetActive(true);
    }
    private void ClosePopup()
    {
        m_OnClose?.Invoke();
        gameObject.SetActive(false);
    }
}
