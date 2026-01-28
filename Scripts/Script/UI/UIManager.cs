using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private PlayerdataReceiver[] m_PlayerdataReceivers;

    public void HandleOnUpdatePlayerData(object data)
    {
        if (data is not PlayerData playerData)
        {
            Debug.LogWarning($"<color=#FF8000>[{GetType().Name}]</color> 플레이어 데이터 업데이트 이벤트 수신 시 전달된 인자가 PlayerData가 아닙니다.");
        }
        else
        {
            foreach (var item in m_PlayerdataReceivers)
            {
                item.UpdateData(playerData);
            }
        }
    }
}
