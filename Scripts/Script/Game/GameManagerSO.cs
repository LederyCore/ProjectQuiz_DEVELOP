using System;
using UnityEngine;

/// <summary>
/// 이 매니저는 매니저 부트스트래퍼 리스트의 맨 마지막에 위치하여야 합니다.
/// </summary>
[CreateAssetMenu(fileName = "GameManagerSO", menuName = "ManagerSO/GameManagerSO")]
public class GameManagerSO : ManagerSO
{
    [Space(10), Header("----- Data -----")]
    [SerializeField] DataManagerSO m_DataManagerSO;

    [Space(10), Header("----- Runtime State -----")]
    [SerializeField] private GameState m_CurrentState = GameState.NONE;

    [Space(10), Header("----- Event Channels (Listen) -----")]
    [SerializeField] private GameStateEventChannelSO m_OnRequestChangeGameState;

    [Space(10), Header("----- Event Channels (Publish) -----")]
    [SerializeField] private GameStateEventChannelSO m_OnStateChanged;
    [SerializeField] private VoidEventChannelSO m_OnRequestAlignView;      // 시야 정렬 요청하는 이벤트
    [SerializeField] private VoidEventChannelSO m_OnRequestActiveMarker;
    [SerializeField] private CSharpObjectEventChannelSO m_OnRequestUpdatePlayerData;
    [SerializeField] private CSharpObjectEventChannelSO m_OnRequestUpdateMissionStat;
    [SerializeField] private VoidEventChannelSO m_OnStartGameLogic;
    [SerializeField] private VoidEventChannelSO m_OnStartTutorial;
    [SerializeField] private CSharpObjectEventChannelSO m_OnLoadTutorial;

    public GameState GameState
    {
        get { return m_CurrentState; }
        set
        {
            m_CurrentState = value; 
            m_OnStateChanged?.RaiseEvent(m_CurrentState);
        }
    }

    private void OnEnable()
    {
        m_CurrentState = GameState.NONE;
    }
    private void OnValidate()
    {
        m_OnStateChanged?.RaiseEvent(m_CurrentState);
    }


    public void GameStart()
    {
        m_OnRequestChangeGameState.OnEventRaised += SetState;
        Debug.Log("<color=yellow>[GameManagerSO]</color> 초기화 완료. 게임 로직을 실행합니다.");
        SetState(GameState.IDLE);

        m_OnRequestUpdatePlayerData?.RaiseEvent(m_DataManagerSO.PlayerData);
        m_OnRequestUpdateMissionStat?.RaiseEvent(m_DataManagerSO.MissionStatistics);

        // 튜토리얼이 완료되지 않았고, 최초 실행이면 튜토리얼 이벤트로 진입
        if (m_DataManagerSO.PlayerData.IsGuideFinished == 0 && m_DataManagerSO.PlayerData.IsFirstRun == 1)
        {
            m_DataManagerSO.PlayerData.IsFirstRun = 0; // 최초 실행 플래그 해제
            m_OnStartTutorial?.RaiseEvent();
            return;
        }
        // 튜토리얼이 완료되지 않았고, 최초 실행이 아니면 튜토리얼 이어서 진행
        else if (m_DataManagerSO.PlayerData.IsGuideFinished == 0 && m_DataManagerSO.PlayerData.IsFirstRun == 0)
        {
            m_OnLoadTutorial?.RaiseEvent(m_DataManagerSO.PlayerData);
            return;
        }

        m_OnRequestAlignView?.RaiseEvent();
        m_OnRequestActiveMarker?.RaiseEvent();

        m_OnStartGameLogic?.RaiseEvent();
    }
    public override void Init()
    {

    }

    public override void Destroy()
    {
        m_OnRequestChangeGameState.OnEventRaised -= SetState;
        Debug.Log("<color=yellow>[GameManagerSO]</color> 게임 로직이 모두 종료 되었습니다.");
    }

    public void SetState(GameState newState)
    {
        if (m_CurrentState == newState) return;

        GameState = newState;

        Debug.Log($"<color=yellow>[GameManagerSO]</color> State changed to: {newState}");
    }

    public void HandleOnAlighView() => m_OnRequestAlignView?.RaiseEvent();
}


public enum GameState
{
    IDLE,
    TRAVELING,
    EVENT,
    //MINIGAME,
    NONE
}