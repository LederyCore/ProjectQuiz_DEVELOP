using UnityEngine;


[CreateAssetMenu(fileName = "NewEventData", menuName = "Card System/Event Data", order = 2)]
public class EventDataSO : ScriptableObject
{
    [Header("=== 이벤트 기본 정보 ===")]
    public string eventID;      // CSV ID (Danger_00, Luck_01 등)
    public EventActionType eventType; // 타입
    public string title;        // 제목 (CSV의 Question 컬럼 사용)
    public Sprite eventImage;   // 이미지

    [TextArea(3, 5)]
    public string description;  // 설명

    [Header("=== 효과 설정 ===")]
    [Tooltip("양수(+)는 획득, 음수(-)는 차감")]
    public int moneyAmount;     // 금액 변동

    [Tooltip("특수 기능이 필요할 때 사용 (예: FREE_FLIGHT)")]
    public string specialEffectID;
}