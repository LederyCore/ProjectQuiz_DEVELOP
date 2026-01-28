using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InventoryPanelController : SlideUIBase
{
    [SerializeField] private ArtifactRepository m_ArtifactRepository;

    [Header("--- UI 연결 ---")]
    [SerializeField] private Button m_BtnPrevPage;
    [SerializeField] private Button m_BtnNextPage;
    [SerializeField] private TextMeshProUGUI m_PageNumText;

    [Header("--- Content 연결 ---")]
    [SerializeField] private RectTransform m_Content;
    [SerializeField] private List<Artifact> m_Artifact;


    [Header("--- 이벤트 연결 ---")]
    [SerializeField] private UnityEvent OnPageChanged;

    [SerializeField] private PlayerData m_PlayerData;

    private int m_CurrentPage = 1;
    public int CurrentPage => m_CurrentPage;

    private void Awake()
    {
        m_BtnPrevPage.onClick.AddListener(() => ChangePage(-1));
        m_BtnNextPage.onClick.AddListener(() => ChangePage(1));
        m_PageNumText.text = m_CurrentPage.ToString();
    }

    private void OnDestroy()
    {
        m_BtnPrevPage.onClick.RemoveAllListeners();
        m_BtnNextPage.onClick.RemoveAllListeners();
    }

    public void HandleOnUpdatePlayerData(System.Object playerData)
    {
        if (playerData is PlayerData pd)
        {
            m_PlayerData = pd;
            UpdateData(m_CurrentPage);
        }
    }

    public override void OpenPanel()
    {
        base.OpenPanel();
        UpdateData(m_CurrentPage);
    }

    public override void ClosePanel()
    {
        base.ClosePanel();
    }

    private void ChangePage(int targetPage, Action callback)
    {
        m_CurrentPage = targetPage;
        m_PageNumText.text = m_CurrentPage.ToString();
        OnPageChanged?.Invoke();
        callback?.Invoke();
    }

    private void ChangePage(int v)
    {
        int totalDataCount = m_PlayerData.Inventory.Count;
        int itemsPerPage = m_Artifact.Count;

        // 최대 페이지 계산
        int maxPage = Mathf.CeilToInt((float)totalDataCount / itemsPerPage);

        // 데이터가 아예 없으면 1페이지로 설정
        if (maxPage < 1) maxPage = 1;

        int targetPage = m_CurrentPage + v;

        // 범위 체크
        if (targetPage < 1 || targetPage > maxPage) return;

        m_CurrentPage = targetPage;
        m_PageNumText.text = m_CurrentPage.ToString();
        UpdateData(m_CurrentPage);
    }

    private void UpdateData(int page)
    {
        // 데이터 리스트 가져오기 (null 체크 포함)
        var artifacts = m_PlayerData.Inventory;
        if (artifacts == null) return;

        // 한 페이지당 표시할 슬롯의 개수 
        int slotsPerPage = m_Artifact.Count;

        // 현재 페이지의 데이터 시작 인덱스 계산
        int dataStartIndex = (page - 1) * slotsPerPage;

        // UI슬롯을 기준으로 순회 (0~14)
        for (int i = 0; i < slotsPerPage; i++)
        {
            // 실제 데이터 리스트에서의 인덱스 계산
            int currentDataIndex = dataStartIndex + i;

            // 1. 데이터가 존재하는 경우 
            if (currentDataIndex < artifacts.Count)
            {
                m_Artifact[i].gameObject.SetActive(true);
                //m_Artifact[i].UpdateData(currentDataIndex, m_PlayerData, m_ArtifactRepository);
            }
            else
            {
                m_Artifact[i].gameObject.SetActive(false);
            }
        }

        Debug.Log($"페이지 {page} 갱신 완료 / 데이터 시작 인덱스: {dataStartIndex}");
    }
}
