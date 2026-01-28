using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;

public class MissionPanelController : UIBase
{
    [Header("--- UI 연결 ---")]
    [SerializeField] private ScrollRect m_ScrollRect;
    [SerializeField] private RectTransform m_Content;
    [SerializeField] private GameObject m_MissionSlotPrefab;
    [SerializeField] private Button m_BtnClose;
    [SerializeField] private Button m_BtnSort;

    [Header("--- 세팅 값 ---")]
    [SerializeField] private int m_itemHeight = 166;
    [SerializeField] private int m_Spacing = 4;
    [SerializeField] private int m_InstancingCount = 11;
    [SerializeField] private MissionRepository m_MissionRepository;
    [SerializeField] private Color m_sortBtnActiveColor;

    [Header("--- 스크롤 경계 설정 ---")]
    public float m_topThreshold = 1.7f;
    public float m_lastThreshold = 0.9f;

    [Header("--- Event Channels (Listen) ---")]
    [SerializeField] private CSharpObjectEventChannelSO m_OnUpdateMissionStatChannel;

    private LinkedList<MissionSlot> m_SlotList = new LinkedList<MissionSlot>();
    private List<MissionDataSO> m_DisplayMissions = new List<MissionDataSO>();
    private MissionStatisticsData m_stats;
    private RectTransform m_ScrollRectTransform;
    private bool m_isSorted = false;

    private void Awake()
    {
        m_ScrollRect.onValueChanged.AddListener(OnScroll);
        m_BtnClose.onClick.AddListener(Close);
        m_BtnSort.onClick.AddListener(Sort);
        m_ScrollRectTransform = m_ScrollRect.GetComponent<RectTransform>();
    }
    private void OnEnable()
    {
        m_OnUpdateMissionStatChannel.OnEventRaised += HandleUpdateMissionStatData;
    }
    private void Start()
    {
        m_DisplayMissions = new List<MissionDataSO>(m_MissionRepository.Missions);
        SetContentHeight();
        CreateSlotInstance();
    }
    private void OnDisable()
    {
        m_OnUpdateMissionStatChannel.OnEventRaised -= HandleUpdateMissionStatData;
    }
    private void OnDestroy()
    {
        if (m_ScrollRect != null) m_ScrollRect.onValueChanged.RemoveAllListeners();
        if (m_BtnClose != null) m_BtnClose.onClick.RemoveAllListeners();
        if (m_BtnSort != null) m_BtnSort.onClick.RemoveAllListeners();
    }

    // 보상 수령 시 MissionStatisticsData에서 호출됨
    public void HandleUpdateMissionStatData(object obj)
    {
        if (obj is MissionStatisticsData stats)
        {
            m_stats = stats;

            // 1. 현재 정렬 모드라면 데이터를 실시간으로 재정렬
            if (m_isSorted)
            {
                ExecuteSortLogic();
                // 정렬 후 슬롯들의 데이터 인덱스에 맞는 값들로 UI 갱신
                foreach (var slot in m_SlotList)
                {
                    if (slot.gameObject.activeSelf)
                        UpdateSlotState(slot);
                }
            }
            else
            {
                // 정렬 모드가 아니라면 단순히 데이터만 업데이트
                foreach (var slot in m_SlotList)
                {
                    if (slot.gameObject.activeSelf)
                        slot.UpdateData(m_stats);
                }
            }
        }
    }

    private void CreateSlotInstance()
    {
        for (int i = 0; i < m_InstancingCount; i++)
        {
            GameObject slotObj = Instantiate(m_MissionSlotPrefab, m_Content);
            MissionSlot slot = slotObj.GetComponent<MissionSlot>();
            slot.DataIndex = i;
            m_SlotList.AddLast(slot);
        }
        ResetVirtualView();
    }

    private void SetContentHeight()
    {
        float contentHeight = m_DisplayMissions.Count * (m_itemHeight + m_Spacing);
        m_Content.sizeDelta = new Vector2(m_Content.sizeDelta.x, contentHeight);
    }

    private void OnScroll(Vector2 vec)
    {
        CheckAndRelocate(m_Content.anchoredPosition.y, m_ScrollRectTransform.rect.height);
    }

    private void CheckAndRelocate(float contentY, float scrollHeight)
    {
        if (m_SlotList.Count == 0) return;

        var firstSlot = m_SlotList.First.Value;
        if (firstSlot.transform.localPosition.y + contentY > m_itemHeight * m_topThreshold)
        {
            var lastSlot = m_SlotList.Last.Value;
            firstSlot.transform.localPosition = lastSlot.transform.localPosition - new Vector3(0, m_itemHeight + m_Spacing);
            firstSlot.DataIndex = lastSlot.DataIndex + 1;
            m_SlotList.RemoveFirst();
            m_SlotList.AddLast(firstSlot);
            UpdateSlotState(firstSlot);
        }
        else if (m_SlotList.Last.Value.transform.localPosition.y + contentY < -scrollHeight - m_itemHeight * m_lastThreshold)
        {
            var lastSlot = m_SlotList.Last.Value;
            var fSlot = m_SlotList.First.Value;
            lastSlot.transform.localPosition = fSlot.transform.localPosition + new Vector3(0, m_itemHeight + m_Spacing);
            lastSlot.DataIndex = fSlot.DataIndex - 1;
            m_SlotList.RemoveLast();
            m_SlotList.AddFirst(lastSlot);
            UpdateSlotState(lastSlot);
        }
    }

    private void UpdateSlotState(MissionSlot slot)
    {
        bool hasData = slot.DataIndex >= 0 && slot.DataIndex < m_DisplayMissions.Count;
        if (hasData)
        {
            slot.gameObject.SetActive(true);
            slot.UpdateData(m_DisplayMissions[slot.DataIndex], m_stats);
        }
        else
        {
            slot.gameObject.SetActive(false);
        }
    }

    private void Sort()
    {
        m_ScrollRect.StopMovement();
        m_Content.anchoredPosition = Vector2.zero;

        m_isSorted = !m_isSorted;

        if (m_isSorted)
        {
            ExecuteSortLogic();
            m_BtnSort.GetComponent<Image>().color = m_sortBtnActiveColor;
        }
        else
        {
            m_DisplayMissions = new List<MissionDataSO>(m_MissionRepository.Missions);
            m_BtnSort.GetComponent<Image>().color = Color.white;
        }

        ResetVirtualView();
    }

    // 정렬 로직만 따로 분리 (실시간 갱신 시 활용)
    private void ExecuteSortLogic()
    {
        if (m_stats == null) return;

        m_DisplayMissions = m_DisplayMissions
            .OrderBy(mission =>
            {
                int rawValue = mission.GetCurrentValue(m_stats);
                bool isReached = rawValue >= mission.Target;
                bool isClaimed = m_stats.StatusTracker.IsCompleted(mission.Id);

                if (isReached && !isClaimed) return 0; // 보상 가능
                if (!isReached) return 1;              // 진행 중
                return 2;                              // 완료
            })
            .ThenBy(mission => mission.Id)
            .ToList();
    }

    private void ResetVirtualView()
    {
        m_Content.anchoredPosition = Vector2.zero;
        int index = 0;
        foreach (var slot in m_SlotList)
        {
            RectTransform slotRt = slot.GetComponent<RectTransform>();
            slotRt.anchoredPosition = new Vector2(0, -(m_itemHeight + m_Spacing) * index);
            slot.DataIndex = index;
            UpdateSlotState(slot);
            index++;
        }
        SetContentHeight();
    }

    private void Close() => gameObject.SetActive(false);
}