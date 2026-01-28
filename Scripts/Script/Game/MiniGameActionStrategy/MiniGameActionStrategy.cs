using UnityEngine;

public abstract class MiniGameActionStrategy : DescriptionSO
{
    public EventActionType ActionType; // 어떤 타입에 대응하는지 정의

    [Header("----- Base Event Channels (Publish) -----")]
    [SerializeField] private VoidEventChannelSO m_OnPlaySound;
    [SerializeField] private UnityObjectEventChannelSO m_OnRequestShowResultPanel;

    public virtual void Execute(EventDataSO data)
    {
        m_OnPlaySound?.RaiseEvent();
        m_OnRequestShowResultPanel?.RaiseEvent(data);
    }
}