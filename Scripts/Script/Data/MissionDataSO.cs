using System;
using UnityEngine;


[CreateAssetMenu(fileName = "MissionDataSO", menuName = "Data/MissionDataSO")]
public class MissionDataSO : ScriptableObject
{
    public string Id;
    public string Title;
    public string Description;
    public MissionType Type;
    public int Target;
    public int RewardMoney;
    public string RewardArtifact;

    public int GetCurrentValue(MissionStatisticsData stats)
    {
        if (stats == null) return 0;

        // enum에 따라 stats의 적절한 프로퍼티를 리턴합니다.
        // 이 로직이 SO 안에 있으면 MissionSlot은 "어떤 데이터를 가져올지" 고민할 필요가 없습니다.
        return Type switch
        {
            MissionType.DayCount => stats.DayCount,
            MissionType.CityCount => stats.CityCount,
            MissionType.CountryCount => stats.CountryCount,
            MissionType.FlightCount => stats.FlightCount,
            MissionType.TotalQuiz => stats.TotalQuiz,
            MissionType.ArtifactCount => stats.ArtifactCount,
            MissionType.MoneyPossessed => stats.MoneyPossessed,
            _ => 0
        };
    }
}

public enum MissionType
{
    DayCount,
    CityCount,
    CountryCount,
    FlightCount,
    TotalQuiz,
    ArtifactCount,
    MoneyPossessed
}