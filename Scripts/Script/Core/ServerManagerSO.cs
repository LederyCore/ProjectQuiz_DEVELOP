using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object; // ObjectEventChannelSO와의 호환성

[CreateAssetMenu(fileName = "ServerManagerSO", menuName = "ManagerSO/ServerManagerSO")]
public class ServerManagerSO : ManagerSO
{
    public static string UserId = "test";

    [Space(10), Header("----- Event Channels (Subscribe) -----")]
    [Tooltip("DataManagerSO 참조를 담아 전달받는 이벤트 채널")]
    [SerializeField] private UnityObjectEventChannelSO m_onRequestLoad;
    [SerializeField] private UnityObjectEventChannelSO m_onRequestSave;
    [SerializeField] private CSharpObjectEventChannelSO m_onUpdatePlayerData;

    [Space(10), Header("----- Event Channels (Publish) -----")]
    [SerializeField] private VoidEventChannelSO m_onCompleteLoadData;

    // --- 생명주기 제어 ---

    public override void Init()
    {
        if (m_onRequestLoad != null) m_onRequestLoad.OnEventRaised += HandleLoadRequest;
        if (m_onRequestSave != null) m_onRequestSave.OnEventRaised += HandleSaveRequest;

        // 보라색 로그 적용
        Debug.Log($"<color=#A020F0>[{GetType().Name}]</color> Initialized.");
    }

    public override void Destroy()
    {
        if (m_onRequestLoad != null) m_onRequestLoad.OnEventRaised -= HandleLoadRequest;
        if (m_onRequestSave != null) m_onRequestSave.OnEventRaised -= HandleSaveRequest;
    }

    // --- 이벤트 핸들러 (참조를 매개변수로 받음) ---

    private async void HandleLoadRequest(Object sender)
    {
        if (sender is DataManagerSO dataManager)
        {
            await LoadFromServerAsync(dataManager);
        }
    }

    private async void HandleSaveRequest(Object sender)
    {
        if (sender is DataManagerSO dataManager)
        {
            await SaveToServerAsync(dataManager);
        }
    }

    // --- 핵심 통신 로직 (Task 기반) ---

    public async Task LoadFromServerAsync(DataManagerSO dataManager)
    {
        Debug.Log($"<color=#A020F0>[{GetType().Name}]</color> ⬇️ 데이터 로드 시작");
        PlayerDataGetRequest  request = new PlayerDataGetRequest
        {
            user_id = UserId
        };
        var result = await WebRequestManager.PostAsync<PlayerDataGetRequest, PlayerDataDTO>(API.URL + API.LOAD_PLAYER_DATA, request);

        if (result.Data == null)
        {
            Debug.LogError($"<color=#A020F0>[{GetType().Name}]</color> ❌ 데이터 로드 실패: {result.ErrorMessage}");
        }

        ProcessLoadedData(result.Data, dataManager);
    }

    private void ProcessLoadedData(PlayerDataDTO data, DataManagerSO dataManager)
    {
        try
        {
            var dtoStatusTracker = new List<string>();
            var statusIds = SplitCsv(data.status_tracker);
            foreach (var id in statusIds)
            {
                dtoStatusTracker.Add(id);
            }


            // DTO -> 도메인 모델 변환 (CSV 파싱 및 Repository 활용)

            var inventory = new List<ArtifactDataSO>();
            var artIds = SplitCsv(data.my_artifacts);
            foreach (var id in artIds)
            {
                var found = dataManager.ArtifactRepository.GetArtifactByID(id);
                if (found != null) inventory.Add(found);
            }

            var loadedPlayerData = new PlayerData(
                data.player_money,
                data.maximum_money, // TODO 최대 금액 DB 연동 필요
                data.current_day,
                data.day_progress,
                data.total_correct_quizzes,
                data.is_first_run,
                data.score, // TODO DB 연동 필요
                SplitCsv(data.visited_places),
                SplitCsv(data.visited_countries),
                SplitCsv(data.travel_log),
                inventory,
                data.is_guide_finished);
            
            // 데이터 적용 (넘겨받은 참조를 직접 사용)
            dataManager.UpdateData(loadedPlayerData, false);
            dataManager.UpdateData(dtoStatusTracker);

            m_onCompleteLoadData?.RaiseEvent();
            Debug.Log($"<color=#A020F0>[{GetType().Name}]</color> ✅ 데이터 로드 및 적용 완료. 플레이어: {loadedPlayerData}");
        }
        catch (Exception e)
        {
            Debug.LogError($"<color=#A020F0>[{GetType().Name}]</color> ❌ JSON 파싱 에러: {e.Message}");
        }
    }

    public async Task SaveToServerAsync(DataManagerSO dataManager)
    {
        var playerData = dataManager.PlayerData;
        if (playerData == null) return;

        UpdatePlayerDataRequest request = new UpdatePlayerDataRequest
        {
            user_id = UserId,
            player_money = playerData.PlayerMoney.ToString(),
            maximum_money = playerData.MaximumMoney,
            current_day = playerData.CurrentDay,
            day_progress = playerData.CurrentDayProgress,
            total_correct_quizzes = playerData.TotalCorrectQuizzes,
            is_first_run = playerData.IsFirstRun,
            score = playerData.Score,
            visited_places = JoinCsv(playerData.VisitedPlaceIDs),
            visited_countries = JoinCsv(playerData.VisitedCountryIDs),
            travel_log = JoinCsv(playerData.TravelLog),
            my_artifacts = JoinCsv(playerData.Inventory.Select(a => a.artifactID).ToList()),
            is_guide_finished = playerData.IsGuideFinished,
            status_tracker = JoinCsv(dataManager.MissionStatistics.StatusTracker.CompletedMissionIds)
        };

        var result = await WebRequestManager.PostAsync<UpdatePlayerDataRequest, UpdatePlayerDataResponse>(API.URL + API.SAVE_PLAYER_DATA, request);

        if (result.IsSuccess)
        {
            Debug.Log($"<color=#A020F0>[{GetType().Name}]</color> ✅ 데이터 저장 완료.");
        }
        else
        {
            Debug.LogError($"<color=#A020F0>[{GetType().Name}]</color> ❌ 데이터 저장 실패: {result.ErrorMessage}");
        }
    }

    // --- 유틸리티 메서드 ---

    private List<string> SplitCsv(string csv)
    {
        if (string.IsNullOrEmpty(csv)) return new List<string>();
        return csv.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
    }

    private string JoinCsv(List<string> list)
    {
        if (list == null || list.Count == 0) return "";
        return string.Join(",", list.Where(s => !string.IsNullOrEmpty(s)));
    }


}