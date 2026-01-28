using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewCountry", menuName = "Card System/Country Data")]
public class CountryDataSO : ScriptableObject
{
    [Header("=== 국가 기본 정보 ===")]
    public string countryID;      // 예: Korea
    public string countryName_KR; // 예: 대한민국

    // [신규] 입국 스탬프 이미지 (배경 투명한 도장 이미지 권장)
    public Sprite stampImage;

    [Header("=== 이 국가의 공통 퀴즈 덱 ===")]
    public List<CardDataSO> countryQuizDeck;
}