using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIConfirmBase : MonoBehaviour
{
    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI DescriptionText;
    public Button CancelButton;
    public Button ConfirmButton;

    private Action m_OnShow = null;
    private Action m_OnClose = null;
    private Action m_OnConfirm = null;
    private Action m_OnCancel = null;

    private void Awake()
    {
        CancelButton.onClick.AddListener(OnCancel);
        ConfirmButton.onClick.AddListener(OnConfirm);
    }

    private void OnDestroy()
    {
        CancelButton.onClick.RemoveListener(OnCancel);
        ConfirmButton.onClick.RemoveListener(OnConfirm);
    }

    public void Setup(string title, string description, Action onShow = null, Action onClose = null, Action onConfirm = null, Action onCancel = null)
    {
        TitleText.text = title;
        DescriptionText.text = description;
        m_OnShow = onShow;
        m_OnClose = onClose;
        m_OnConfirm = onConfirm;
        m_OnCancel = onCancel;
        m_OnShow?.Invoke();
        gameObject.SetActive(true);
    }

    private void OnConfirm()
    {
        m_OnConfirm?.Invoke();
        ClosePopup();
    }

    private void OnCancel()
    {
        m_OnCancel?.Invoke();
        ClosePopup();
    }

    private void ClosePopup()
    {
        m_OnClose?.Invoke();
        gameObject.SetActive(false);
    }
}
