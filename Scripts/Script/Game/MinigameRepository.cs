using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MinigameRepository", menuName = "Data/MinigameRepository")]
public class MinigameRepository : DescriptionSO // 혹은 DescriptionSO
{
    [SerializeField] private List<GameObject> m_MinigamePrefab;

    // 생성된 인스턴스들을 관리할 리스트
    private List<MiniGame> m_MinigameInstanceCache = new List<MiniGame>();

    public void CreateInstanceAllMinigame(Transform parent, EventRepository repo)
    {
        m_MinigameInstanceCache.Clear();

        foreach (var minigamePrefab in m_MinigamePrefab)
        {
            // 1. 인스턴스화
            GameObject minigameInstance = Instantiate(minigamePrefab, parent);

            // 2. 부모의 첫 번째 인덱스(가장 위)로 이동
            // 이 코드를 통해 새로 생성된 객체가 항상 Hierarchy 상단에 위치하게 됩니다.
            minigameInstance.transform.SetAsFirstSibling();

            minigameInstance.SetActive(false);

            if (minigameInstance.TryGetComponent<MiniGame>(out var minigame))
            {
                minigame.EventDataRepository = repo;
                m_MinigameInstanceCache.Add(minigame);
            }
        }
    }

    public MiniGame OnActiveRandomMinigame()
    {
        MiniGame selectedMinigame = SelectRandomMinigame();

        if (selectedMinigame != null)
        {
            selectedMinigame.gameObject.SetActive(true);
        }

        return selectedMinigame;
    }

    private MiniGame SelectRandomMinigame()
    {
        // 캐시된 리스트가 비어있는지 확인
        if (m_MinigameInstanceCache == null || m_MinigameInstanceCache.Count == 0)
        {
            Debug.LogWarning("캐시된 미니게임 인스턴스가 없습니다. CreateInstanceAllMinigame을 먼저 호출했나요?");
            return null;
        }

        // 전체 인덱스 중 완전 랜덤하게 하나 선택
        int randomIndex = Random.Range(0, m_MinigameInstanceCache.Count);

        return m_MinigameInstanceCache[randomIndex];
    }
}