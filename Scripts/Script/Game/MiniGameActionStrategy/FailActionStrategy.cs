using UnityEngine;

[CreateAssetMenu(fileName = "FailActionStrategy", menuName = "MiniGameActionStrategy/FailActionStrategy")]
public class FailActionStrategy : MiniGameActionStrategy
{
    public override void Execute(EventDataSO data)
    {
        base.Execute(data);
    }
}