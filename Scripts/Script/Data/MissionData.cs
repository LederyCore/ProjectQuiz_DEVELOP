using System;

[Serializable]
/// <summary>
/// 미션들의 상태를 저장하는 런타임용 데이터 클래스입니다.
/// </summary>
public class MissionData
{
    public string ID;
    public bool IsClaimed = false;
}
