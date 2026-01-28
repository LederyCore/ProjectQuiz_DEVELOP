using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MissionStatisticsData
{
    public MissionStatisticsData()
    {
        m_dayCount = 0;
        m_cityCount = 0;
        m_countryCount = 0;
        m_flightCount = 0;
        m_totalQuiz = 0;
        m_artifactCount = 0;
        m_moneyPossessed = 0;
        m_statusTracker = new();
    }
    public MissionStatisticsData(int dayCount, int cityCount, int countryCount, int flightCount, int totalQuiz, int artifactCount, int moneyPossessed)
    {
        m_dayCount = dayCount;
        m_cityCount = cityCount;
        m_countryCount = countryCount;
        m_flightCount = flightCount;
        m_totalQuiz = totalQuiz;
        m_artifactCount = artifactCount;
        m_moneyPossessed = moneyPossessed;
    }

    [SerializeField] private int m_dayCount;
    [SerializeField] private int m_cityCount;
    [SerializeField] private int m_countryCount;
    [SerializeField] private int m_flightCount;
    [SerializeField] private int m_totalQuiz;
    [SerializeField] private int m_artifactCount;
    [SerializeField] private int m_moneyPossessed;
    [SerializeField] private MissionStatusTracker m_statusTracker = new();  // 미션 수령 상태 추적기

    private MissionStatisticsEventChannels m_eventChannels;
    private MissionRepository m_missionRepository;

    public MissionStatisticsEventChannels Event
    {
        set
        {
            m_eventChannels = value;
        }
    }
    public MissionRepository MissionRepository
    {
        set => m_missionRepository = value;
    }
    public MissionStatusTracker StatusTracker => m_statusTracker;
    public int DayCount
    {
        get => m_dayCount;
        private set
        {
            if (m_dayCount == value) return;
            m_dayCount = value;
            m_eventChannels?.OnChangeDayCount.RaiseEvent(m_dayCount);
        }
    }

    public int CityCount
    {
        get => m_cityCount;
        private set
        {
            if (m_cityCount == value) return;
            m_cityCount = value;
            m_eventChannels?.OnChangeCityCount.RaiseEvent(m_cityCount);
        }
    }

    public int CountryCount
    {
        get => m_countryCount;
        private set
        {
            if (m_countryCount == value) return;
            m_countryCount = value;
            m_eventChannels?.OnChangeCountryCount.RaiseEvent(m_countryCount);
        }
    }

    public int FlightCount
    {
        get => m_flightCount;
        private set
        {
            if (m_flightCount == value) return;
            m_flightCount = value;
            m_eventChannels?.OnChangeFlightCount.RaiseEvent(m_flightCount);
        }
    }

    public int TotalQuiz
    {
        get => m_totalQuiz;
        private set
        {
            if (m_totalQuiz == value) return;
            m_totalQuiz = value;
            m_eventChannels?.OnChangeTotalQuiz.RaiseEvent(m_totalQuiz);
        }
    }

    public int ArtifactCount
    {
        get => m_artifactCount;
        private set
        {
            if (m_artifactCount == value) return;
            m_artifactCount = value;
            m_eventChannels?.OnChangeArtifactCount.RaiseEvent(m_artifactCount);
        }
    }

    public int MoneyPossessed
    {
        get => m_moneyPossessed;
        private set
        {
            if (m_moneyPossessed == value) return;
            m_moneyPossessed = value;
            m_eventChannels?.OnChangeMoneyPossessed.RaiseEvent(m_moneyPossessed);
        }
    }
    public void ClaimMission(string missionId)
    {
        if (m_statusTracker.IsCompleted(missionId)) return;

        m_statusTracker.MarkAsCompleted(missionId);

        // 보상을 받은 후에도 다른 미션이 남았는지 체크할 수 있음
        m_eventChannels?.CanClaimMissionReward.RaiseEvent(HasAnyClaimableMission());
        m_eventChannels?.OnUpdateMissionStat?.RaiseEvent(this);
    }

    public void SubscribeEvents()
    {
        if (m_eventChannels == null) return;
        var evt = m_eventChannels;

        evt.OnUpdatePlayerData.OnEventRaised += HandleChangeData;
    }

    public void UnsubscribeEvents()
    {
        if (m_eventChannels == null) return;
        var evt = m_eventChannels;
        evt.OnUpdatePlayerData.OnEventRaised -= HandleChangeData;
    }
    /// <summary>
    /// 보상을 받을 수 있는 미션이 단 하나라도 있는지 검사합니다.
    /// </summary>
    public bool HasAnyClaimableMission()
    {
        if (m_missionRepository == null || m_missionRepository.Missions == null)
            return false;

        // LINQ의 Any를 사용하거나 for문에서 return을 사용하여 즉시 종료
        foreach (var mission in m_missionRepository.Missions)
        {
            // 1. 이미 받은 미션은 제외
            if (m_statusTracker.IsCompleted(mission.Id)) continue;

            // 2. 목표 도달 확인
            if (mission.GetCurrentValue(this) >= mission.Target)
            {
                // 조건을 만족하는 것을 찾은 즉시 true를 반환하고 메서드를 종료(탈출)합니다.
                return true;
            }
        }

        return false;
    }
    private void HandleChangeData(object obj)
    {
        if (obj is not PlayerData data) return;

        // 1. 통계 데이터 업데이트
        UpdateStatistics(data);

        m_eventChannels?.CanClaimMissionReward.RaiseEvent(HasAnyClaimableMission());
        m_eventChannels?.OnUpdateMissionStat?.RaiseEvent(this);
    }
    private void UpdateStatistics(PlayerData data)
    {
        DayCount = data.CurrentDay;
        CityCount = data.VisitedPlaceIDs.Count;
        CountryCount = data.VisitedCountryIDs.Count;
        FlightCount = data.TravelLog.Count;
        TotalQuiz = data.TotalCorrectQuizzes;
        ArtifactCount = data.Inventory.Count;
        MoneyPossessed = data.MaximumMoney;
    }

    public void UpdateData(MissionStatusTracker tracker)
    {
        m_statusTracker = tracker;
    }
}

[Serializable]
public class MissionStatusTracker
{
    // 보상 수령이 완료된 미션 ID 목록
    [SerializeField] List<string> m_completedMissionIds = new();

    public List<string> CompletedMissionIds
    {
        get => m_completedMissionIds;
    }

    // 빠른 조회를 위한 해시셋 (런타임용)
    private HashSet<string> m_hashSet;

    private void PrepareHashSet()
    {
        if (m_hashSet == null)
            m_hashSet = new HashSet<string>(m_completedMissionIds);
    }

    public bool IsCompleted(string missionId)
    {
        PrepareHashSet();
        return m_hashSet.Contains(missionId);
    }

    public void MarkAsCompleted(string missionId)
    {
        PrepareHashSet();
        if (m_hashSet.Add(missionId))
        {
            m_completedMissionIds.Add(missionId);
        }
    }

    public void UpdateData(List<string> data)
    {
        // 서버로부터 받은 데이터로 상태를 갱신합니다.
        m_completedMissionIds = data;
        PrepareHashSet();
        m_hashSet.Clear();
        foreach (var id in m_completedMissionIds)
        {
            m_hashSet.Add(id);
        }
    }
}