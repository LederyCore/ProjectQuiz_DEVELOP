
using UnityEngine;

[CreateAssetMenu(fileName = "LuckActionStrategy", menuName = "MiniGameActionStrategy/LuckActionStrategy")]
public class LuckActionStrategy : MiniGameActionStrategy
{
    [Header("----- Event Channels (Publish) -----")]
    [SerializeField] private IntEventChannelSO m_OnRequestChangePlayerMoney;

    public override void Execute(EventDataSO data)
    {
        base.Execute(data);

        m_OnRequestChangePlayerMoney.RaiseEvent(data.moneyAmount);
    }
}