
using UnityEngine;

[CreateAssetMenu(fileName = "ArtifactActionStrategy", menuName = "MiniGameActionStrategy/ArtifactActionStrategy")]
public class ArtifactActionStrategy : MiniGameActionStrategy
{
    [Header("----- Event Channels (Publish) -----")]
    [SerializeField] private UnityObjectEventChannelSO m_OnRequestAddInventory;

    [Header("----- Repository Inject -----")]
    [SerializeField] private ArtifactRepository m_ArtifactRepository;

    public override void Execute(EventDataSO data)
    {
        base.Execute(data);
        m_OnRequestAddInventory.RaiseEvent(m_ArtifactRepository.GetRandomArtifact());
    }
}