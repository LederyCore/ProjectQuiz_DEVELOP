using UnityEngine;

public abstract class MiniGame : MonoBehaviour
{
    // 미니게임이 끝날때 게임 상태 변경 요청 이벤트 채널
    [Space(10), Header("----- Base Event Channels (Publish) -----")]
    [SerializeField] private GameStateEventChannelSO _OnRequestChangeGameState;

    private EventRepository m_EventDataRepository;

    public EventRepository EventDataRepository
    {
        get => m_EventDataRepository;
        set
        {
            m_EventDataRepository = value;
        }
    }

    protected void RequestChangeGameState(GameState newState)
    {
        if (_OnRequestChangeGameState != null)
        {
            _OnRequestChangeGameState.RaiseEvent(newState);
        }
    }
}