using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Events;

/// <summary>
/// 이 컴포넌트는 여권 창의 제어를 담당합니다.
/// </summary>
public class PassportPanelController : SlideUIBase
{
    [SerializeField] private PlaceRepository m_placeRepository;
    [SerializeField] private StampController m_stamper;

    [Space(10), Header("----- Event Channels (Publish) -----")]
    [SerializeField] private UnityObjectEventChannelSO m_OnStartQuizEvent;
    [SerializeField] private UnityObjectEventChannelSO m_OnRequestAddTravelLog;
    [SerializeField] private BooleanEventChannelSO m_onDisableGlobControl;

    [Header("--- UI 연결 ---")]
    [SerializeField] private Button m_btnPrevPage;
    [SerializeField] private Button m_btnNextPage;
    [SerializeField] private Button m_btnClose;
    [SerializeField] private TextMeshProUGUI m_pageNumText;

    [Header("--- Content 연결 ---")]
    [SerializeField] private RectTransform m_content;
    [SerializeField] private List<Stamp> m_stamps;

    [Header("--- 이벤트 연결 ---")]
    [SerializeField] private UnityEvent OnPageChanged;

    [SerializeField] private PlayerData m_playerData;
    private int m_currentPage = 1;


    public int CurrentPage => m_currentPage;

    private void Awake()
    {
        m_btnPrevPage.onClick.AddListener(() => ChangePage(-1));
        m_btnNextPage.onClick.AddListener(() => ChangePage(1));
        m_btnClose.onClick.AddListener(ClosePanel);

        m_pageNumText.text = m_currentPage.ToString();
    }

    private void OnDestroy()
    {
        m_btnPrevPage.onClick.RemoveAllListeners();
        m_btnNextPage.onClick.RemoveAllListeners();
        m_btnClose.onClick.RemoveAllListeners();
    }

    public void HandleOnArrival(UnityEngine.Object obj)
    {
        if (obj is PlaceDataSO placeData)
        {
            // 현재 플레이어 데이터 중에 방문한 국가가 이미 있을 경우는 스탬프 연출을 하지 않고 해당 국가에 대한 퀴즈를 바로 시작한다.
            if (m_playerData.VisitedCountryIDs.Contains(placeData.countryID))
            {
                m_OnRequestAddTravelLog.RaiseEvent(placeData);
                m_OnStartQuizEvent.RaiseEvent(placeData);
                return;
            }
            var stamp = m_placeRepository.GetCountryByPlaceID(placeData.placeID);
            var stampData = stamp != null ? stamp.stampImage : null;
            m_OnRequestAddTravelLog.RaiseEvent(placeData);
            ShowEntryStampSequence(stampData, () => m_OnStartQuizEvent.RaiseEvent(placeData));
        }
    }

    public void HandOnUpdatePlayerData(System.Object playerData)
    {
        if (playerData is PlayerData pd)
        {
            m_playerData = pd;
        }
    }

    // 스탬프 찍는 연출 함수. 각 인덱스 자리에 알맞는 슬롯의 위치를 넘긴다.
    public void ShowEntryStampSequence(Sprite stampSprite = null, Action onFinished = null)
    {
        int totalDataCount = m_playerData.VisitedCountryIDs.Count;
        int itemsPerPage = m_stamps.Count;
        int maxPage = Mathf.CeilToInt((float)totalDataCount / itemsPerPage);

        ChangePage(maxPage, () => UpdateData(maxPage));
        int lastslotindex = (totalDataCount % 15) - 1;
        m_stamps[lastslotindex].gameObject.SetActive(false);
        base.OpenPanel();

        RectTransform rect = m_stamps[lastslotindex].gameObject.GetComponent<RectTransform>();
        Vector2 slotVec = rect.anchoredPosition;

        m_stamper.StartStampSequence(slotVec, () =>
        {
            ClosePanel();
            onFinished?.Invoke();
        }, () =>
        {
            m_stamps[lastslotindex].gameObject.SetActive(true);
        });
    }

    public override void OpenPanel()
    {
        base.OpenPanel();
        UpdateData(m_currentPage);
        m_onDisableGlobControl.RaiseEvent(false);
    }

    public override void ClosePanel()
    {
        base.ClosePanel();
        m_onDisableGlobControl.RaiseEvent(true);
    }

    private void ChangePage(int targetPage, Action callback)
    {
        m_currentPage = targetPage;
        m_pageNumText.text = m_currentPage.ToString();
        OnPageChanged?.Invoke();
        callback?.Invoke();
    }
    private void ChangePage(int v)
    {
        int totalDataCount = m_playerData.VisitedCountryIDs.Count;
        int itemsPerPage = m_stamps.Count;

        // 최대 페이지 계산 (올림 처리)
        int maxPage = Mathf.CeilToInt((float)totalDataCount / itemsPerPage);

        // 데이터가 아예 없으면 1페이지로 설정
        if (maxPage < 1) maxPage = 1;

        int targetPage = m_currentPage + v;

        // 범위 체크
        if (targetPage < 1 || targetPage > maxPage) return;

        m_currentPage = targetPage;
        m_pageNumText.text = m_currentPage.ToString();
        UpdateData(m_currentPage);
    }
    public void UpdateData(int page)
    {
        // 데이터 리스트 가져오기 (null 체크 포함)
        var visitedList = m_playerData.VisitedCountryIDs;
        if (visitedList == null) return;

        // 한 페이지당 표시할 슬롯의 개수 (m_stamps 리스트의 크기)
        int slotsPerPage = m_stamps.Count;

        // 현재 페이지의 데이터 시작 인덱스 계산
        int dataStartIndex = (page - 1) * slotsPerPage;

        // UI 슬롯을 기준으로 순회 (0부터 14까지)
        for (int i = 0; i < slotsPerPage; i++)
        {
            // 실제 데이터 리스트에서의 인덱스 계산
            int currentDataIndex = dataStartIndex + i;

            // 1. 데이터가 존재하는 경우 (데이터 범위 내)
            if (currentDataIndex < visitedList.Count)
            {
                m_stamps[i].gameObject.SetActive(true);
                // 슬롯(i)에게 실제 데이터 인덱스(currentDataIndex)를 전달
                //m_stamps[i].UpdateData(currentDataIndex, m_playerData, m_placeRepository);
            }
            // 2. 데이터가 없는 경우 (빈 슬롯)
            else
            {
                // 빈 슬롯은 비활성화하거나 빈 상태로 표현
                m_stamps[i].gameObject.SetActive(false);
            }
        }

        Debug.Log($"페이지 {page} 갱신 완료 / 데이터 시작 인덱스: {dataStartIndex}");
    }

}