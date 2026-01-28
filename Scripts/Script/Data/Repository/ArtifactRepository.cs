using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "ArtifactRepository", menuName = "Data/ArtifactRepository")]
public class ArtifactRepository : ScriptableObject
{
    [SerializeField] private List<ArtifactDataSO> m_artifacts = new();

    public List<ArtifactDataSO> Artifacts
    {
        get => m_artifacts;
        set => m_artifacts = value;
    }

    public ArtifactDataSO GetRandomArtifact()
    {
        if (m_artifacts == null || m_artifacts.Count == 0)
        {
            Debug.LogWarning("Artifact 리스트가 비어있습니다.");
            return null;
        }
        int randomIndex = Random.Range(0, m_artifacts.Count);
        return m_artifacts[randomIndex];
    }

    public ArtifactDataSO GetArtifactByID(string id)
    {
        // Null 체크와 ID 매칭을 동시에 수행
        return m_artifacts.FirstOrDefault(m => m != null && m.artifactID == id);
    }

    public List<string> GetAllArtifactIDs()
    {
        return m_artifacts
            .Where(m => m != null)
            .Select(m => m.artifactID)
            .ToList();
    }
}
