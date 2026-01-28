using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollInventoryPanelView : SlideUIBase
{
    [Header("--- Repository Inject ---")]
    [SerializeField] private ArtifactRepository m_ArtifactRepository;

    [Header("--- UI 연결 ---")]
    [SerializeField] private ScrollRect m_ScrollRect;
    [SerializeField] private RectTransform m_Content;
    [SerializeField] private GameObject m_ArtifactPrafabs;

    [Header("--- 세팅 값 ---")]
    [SerializeField] private Padding m_Padding;
    [SerializeField] private CellSize m_CellSize;
    [SerializeField] private Spacing m_Spacing;
    [SerializeField] private int m_InstancingCount = 21;
    [SerializeField] private int m_ColumnCount = 4;

    [Header("--- 스크롤 경계 설정 ---")]
    [SerializeField] private float m_TopTheshhold = 1.7f;
    [SerializeField] private float m_BottomTheshhold = 0.9f;

    [Header("--- Event Channels (Listen) ---")]
    [SerializeField] private CSharpObjectEventChannelSO m_OnUpdatePlayerData;

    private LinkedList<Artifact> m_AritfactList = new LinkedList<Artifact>();
    private List<ArtifactDataSO> m_DisplayArtifact = new();
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

    private void OnDisable()
    {
        m_OnUpdatePlayerData.OnEventRaised -= HandleOnUpdatePlayerData;
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
            m_DisplayArtifact = m_PlayerData.Inventory;

            foreach (var slot in m_AritfactList)
            {
                UpdateSlotState(slot);
            }
            SetContentHeight();
        }
    }

    private void CreateSlotInstance()
    {
        for (int i = 0; i < m_InstancingCount; i++)
        {
            GameObject slotObj = Instantiate(m_ArtifactPrafabs, m_Content);
            Artifact slot = slotObj.GetComponent<Artifact>();
            slot.DataIndex = i;
            m_AritfactList.AddLast(slot);
        }

        ResetVirtualView();
    }

    private void ResetVirtualView()
    {
        m_Content.anchoredPosition = Vector2.zero;
        int index = 0;
        foreach (var slot in m_AritfactList)
        {
            RectTransform slotRt = slot.GetComponent<RectTransform>();
            var x = m_CellSize.Width/2 +  (m_Spacing.X * (m_ColumnCount -1)) + (m_CellSize.Width + m_Spacing.X) * (index % m_ColumnCount);
            var y = -m_CellSize.Height/2 -m_Padding.Top - (m_CellSize.Height + m_Spacing.Y) * (index / m_ColumnCount);
            slotRt.anchoredPosition = new Vector2(x, y);
            slot.DataIndex = index;
            UpdateSlotState(slot);
            index++;
        }
        SetContentHeight();
    }

    private void SetContentHeight()
    {
        var rowCount = Mathf.CeilToInt((float)m_DisplayArtifact.Count / m_ColumnCount);
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
        if (m_AritfactList.Count == 0) return;

        var topFirstSlot1 = m_AritfactList.First.Value;
        var topFirstSlot2 = m_AritfactList.First.Next.Value;
        var topFirstSlot3 = m_AritfactList.First.Next.Next.Value;
        if (topFirstSlot1.transform.localPosition.y + contentY > m_CellSize.Height * m_TopTheshhold)
        {
            var lastSlot1 = m_AritfactList.Last.Value;
            var lastSlot2 = m_AritfactList.Last.Previous.Value;
            var lastSlot3 = m_AritfactList.Last.Previous.Previous.Value;

            topFirstSlot1.transform.localPosition = lastSlot3.transform.localPosition - new Vector3(0, m_CellSize.Height + m_Spacing.Y);
            topFirstSlot2.transform.localPosition = lastSlot2.transform.localPosition - new Vector3(0, m_CellSize.Height + m_Spacing.Y);
            topFirstSlot3.transform.localPosition = lastSlot1.transform.localPosition - new Vector3(0, m_CellSize.Height + m_Spacing.Y);

            topFirstSlot1.DataIndex = lastSlot3.DataIndex + m_ColumnCount;
            topFirstSlot2.DataIndex = lastSlot2.DataIndex + m_ColumnCount;
            topFirstSlot3.DataIndex = lastSlot1.DataIndex + m_ColumnCount;

            m_AritfactList.RemoveFirst();
            m_AritfactList.RemoveFirst();
            m_AritfactList.RemoveFirst();

            m_AritfactList.AddLast(topFirstSlot1);
            m_AritfactList.AddLast(topFirstSlot2);
            m_AritfactList.AddLast(topFirstSlot3);

            UpdateSlotState(topFirstSlot1);
            UpdateSlotState(topFirstSlot2);
            UpdateSlotState(topFirstSlot3);
        }
        else if (m_AritfactList.Last.Value.transform.localPosition.y + contentY < -scrollHeight - m_CellSize.Height * m_BottomTheshhold)
        {
            var lastSlot1 = m_AritfactList.Last.Value;
            var lastSlot2 = m_AritfactList.Last.Previous.Value;
            var lastSlot3 = m_AritfactList.Last.Previous.Previous.Value;

            var FirstSlot1 = m_AritfactList.First.Value;
            var FirstSlot2 = m_AritfactList.First.Next.Value;
            var FirstSlot3 = m_AritfactList.First.Next.Next.Value;

            lastSlot1.transform.localPosition = FirstSlot3.transform.localPosition + new Vector3(0, m_CellSize.Height + m_Spacing.Y);
            lastSlot2.transform.localPosition = FirstSlot2.transform.localPosition + new Vector3(0, m_CellSize.Height + m_Spacing.Y);
            lastSlot3.transform.localPosition = FirstSlot1.transform.localPosition + new Vector3(0, m_CellSize.Height + m_Spacing.Y);

            lastSlot1.DataIndex = FirstSlot3.DataIndex - m_ColumnCount;
            lastSlot2.DataIndex = FirstSlot2.DataIndex - m_ColumnCount;
            lastSlot3.DataIndex = FirstSlot1.DataIndex - m_ColumnCount;

            m_AritfactList.RemoveLast();
            m_AritfactList.RemoveLast();
            m_AritfactList.RemoveLast();

            m_AritfactList.AddFirst(lastSlot3);
            m_AritfactList.AddFirst(lastSlot2);
            m_AritfactList.AddFirst(lastSlot1);

            UpdateSlotState(lastSlot1);
            UpdateSlotState(lastSlot2);
            UpdateSlotState(lastSlot3);
        }
    }

    private void UpdateSlotState(Artifact slot)
    {
        bool hasData = slot.DataIndex >= 0 && slot.DataIndex < m_DisplayArtifact.Count;
        if (hasData)
        {
            slot.gameObject.SetActive(true);
            slot.UpdateData(m_PlayerData.Inventory, m_ArtifactRepository);
        }
        else
        {
            slot.gameObject.SetActive(false);
        }
    }
}

[Serializable]
public struct Padding
{
    public int Top;
    public int Bottom;
    public int Left;
    public int Right;
}

[Serializable]
public struct Spacing
{
    public int X;
    public int Y;
}

[Serializable]
public struct CellSize
{
    public int Width;
    public int Height;
}