using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EventRepository", menuName = "Data/EventRepository")]
public class EventRepository : DescriptionSO
{
    [SerializeField] private List<EventDataSO> m_CommonDangerDeck;
    [SerializeField] private List<EventDataSO> m_CommonLuckDeck;
    [SerializeField] private List<EventDataSO> m_CommonEventDeck;
    [SerializeField] private List<EventDataSO> m_CommonArtifactDeck;
    [SerializeField] private List<EventDataSO> m_FailEventData;
    [SerializeField] private List<MiniGameActionStrategy> m_Strategies;

    // 에디터에서 할당하기 위한 프로퍼티 (Internal 또는 Public)
    public List<EventDataSO> CommonDangerDeck { get => m_CommonDangerDeck; set => m_CommonDangerDeck = value; }
    public List<EventDataSO> CommonLuckDeck { get => m_CommonLuckDeck; set => m_CommonLuckDeck = value; }
    public List<EventDataSO> CommonEventDeck { get => m_CommonEventDeck; set => m_CommonEventDeck = value; }
    public List<EventDataSO> CommonArtifactDeck { get => m_CommonArtifactDeck; set => m_CommonArtifactDeck = value; }
    public List<EventDataSO> FailEventData { get => m_FailEventData; set => m_FailEventData = value; }

    public EventDataSO SelectRandomEvent(List<EventDataSO> list)
    {
        if (list == null || list.Count == 0)
        {
            Debug.LogWarning("이벤트 데이터 리스트가 비어있습니다.");
            return null;
        }
        int randomIndex = Random.Range(0, list.Count);
        return list[randomIndex];
    }

    public void ExecuteStrategy(EventActionType type)
    {
        // 1. 해당 타입에 맞는 덱(데이터 리스트)을 가져옴
        List<EventDataSO> targetDeck = GetDeckByType(type);

        // 2. 덱에서 무작위 데이터 선택
        EventDataSO selectedData = SelectRandomEvent(targetDeck);

        // 3. 타입에 맞는 전략 탐색 및 실행
        var targetStrategy = m_Strategies.Find(strategy => strategy.ActionType == type);

        if (targetStrategy != null)
        {
            targetStrategy.Execute(selectedData);
        }
        else
        {
            Debug.LogWarning($"해당 ActionType에 대한 전략이 없습니다: {type}");
        }
    }

    // 딕셔너리 대신 타입을 기반으로 리스트를 반환하는 헬퍼 메서드
    private List<EventDataSO> GetDeckByType(EventActionType type)
    {
        return type switch
        {
            EventActionType.Danger => m_CommonDangerDeck,
            EventActionType.Luck => m_CommonLuckDeck,
            EventActionType.Event => m_CommonEventDeck,
            EventActionType.Artifact => m_CommonArtifactDeck,
            EventActionType.Fail => m_FailEventData,
            _ => null
        };
    }
}

public enum EventActionType { Danger, Luck, Event, Artifact, Fail }