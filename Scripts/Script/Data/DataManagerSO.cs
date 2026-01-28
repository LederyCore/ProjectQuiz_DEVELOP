using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "DataManagerSO", menuName = "ManagerSO/DataManagerSO")]
public class DataManagerSO : ManagerSO
{
    [SerializeField] private bool m_connetToServer = true;

    [Space(10), Header("----- Repository Inject -----")]
    [SerializeField] private ArtifactRepository m_artifactRepository;
    [SerializeField] private PlaceRepository m_placeRepository;
    [SerializeField] private MissionRepository m_missionRepository;

    [Space(10), Header("----- Event Channels (Listen) -----")]
    [SerializeField] private VoidEventChannelSO m_OnApplicationPauseOrQuit;

    [Space(10), Header("----- Event Channels (Publish) -----")]
    [SerializeField] private CSharpObjectEventChannelSO m_onUpdatePlayerData;
    [SerializeField] private CSharpObjectEventChannelSO m_onUpdateMissionStat;
    [SerializeField] private UnityObjectEventChannelSO m_onRequestLoad;
    [SerializeField] private UnityObjectEventChannelSO m_onRequestSave;

    [Space(10), Header("----- Player Runtime Data -----")]
    [SerializeField] private PlayerData m_playerData;
    [SerializeField] private PlayerDataEventChannels m_playerDataEventChannels;
    [SerializeField] private MissionStatisticsData m_missionStatistics;
    [SerializeField] private MissionStatisticsEventChannels m_missionStatisticsEventChannels;


    public PlayerData PlayerData => m_playerData;
    public MissionStatisticsData MissionStatistics => m_missionStatistics;
    public ArtifactRepository ArtifactRepository => m_artifactRepository;

    private void OnValidate()
    {
        // 인스펙터에서 값을 직접 수정할 때 로직 반영
        // 단, Init이 된 상태에서 플레이 모드일 때만 안전하게 실행
        if (IsInitialized && Application.isPlaying)
        {
            ValidateData();
        }
    }

    public override void Init()
    {

        // 1. 순수 C# 데이터 객체들 생성 및 초기화
        m_missionStatistics = new MissionStatisticsData();
        m_playerData = new PlayerData();

        // 2. 이벤트 구독 추가 (중요!)
        if (m_OnApplicationPauseOrQuit != null)
            m_OnApplicationPauseOrQuit.OnEventRaised += RequestSave;

        m_playerData.Event = m_playerDataEventChannels;
        m_playerData.OnUpdatePlayerData = m_onUpdatePlayerData;
        m_missionStatistics.Event = m_missionStatisticsEventChannels;
        m_missionStatistics.MissionRepository = m_missionRepository;
        m_missionStatistics.SubscribeEvents();
        // 초록색 로그 적용
        Debug.Log($"<color=green>[{GetType().Name}]</color> Initialized.");

        RequestLoad();
    }



    public override void Destroy()
    {
        // 1. 이벤트 구독 해제 (메모리 누수 방지)
        if (m_OnApplicationPauseOrQuit != null)
        {
            m_OnApplicationPauseOrQuit.OnEventRaised -= RequestSave;
        }

        PlayerData.UnsubscribeEvents();
        MissionStatistics.UnsubscribeEvents();

        // 메모리 해제 및 참조 정리
        m_playerData = null;
        m_missionStatistics = null;
    }

    public void ValidateData()
    {
        m_playerData.ValidateData();
        m_onUpdatePlayerData?.RaiseEvent(PlayerData);
    }

    public void UpdateData(PlayerData loadedData, bool isOnEvent = true)
    {
        if (m_playerData != loadedData)
        {
            m_playerData.UpdateData(loadedData);

            if (isOnEvent)
            {
                m_onUpdatePlayerData?.RaiseEvent(PlayerData);
                Debug.Log($"<color=green>[{GetType().Name}]</color> PlayerData Updated successfully.");
            }
        }
    }

    public void UpdateData(List<string> statusIds)
    {
        m_missionStatistics.StatusTracker.UpdateData(statusIds);
        m_onUpdateMissionStat?.RaiseEvent(MissionStatistics);
        Debug.Log($"<color=green>[{GetType().Name}]</color> MissionStatisticsData Updated successfully.");
    }

    private void RequestLoad()
    {
        if (m_connetToServer)
        {
            Debug.Log($"<color=green>[{GetType().Name}]</color> 서버에 데이터 로드 요청 중...");
            m_onRequestLoad?.RaiseEvent(this);
        }
    }
    private void RequestSave()
    {
        if (m_connetToServer)
        {
            Debug.Log($"<color=green>[{GetType().Name}]</color> 서버에 데이터 저장 요청 중...");
            m_onRequestSave?.RaiseEvent(this);
        }
    }
}