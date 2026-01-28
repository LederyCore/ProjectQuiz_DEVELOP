using UnityEngine.Events;

public abstract class EventChannelSO<T> : EventChannelBaseSO
{
    // C#의 기본 대리자인 UnityAction을 사용합니다.
    public UnityAction<T> OnEventRaised;

    public void RaiseEvent(T parameter)
    {
        OnEventRaised?.Invoke(parameter);
    }
}