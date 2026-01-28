using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewCard", menuName = "Card System/Card Data", order = 1)]
public class CardDataSO : ScriptableObject
{
    [Header("=== 1. 기본 정보 ===")]
    public CardType cardType; // "Quiz" 고정
    public string CompositeID; // 카드 고유 ID (예: CAE10)
    public string cardName;   // 파일명 (ID)
    public Sprite cardImage;  // 문제 이미지

    [Header("=== 2. 퀴즈 데이터 ===")]
    [TextArea(3, 5)]
    public string question;   // 문제 내용

    [TextArea(3, 5)]
    public string explanation; // 정답 해설

    [Header("=== 3. 정답 및 오답 ===")]
    [Tooltip("여기에 정답을 적으세요")]
    public string correctAnswer;

    [Tooltip("오답 (4지선다용, OX는 비워둠)")]
    public string wrongAnswer1;
    public string wrongAnswer2;
    public string wrongAnswer3;
}
