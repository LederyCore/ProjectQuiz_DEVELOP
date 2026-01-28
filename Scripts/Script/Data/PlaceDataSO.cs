using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewPlace", menuName = "Card System/Place Data")]
public class PlaceDataSO : ScriptableObject
{
    [Header("=== 장소 기본 정보 ===")]
    public string placeID;

    [Tooltip("소속 국가 ID (글로벌 지역은 비워도 됨)")]
    public string countryID;

    public string regionName_KR;
    public string placeName_KR;

    [Header("=== 랜드마크 설정 ===")]
    [Tooltip("이 장소를 대표하는 랜드마크 프리팹 (없으면 비워도 됨)")]
    public GameObject landmarkPrefab;

    [Header("=== [신규] 글로벌 퀴즈 여부 ===")]
    [Tooltip("체크되면 비행기 이동과 여권 심사를 건너뛰고 바로 퀴즈가 시작됩니다.")]
    public bool isGlobal;

    [Header("=== 웰컴 카드 설정 ===")]
    [TextArea(3, 5)]
    public string welcomeMessage;
    public Sprite welcomeImage;

    [Header("=== 이 장소의 카드 덱 ===")]
    public List<CardDataSO> placeQuizDeck;

    [Header("=== 획득 가능한 유물 (보상용) ===")]
    public List<ArtifactDataSO> placeArtifactDeck;
}