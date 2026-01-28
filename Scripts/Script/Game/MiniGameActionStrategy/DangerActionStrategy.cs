using UnityEngine;

[CreateAssetMenu(fileName = "DangerActionStrategy", menuName = "MiniGameActionStrategy/DangerActionStrategy")]
public class DangerActionStrategy : MiniGameActionStrategy
{
    [Header("----- Event Channels (Publish) -----")]
    [SerializeField] private VoidEventChannelSO m_OnRequestRemoveInventory;
    [SerializeField] private IntEventChannelSO m_OnRequestChangePlayerMoney;

    public override void Execute(EventDataSO data)
    {
        base.Execute(data);
        // 추가적인 위험 이벤트 처리 로직을 여기에 작성할 수 있습니다.

        if (data.specialEffectID == "STEAL_ARTIFACT")
        {
            m_OnRequestRemoveInventory.RaiseEvent();
        }

        m_OnRequestChangePlayerMoney.RaiseEvent(data.moneyAmount);
    }
}