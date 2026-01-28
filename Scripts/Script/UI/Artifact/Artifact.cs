using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Artifact : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image m_ArtifactImage;
    [SerializeField] private UnityObjectEventChannelSO m_OnRequestArtifactDetailPopup;

    private ArtifactDataSO m_ArtifactData;

    public int DataIndex { get;  set; }

    public void OnPointerClick(PointerEventData eventData)
    {
        m_OnRequestArtifactDetailPopup.RaiseEvent(m_ArtifactData);
    }

    public void UpdateData(List<ArtifactDataSO> list, ArtifactRepository repo)
    {
        // 1. 기초 데이터 유효성 검사
        if (list == null || repo == null) return;


        // 3. 인덱스 범위 및 요소 유효성 검사
        if (DataIndex < 0 || DataIndex >= list.Count || list[DataIndex] == null)
        {
            // 데이터가 없는 슬롯일 경우 UI를 비우는 처리가 필요할 수 있습니다.
            if (m_ArtifactImage != null) m_ArtifactImage.enabled = false;
            return;
        }

        string artifactID = list[DataIndex].artifactID;

        m_ArtifactData = repo.GetArtifactByID(artifactID);

        if (m_ArtifactData == null)
        {
            Debug.LogWarning($"Artifact with ID {artifactID} not found in repository.");
            m_ArtifactImage.enabled = false;
            return;
        }

        Sprite laodSprite = m_ArtifactData.artifactImage;

        if (m_ArtifactImage != null)
        {
            m_ArtifactImage.enabled = true;
            m_ArtifactImage.sprite = laodSprite;
        }
        else
        {
            Debug.LogWarning("Artifact Image component is not assigned.");
        }
    }
}
