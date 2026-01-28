using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollPassportPanelController : SlideUIBase
{
    [Header("--- Repository Inject ---")]
    [SerializeField] private PlaceRepository m_PlaceRepository;

    [Header("--- UI 연결 ---")]
    [SerializeField] private ScrollRect m_ScrollRect;
    [SerializeField] private RectTransform m_Content;
    [SerializeField] private GameObject m_StampPrefab;

    [Header("--- 세팅 값 ---")]
    [SerializeField] private Padding m_Padding;
    [SerializeField] private CellSize m_CellSize;
    [SerializeField] private Spacing m_Spacing;
    [SerializeField] private int m_InstancingCount = 15;
    [SerializeField] private int m_ColumnCount = 3;

    [Header("--- 스크롤 경계 설정 ---")]
    [SerializeField] private float m_TopTheshhold = 1.7f;
    [SerializeField] private float m_BottomTheshhold = 0.9f;

    [Header("--- Event Channels (Listen) ---")]
    [SerializeField] private CSharpObjectEventChannelSO m_OnUpdatePlayerData;

    [Header("--- Event Channels (Publish) ---")]
    [SerializeField] private UnityObjectEventChannelSO m_OnRequestAddTravelLog;
    [SerializeField] private UnityObjectEventChannelSO m_OnStartQuizEvent;

    private LinkedList<Stamp> m_StampList = new LinkedList<Stamp>();
    private List<CountryDataSO> m_DisplayStamp = new();
    private PlayerData m_PlayerData;
    private RectTransform m_ScrollRectTransform;


    private void Awake()
    {
        m_ScrollRect.onValueChanged.AddListener(OnScroll);
        m_ScrollRectTransform = m_ScrollRect.GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        m_OnUpdatePlayerData.OnEventRaised += HandleOnUpdatePlayerData;
    }

    private void Start()
    {
        SetContentHeight();
        CreateSlotInstance();
    }

    private void OnDestroy()
    {
        if (m_ScrollRect != null) m_ScrollRect.onValueChanged.RemoveAllListeners();
    }

    public void HandleOnUpdatePlayerData(System.Object playerData)
    {
        if (playerData is PlayerData pd)
        {
            m_PlayerData = pd;
            m_DisplayStamp = m_PlaceRepository.GetCountriesByIDs(m_PlayerData.VisitedCountryIDs);

            foreach (var slot in m_StampList)
            {
                UpdateSlotState(slot);
            }
            SetContentHeight();
        }
    }

    public void HandleOnArrival(UnityEngine.Object obj)
    {
        if (obj is PlaceDataSO placeData)
        {
            // 현재 플레이어 데아터 중에 방문한 국가가 이미 있을 경우는 스탬프 연출을 하지 않고 해당 국가에 대한 퀴즈를 바로 시작한다.
            if (m_PlayerData.VisitedCountryIDs.Contains(placeData.countryID))
            {
                m_OnRequestAddTravelLog.RaiseEvent(placeData);
                m_OnStartQuizEvent.RaiseEvent(placeData);
                return;
            }
            var stamp = m_PlaceRepository.GetCountryByPlaceID(placeData.placeID);
            var stampData = stamp != null ? stamp.stampImage : null;
            m_OnRequestAddTravelLog.RaiseEvent(placeData);
            //ShowEntryStampSequence(stampData, () => m_OnStartQuizEvent.RaiseEvent(placeData));
        }
    }

    private void CreateSlotInstance()
    {
        for (int i = 0; i < m_InstancingCount; i++)
        {
            GameObject slotObj = Instantiate(m_StampPrefab, m_Content);
            Stamp slot = slotObj.GetComponent<Stamp>();
            slot.DataIndex = i;
            m_StampList.AddLast(slot);
        }

        ResetVirtualView();
    }

    private void ResetVirtualView()
    {
        m_Content.anchoredPosition = Vector2.zero;
        int index = 0;
        foreach (var slot in m_StampList)
        {
            RectTransform slotRt = slot.GetComponent<RectTransform>();
            var x = m_CellSize.Width / 2 + (m_Spacing.X * (m_ColumnCount - 1)) + (m_CellSize.Width + m_Spacing.X) * (index % m_ColumnCount);
            var y = -m_CellSize.Height / 2 - m_Padding.Top - (m_CellSize.Height + m_Spacing.Y) * (index / m_ColumnCount);
            slotRt.anchoredPosition = new Vector2(x, y);
            slot.DataIndex = index;
            UpdateSlotState(slot);
            index++;
        }
        SetContentHeight();
    }

    private void SetContentHeight()
    {
        var rowCount = Mathf.CeilToInt((float)m_DisplayStamp.Count / m_ColumnCount);
        float contentHeight = m_Padding.Top + m_Padding.Bottom +
                              rowCount * (m_CellSize.Height + m_Spacing.Y) + m_Spacing.Y;
        m_Content.sizeDelta = new Vector2(m_Content.sizeDelta.x, contentHeight);
    }

    private void OnScroll(Vector2 vec)
    {
        CheckAndRelocate(m_Content.anchoredPosition.y, m_ScrollRectTransform.rect.height);
    }

    private void CheckAndRelocate(float contentY, float scrollHeight)
    {
        if (m_StampList.Count == 0) return;

        var topFirstSlot1 = m_StampList.First.Value;
        var topFirstSlot2 = m_StampList.First.Next.Value;
        var topFirstSlot3 = m_StampList.First.Next.Next.Value;
        if (topFirstSlot1.transform.localPosition.y + contentY > m_CellSize.Height * m_TopTheshhold)
        {
            var lastSlot1 = m_StampList.Last.Value;
            var lastSlot2 = m_StampList.Last.Previous.Value;
            var lastSlot3 = m_StampList.Last.Previous.Previous.Value;

            topFirstSlot1.transform.localPosition = lastSlot3.transform.localPosition - new Vector3(0, m_CellSize.Height + m_Spacing.Y);
            topFirstSlot2.transform.localPosition = lastSlot2.transform.localPosition - new Vector3(0, m_CellSize.Height + m_Spacing.Y);
            topFirstSlot3.transform.localPosition = lastSlot1.transform.localPosition - new Vector3(0, m_CellSize.Height + m_Spacing.Y);

            topFirstSlot1.DataIndex = lastSlot3.DataIndex + m_ColumnCount;
            topFirstSlot2.DataIndex = lastSlot2.DataIndex + m_ColumnCount;
            topFirstSlot3.DataIndex = lastSlot1.DataIndex + m_ColumnCount;

            m_StampList.RemoveFirst();
            m_StampList.RemoveFirst();
            m_StampList.RemoveFirst();

            m_StampList.AddLast(topFirstSlot1);
            m_StampList.AddLast(topFirstSlot2);
            m_StampList.AddLast(topFirstSlot3);

            UpdateSlotState(topFirstSlot1);
            UpdateSlotState(topFirstSlot2);
            UpdateSlotState(topFirstSlot3);
        }
        else if (m_StampList.Last.Value.transform.localPosition.y + contentY < -scrollHeight - m_CellSize.Height * m_BottomTheshhold)
        {
            var lastSlot1 = m_StampList.Last.Value;
            var lastSlot2 = m_StampList.Last.Previous.Value;
            var lastSlot3 = m_StampList.Last.Previous.Previous.Value;

            var FirstSlot1 = m_StampList.First.Value;
            var FirstSlot2 = m_StampList.First.Next.Value;
            var FirstSlot3 = m_StampList.First.Next.Next.Value;

            lastSlot1.transform.localPosition = FirstSlot3.transform.localPosition + new Vector3(0, m_CellSize.Height + m_Spacing.Y);   
            lastSlot2.transform.localPosition = FirstSlot2.transform.localPosition + new Vector3(0, m_CellSize.Height + m_Spacing.Y);
            lastSlot3.transform.localPosition = FirstSlot1.transform.localPosition + new Vector3(0, m_CellSize.Height + m_Spacing.Y);

            lastSlot1.DataIndex = FirstSlot3.DataIndex - m_ColumnCount;
            lastSlot2.DataIndex = FirstSlot2.DataIndex - m_ColumnCount;
            lastSlot3.DataIndex = FirstSlot1.DataIndex - m_ColumnCount;

            m_StampList.RemoveLast();
            m_StampList.RemoveLast();
            m_StampList.RemoveLast();

            m_StampList.AddFirst(lastSlot3);
            m_StampList.AddFirst(lastSlot2);
            m_StampList.AddFirst(lastSlot1);

            UpdateSlotState(lastSlot1);
            UpdateSlotState(lastSlot2);
            UpdateSlotState(lastSlot3);
        }
    }

    private void UpdateSlotState(Stamp slot)
    {
        bool hasData = slot.DataIndex >= 0 && slot.DataIndex < m_DisplayStamp.Count;
        if (hasData)
        {
            slot.gameObject.SetActive(true);
            slot.UpdateData(m_PlayerData.VisitedCountryIDs, m_PlaceRepository);
        }
        else
        {
            slot.gameObject.SetActive(false);
        }
    }
}