using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultPanelController : MonoBehaviour
{
    [SerializeField] private Image m_Image;
    [SerializeField] private TextMeshProUGUI m_TitleTxt;
    [SerializeField] private TextMeshProUGUI m_DescriptionTxt;
    [SerializeField] private Button m_CloseButton;

    private void Awake()
    {
        m_CloseButton.onClick.AddListener(ClosePanel);
    }

    private void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    public void HandleOnRequestShowResultPanel(Object obj)
    {
        if (obj is not EventDataSO evtData) return;


        m_Image.sprite = evtData.eventImage;
        m_TitleTxt.text = evtData.title;
        m_DescriptionTxt.text = evtData.description;

        gameObject.SetActive(true);
    }
}
