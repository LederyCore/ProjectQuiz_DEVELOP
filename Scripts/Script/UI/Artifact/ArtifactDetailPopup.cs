using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class ArtifactDetailPopup : MonoBehaviour
{
    [SerializeField] private Image m_artifactImage;
    [SerializeField] private TextMeshProUGUI m_artifactNameText;
    [SerializeField] private TextMeshProUGUI m_artifactDescriptionText;
    [SerializeField] private Button m_closeButton;
    [SerializeField] private BooleanEventChannelSO m_onDisableGlobControl;
    private void Awake()
    {
        m_closeButton.onClick.AddListener(ClosePopup);
    }
    private void OnDestroy()
    {
        m_closeButton.onClick.RemoveAllListeners();
    }
    public void HandleOnRequestArtifactDetailPopup(object artifactData)
    {
        if (artifactData is not ArtifactDataSO artifact)
        {
            Debug.LogWarning("Received data is not of type ArtifactDataSO.");
            return;
        }
        m_artifactImage.sprite = artifact.artifactImage;
        m_artifactNameText.text = artifact.artifactName;
        m_artifactDescriptionText.text = artifact.description;

        gameObject.SetActive(true);
    }
    private void ClosePopup()
    {
        m_onDisableGlobControl.RaiseEvent(true);
        gameObject.SetActive(false);
    }
}