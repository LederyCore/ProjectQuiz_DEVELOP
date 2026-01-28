using System;
using UnityEngine;

[Serializable]
public class PlayerDataEventChannels
{
    [Space(5), Header("----- Event Channels (Subscribe) -----")]
    public IntEventChannelSO OnRequestChangePlayerMoney;
    public IntEventChannelSO OnRequestChangeCurrentDay;
    public IntEventChannelSO OnRequestChangeCurrentDayProgress;
    public IntEventChannelSO OnRequestChangeTotalCorrectQuizzes;
    public IntEventChannelSO OnRequestChangeIsFirstRun;
    public IntEventChannelSO OnRequestChangeScore;
    public UnityObjectEventChannelSO OnRequestAddTravelLog;
    public UnityObjectEventChannelSO OnRequestAddInventory;
    public VoidEventChannelSO OnRequestRemoveInventory;
    public IntEventChannelSO OnRequestChangeIsGuideFinished;

    [Space(5), Header("----- Event Channels (Publish) -----")]
    // PlayerData에서 접근할 수 있도록 public 혹은 internal로 선언합니다.
    public IntEventChannelSO OnChangedPlayerMoney;
    public IntEventChannelSO OnChangedMaximumMoney;
    public IntEventChannelSO OnChangedCurrentDay;
    public IntEventChannelSO OnChangedCurrentDayProgress;
    public IntEventChannelSO OnChangedTotalCorrectQuizzes;
    public IntEventChannelSO OnChangedIsFirstRun;
    public IntEventChannelSO OnChangedScore;
    public VoidEventChannelSO OnChangedVisitedPlaceIDs;
    public VoidEventChannelSO OnChangedVisitedCountryIDs;
    public CSharpObjectEventChannelSO OnChangedTravelLog;
    public VoidEventChannelSO OnChangedInventory;
    public IntEventChannelSO OnChangedIsGuideFinished;
}

[Serializable]
public class MissionStatisticsEventChannels
{
    [Space(5), Header("----- Event Channels (Subscribe) -----")]
    public CSharpObjectEventChannelSO OnUpdatePlayerData;

    [Space(5), Header("----- Event Channels (Publish) -----")]
    public IntEventChannelSO OnChangeDayCount;
    public IntEventChannelSO OnChangeCityCount;
    public IntEventChannelSO OnChangeCountryCount;
    public IntEventChannelSO OnChangeFlightCount;
    public IntEventChannelSO OnChangeTotalQuiz;
    public IntEventChannelSO OnChangeArtifactCount;
    public IntEventChannelSO OnChangeMoneyPossessed;
    public CSharpObjectEventChannelSO OnUpdateMissionStat;
    public BooleanEventChannelSO CanClaimMissionReward;
}