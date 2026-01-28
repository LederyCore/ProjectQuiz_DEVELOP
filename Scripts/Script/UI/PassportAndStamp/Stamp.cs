using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 이 컴포넌트는 스탬프 개별 요소의 제어를 담당합니다.
/// </summary>
public class Stamp : MonoBehaviour
{
    [SerializeField] private Image m_stampImage;

    public int DataIndex { get; set; }

    /// <summary>
    /// 데이터를 갱신하고 이미지를 로드합니다.
    /// </summary>
    public void UpdateData(List<string> visitedCountries, PlaceRepository repo)
    {
        // 1. 기초 데이터 유효성 검사
        if (visitedCountries == null || repo == null)
        {
            Debug.LogError($"[Stamp] PlayerData 또는 Repository가 Null입니다.");
            return;
        }

        // 2. 인덱스 범위 확인 (안전성 확보)
        if (DataIndex < 0 || DataIndex >= visitedCountries.Count)
        {
            Debug.LogWarning($"[Stamp] 인덱스 {DataIndex}가 방문 목록 범위를 벗어났습니다.");
            return;
        }

        string countryId = visitedCountries[DataIndex];

        // 3. Repository 검색 결과 확인 (가장 의심되는 구간)
        var countryData = repo.GetCountryByCountryID(countryId);

        if (countryData == null)
        {
            Debug.LogError($"[Stamp] ID: {countryId}에 해당하는 국가 데이터를 찾을 수 없습니다.");
            m_stampImage.enabled = false; // 이미지 컴포넌트 비활성화 등 처리
            return;
        }

        // 4. 최종 할당 (삼항 연산자보다 가독성 높은 구조)
        Sprite loadSprite = countryData.stampImage;

        if (m_stampImage != null)
        {
            m_stampImage.sprite = loadSprite;
            m_stampImage.enabled = (loadSprite != null);
        }
    }
}
