using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "MissionRepository", menuName = "Data/MissionRepository")]
public class MissionRepository : ScriptableObject
{
    [SerializeField] private List<MissionDataSO> m_missions = new();

    public List<MissionDataSO> Missions
    {
        get => m_missions;
        set => m_missions = value;
    }

    /// <summary>
    /// ID를 통해 특정 미션을 찾습니다.
    /// </summary>
    public MissionDataSO GetMissionByID(string id)
    {
        // Null 체크와 ID 매칭을 동시에 수행
        return m_missions.FirstOrDefault(m => m != null && m.Id == id);
    }


    /// <summary>
    /// (옵션) 모든 미션 ID 목록만 가져오기
    /// </summary>
    public List<string> GetAllMissionIDs()
    {
        return m_missions
            .Where(m => m != null)
            .Select(m => m.Id)
            .ToList();
    }
}