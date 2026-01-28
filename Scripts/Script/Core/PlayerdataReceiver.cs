using UnityEngine;

public abstract class PlayerdataReceiver : MonoBehaviour
{
    protected PlayerData m_PlayerData;

    public void UpdateData(PlayerData playerData)
    {
        m_PlayerData = playerData;
    }
}