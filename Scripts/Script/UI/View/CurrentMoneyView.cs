using UnityEngine;
using TMPro;

public class CurrentMoneyView : MonoBehaviour
{
    [Header("--- UI 연결 ---")]
    public TextMeshProUGUI txtMoney; // 돈 텍스트 (예: 1,500 G)

    // 돈이 바뀔 때마다 호출되어 화면을 갱신합니다.
    public void HandleOnUpdatePlayerData(object obj)
    {
        if (obj is not PlayerData data) return;
        var amount = data.PlayerMoney;
        if (txtMoney != null)
        {
            // N0: 숫자 3자리마다 쉼표를 찍어줍니다. (예: 1000 -> 1,000)
            txtMoney.text = $"{amount:N0}";
        }
    }

    public void HandleOnChangePlayerMoney(int value)
    {
        txtMoney.text = $"{value:N0}";
    }
}