using UnityEngine;
using UnityEngine.Events;

// T : 데이터 타입, TChannel : 해당 데이터를 사용하는 채널 타입
public abstract class GenericEventListener<T, TChannel> : MonoBehaviour
    where TChannel : EventChannelSO<T>
{
    [SerializeField] private TChannel m_channel = default;

    // 유니티 인스펙터에서 콜백을 등록할 수 있게 합니다.
    public UnityEvent<T> OnEventRaised;

    private void OnEnable()
    {
        if (m_channel != null) m_channel.OnEventRaised += Respond;
    }

    private void OnDisable()
    {
        if (m_channel != null) m_channel.OnEventRaised -= Respond;
    }

    private void Respond(T value)
    {
        OnEventRaised?.Invoke(value);
    }
}