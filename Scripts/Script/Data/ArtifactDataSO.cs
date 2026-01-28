using UnityEngine;

[CreateAssetMenu(fileName = "NewArtifact", menuName = "Card System/Artifact Data")]
public class ArtifactDataSO : ScriptableObject
{
    [Header("=== 유물 기본 정보 ===")]
    public string artifactID;     // ID (예: Art_KR_01)
    public string artifactName;   // 이름 (예: 금동미륵보살반가사유상)
    public Sprite artifactImage;  // 이미지

    [TextArea(3, 10)]
    public string description;    // 설명 (도감에서 보여줄 내용)

    [Header("=== 가치 및 등급 ===")]
    public string rarity;         // 등급 (Common, Rare, Legend)
    public int value;             // 가치 (나중에 상점에 팔 때 가격)
}