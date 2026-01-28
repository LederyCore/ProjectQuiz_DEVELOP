using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/TravelStart Event Channel")]
public class TravelStartEventChannelSO : EventChannelSO<TravelData> { }


[System.Serializable]
public struct TravelData
{
    public Transform StartPlace;
    public Transform EndPlace;
    public int CurrentMoney;
    public int TravelCost; // 필요 시 비용 정보도 포함
    public string Name;
    public PlaceDataSO PlaceData;
    public CountryDataSO CountryData;
    public Action CallBack;
}