
using UnityEngine;

[CreateAssetMenu(fileName = "EventActionStrategy", menuName = "MiniGameActionStrategy/EventActionStrategy")]
public class EventActionStrategy : MiniGameActionStrategy
{
    [Header("----- Event Channels (Publish) -----")]
    [SerializeField] private IntEventChannelSO m_OnRequestChangePlayerMoney;


    public override void Execute(EventDataSO data)
    {
        base.Execute(data);
        m_OnRequestChangePlayerMoney.RaiseEvent(data.moneyAmount);
    }
}