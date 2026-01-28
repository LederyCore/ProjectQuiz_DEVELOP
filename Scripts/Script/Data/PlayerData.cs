using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class PlayerData
{
    
    public PlayerData()
    {
        m_playerMoney = 0;
        m_maximumMoney = 0;
        m_currentDay = 1;
        m_currentDayProgress = 0;
        m_totalCorrectQuizzes = 0;
        m_isFirstRun = 1;
        m_score = 0;
        m_visitedPlaceIDs = new List<string>();
        m_visitedCountryIDs = new List<string>();
        m_travelLog = new List<string>();
        m_inventory = new List<ArtifactDataSO>();
    }

    // 추가 생성자 모든 직렬화 필드를 매개변수로 받음
    public PlayerData(int playerMoney, int maximumMoney, int currentDay, int currentDayProgress,
                      int totalCorrectQuizzes, int isFirstRun, int score,
                      List<string> visitedPlaceIDs, List<string> visitedCountryIDs,
                      List<string> travelLog, List<ArtifactDataSO> inventory, int isGuideFinished)
    {
        m_playerMoney = playerMoney;
        m_maximumMoney = maximumMoney;
        m_currentDay = currentDay;
        m_currentDayProgress = currentDayProgress;
        m_totalCorrectQuizzes = totalCorrectQuizzes;
        m_isFirstRun = isFirstRun;
        m_score = score;

        m_visitedPlaceIDs = visitedPlaceIDs ?? new List<string>();
        m_visitedCountryIDs = visitedCountryIDs ?? new List<string>();
        m_travelLog = travelLog ?? new List<string>();
        m_inventory = inventory ?? new List<ArtifactDataSO>();

        m_isGuideFinished = isGuideFinished;
    }

    // [필드 영역 생략: 기존과 동일]
    [SerializeField] private int m_playerMoney;
    [SerializeField] private int m_maximumMoney;
    [SerializeField] private int m_currentDay = 1;
    [SerializeField] private int m_currentDayProgress = 0;
    [SerializeField] private int m_totalCorrectQuizzes = 0;
    [SerializeField] private int m_isFirstRun = 1;
    [SerializeField] private int m_score = 0;
    [SerializeField] private List<string> m_visitedPlaceIDs = new();
    [SerializeField] private List<string> m_visitedCountryIDs = new();
    [SerializeField] private List<string> m_travelLog = new();
    [SerializeField] private List<ArtifactDataSO> m_inventory = new();
    [SerializeField] private int m_isGuideFinished = 1;

    private PlayerDataEventChannels m_eventChannels;
    private CSharpObjectEventChannelSO m_OnUpdatePlayerData;

    public PlayerDataEventChannels Event
    {
        set
        {
            UnsubscribeEvents();
            m_eventChannels = value;
            if (m_eventChannels != null) SubscribeEvents();
        }
    }

    public CSharpObjectEventChannelSO OnUpdatePlayerData
    {
        set => m_OnUpdatePlayerData = value;
    }

    // --- 프로퍼티 및 내부 로직 (기존과 동일) ---
    public int PlayerMoney
    {
        get => m_playerMoney;
        private set
        {
            if (m_playerMoney == value) return;
            m_playerMoney = value;

            if (m_playerMoney > m_maximumMoney)
                MaximumMoney = m_playerMoney;

            m_eventChannels?.OnChangedPlayerMoney?.RaiseEvent(m_playerMoney);
        }
    }
    public int MaximumMoney
    {
        get => m_maximumMoney;
        private set
        {
            if (m_maximumMoney == value) return;
            m_maximumMoney = value;
            m_eventChannels?.OnChangedMaximumMoney?.RaiseEvent(m_maximumMoney);
        }
    }
    public int CurrentDay
    {
        get => m_currentDay;
        private set
        {
            if (m_currentDay == value) return;
            m_currentDay = value;
            m_eventChannels?.OnChangedCurrentDay?.RaiseEvent(m_currentDay);
        }
    }
    public int CurrentDayProgress
    {
        get => m_currentDayProgress;
        private set
        {
            if (m_currentDayProgress == value) return;
            m_currentDayProgress = value;

            if (m_currentDayProgress % 3 == 0)
            {
                CurrentDay += 1;
                m_currentDayProgress = 0;
            }

            m_eventChannels?.OnChangedCurrentDayProgress?.RaiseEvent(m_currentDayProgress);
        }
    }
    public string CurrentPlaceID
    {
        get
        {
            if (TravelLog.Count != 0 && TravelLog != null)
                return TravelLog.LastOrDefault();
            return string.Empty;
        }
    }
    public int TotalCorrectQuizzes
    {
        get => m_totalCorrectQuizzes;
        private set
        {
            if (m_totalCorrectQuizzes == value) return;
            m_totalCorrectQuizzes = value;
            Score = m_score + UnityEngine.Random.Range(5, 20); // 예: 퀴즈당 5 ~ 20점
            m_eventChannels?.OnChangedTotalCorrectQuizzes?.RaiseEvent(m_totalCorrectQuizzes);
        }
    }
    public int IsFirstRun
    {
        get => m_isFirstRun;
        set
        {
            if (m_isFirstRun == value) return;
            m_isFirstRun = value;
            m_eventChannels?.OnChangedIsFirstRun?.RaiseEvent(m_isFirstRun);
        }
    }
    public int Score
    {
        get => m_score;
        private set
        {
            if (m_score == value) return;
            m_score = value;
            m_eventChannels?.OnChangedScore?.RaiseEvent(m_score);
        }
    }
    public List<string> VisitedPlaceIDs
    {
        get => m_visitedPlaceIDs;
        private set
        {
            if (m_visitedPlaceIDs == value) return;
            m_visitedPlaceIDs = value;
            m_eventChannels?.OnChangedVisitedPlaceIDs?.RaiseEvent();
        }
    }
    public List<string> VisitedCountryIDs
    {
        get => m_visitedCountryIDs;
        private set
        {
            if (m_visitedCountryIDs == value) return;
            m_visitedCountryIDs = value;
            m_eventChannels?.OnChangedVisitedCountryIDs?.RaiseEvent();
        }
    }
    public List<string> TravelLog
    {
        get => m_travelLog;
        private set
        {
            if (m_travelLog == value) return;
            m_travelLog = value;
            m_eventChannels?.OnChangedTravelLog?.RaiseEvent(m_travelLog);
        }
    }
    public List<ArtifactDataSO> Inventory
    {
        get => m_inventory;
        private set
        {
            if (m_inventory == value) return;
            m_inventory = value;
            m_eventChannels?.OnChangedInventory?.RaiseEvent();
        }
    }
    public int IsGuideFinished
    {
        get => m_isGuideFinished;
        private set
        {
            if (m_isGuideFinished == value) return;
            m_isGuideFinished = value;
            m_eventChannels?.OnChangedIsGuideFinished?.RaiseEvent(m_isGuideFinished);
        }
    }



    // --- 이벤트 구독 관리 (변경된 시그니처 반영) ---
    private void SubscribeEvents()
    {
        if (m_eventChannels == null) return;
        var evt = m_eventChannels;

        // 수치 변경 요청
        evt.OnRequestChangePlayerMoney.OnEventRaised += HandleChangeMoney;
        evt.OnRequestChangeCurrentDay.OnEventRaised += HandleChangeCurrentDay;
        evt.OnRequestChangeCurrentDayProgress.OnEventRaised += HandleChangeDayProgress;
        evt.OnRequestChangeTotalCorrectQuizzes.OnEventRaised += HandleChangeTotalQuizzes;
        evt.OnRequestChangeIsFirstRun.OnEventRaised += HandleChangeIsFirstRun;
        evt.OnRequestChangeScore.OnEventRaised += HandleChangeScore;

        // 리스트 추가/제거 요청
        evt.OnRequestAddTravelLog.OnEventRaised += HandleAddTravelLog;
        evt.OnRequestAddInventory.OnEventRaised += HandleAddInventoryRequest;
        evt.OnRequestRemoveInventory.OnEventRaised += HandleRemoveInventoryRequest;

        evt.OnRequestChangeIsGuideFinished.OnEventRaised += HandleChangeIsGuideFinished;
    }
    public void UnsubscribeEvents()
    {
        if (m_eventChannels == null) return;
        var evt = m_eventChannels;

        evt.OnRequestChangePlayerMoney.OnEventRaised -= HandleChangeMoney;
        evt.OnRequestChangeCurrentDay.OnEventRaised -= HandleChangeCurrentDay;
        evt.OnRequestChangeCurrentDayProgress.OnEventRaised -= HandleChangeDayProgress;
        evt.OnRequestChangeTotalCorrectQuizzes.OnEventRaised -= HandleChangeTotalQuizzes;
        evt.OnRequestChangeIsFirstRun.OnEventRaised -= HandleChangeIsFirstRun;
        evt.OnRequestChangeScore.OnEventRaised -= HandleChangeScore;

        evt.OnRequestAddTravelLog.OnEventRaised -= HandleAddTravelLog;
        evt.OnRequestAddInventory.OnEventRaised -= HandleAddInventoryRequest;
        evt.OnRequestRemoveInventory.OnEventRaised -= HandleRemoveInventoryRequest;

        evt.OnRequestChangeIsGuideFinished.OnEventRaised -= HandleChangeIsGuideFinished;
    }

    // --- 명시적 요청 핸들러 (Handler Methods) ---
    private void HandleChangeMoney(int amount)
    {
        PlayerMoney += amount;
        m_OnUpdatePlayerData?.RaiseEvent(this);
    }
    private void HandleChangeCurrentDay(int day)
    {
        CurrentDay += day;
        m_OnUpdatePlayerData?.RaiseEvent(this);
    }
    private void HandleChangeDayProgress(int progress)
    {
        CurrentDayProgress += progress;
        m_OnUpdatePlayerData?.RaiseEvent(this);
    }
    private void HandleChangeTotalQuizzes(int count)
    {
        TotalCorrectQuizzes += count;
        m_OnUpdatePlayerData?.RaiseEvent(this);
    } 
    private void HandleChangeIsFirstRun(int state)
    {
        IsFirstRun = state;
        m_OnUpdatePlayerData?.RaiseEvent(this);
    }
    private void HandleChangeScore(int score)
    {
        Score += score;
        m_OnUpdatePlayerData?.RaiseEvent(this);
    }
    private void HandleAddTravelLog(object data)
    {
        if (data is PlaceDataSO placeData)
        {
            var placeId = placeData.placeID;
            var countryId = placeData.countryID;

            AddVisitRecord(placeId, countryId);
        }
        m_OnUpdatePlayerData?.RaiseEvent(this);
    }
    private void HandleAddInventoryRequest(Object obj)
    {
        if (obj is ArtifactDataSO artifact) AddArtifact(artifact);
    }
    private void HandleRemoveInventoryRequest()
    {
        var ranomIndex = UnityEngine.Random.Range(0, m_inventory.Count);
        RemoveArtifact(Inventory[ranomIndex]);
    }
    private void HandleChangeIsGuideFinished(int state)
    {
        IsGuideFinished = state;
        m_OnUpdatePlayerData?.RaiseEvent(this);
    }
    private void AddVisitRecord(string placeID, string countryID)
    {
        Debug.Log($"<color=cyan>[TravelLog]</color> Adding visit record - PlaceID: {placeID}, CountryID: {countryID}");
        bool isChanged = false;

        // 1. 장소 방문 기록 (중복 체크 제거, 방문할 때마다 기록)
        if (!string.IsNullOrEmpty(placeID))
        {
            // 처음 가본 장소라면 '최초 방문 목록'에도 추가 (통계용 등으로 활용 가능)
            if (!m_visitedPlaceIDs.Contains(placeID))
            {
                m_visitedPlaceIDs.Add(placeID);
                m_eventChannels?.OnChangedVisitedPlaceIDs?.RaiseEvent();
            }

            // 방문할 때마다 로그에 추가 (장소는 여러 번 기록될 수 있음)
            m_travelLog.Add(placeID);
            isChanged = true;
        }

        // 2. 국가 방문 기록 (국가는 처음 방문했을 때만 기록)
        if (!string.IsNullOrEmpty(countryID) && !m_visitedCountryIDs.Contains(countryID))
        {
            m_visitedCountryIDs.Add(countryID);
            m_eventChannels?.OnChangedVisitedCountryIDs?.RaiseEvent();
            isChanged = true;
        }

        // 변화가 있다면 전체 로그 갱신 이벤트 발생
        if (isChanged) m_eventChannels?.OnChangedTravelLog?.RaiseEvent(m_travelLog);
    }
    private void AddArtifact(ArtifactDataSO artifact)
    {
        if (artifact == null) return;
        m_inventory.Add(artifact);
        m_eventChannels?.OnChangedInventory?.RaiseEvent();
        Debug.Log($"<color=yellow>[Inventory]</color> Added: {artifact.artifactName}");
    }
    private void RemoveArtifact(ArtifactDataSO artifact)
    {
        if (artifact == null) return;
        if (m_inventory.Remove(artifact))
        {
            m_eventChannels?.OnChangedInventory?.RaiseEvent();
            Debug.Log($"<color=red>[Inventory]</color> Removed: {artifact.artifactName}");
        }
    }
    public void ClearInventory()
    {
        if (m_inventory.Count > 0)
        {
            m_inventory.Clear();
            m_eventChannels?.OnChangedInventory?.RaiseEvent();
        }
    }


    internal void ValidateData()
    {
        var evt = m_eventChannels;
        evt.OnChangedPlayerMoney?.RaiseEvent(PlayerMoney);
        if (PlayerMoney > MaximumMoney)
        {
            MaximumMoney = PlayerMoney;
            evt.OnChangedMaximumMoney?.RaiseEvent(MaximumMoney);
        }
        evt.OnChangedCurrentDay?.RaiseEvent(CurrentDay);
        evt.OnChangedCurrentDayProgress?.RaiseEvent(CurrentDayProgress);
        evt.OnChangedTotalCorrectQuizzes?.RaiseEvent(TotalCorrectQuizzes);
        evt.OnChangedIsFirstRun?.RaiseEvent(IsFirstRun);
        evt.OnChangedScore?.RaiseEvent(Score);
        evt.OnChangedVisitedPlaceIDs?.RaiseEvent();
        evt.OnChangedVisitedCountryIDs?.RaiseEvent();
        evt.OnChangedTravelLog?.RaiseEvent(m_travelLog);
        evt.OnChangedInventory?.RaiseEvent();
        evt.OnChangedIsGuideFinished?.RaiseEvent(IsGuideFinished);
    }

    internal void UpdateData(PlayerData loadedData)
    {
        PlayerMoney = loadedData.PlayerMoney;
        MaximumMoney = loadedData.MaximumMoney;
        CurrentDay = loadedData.CurrentDay;
        CurrentDayProgress = loadedData.CurrentDayProgress;
        TotalCorrectQuizzes = loadedData.TotalCorrectQuizzes;
        IsFirstRun = loadedData.IsFirstRun;
        Score = loadedData.Score;
        VisitedPlaceIDs = loadedData.VisitedPlaceIDs;
        VisitedCountryIDs = loadedData.VisitedCountryIDs;
        TravelLog = loadedData.TravelLog;
        Inventory = loadedData.Inventory;
        IsGuideFinished = loadedData.IsGuideFinished;
    }
}