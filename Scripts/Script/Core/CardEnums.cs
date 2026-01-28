using UnityEngine;

public enum CardType
{
    Quiz,       // 퀴즈
    Danger,     // 위험 (돈 잃음)
    Artifact,   // 유물 (데이터 분류용으로 남겨둠, 덱에서는 안 쓰임)
    Luck,       // 행운 (돈 얻음)
    Event       // 특수 이벤트 (거지 등)
    // MiniGame <- 삭제함
}