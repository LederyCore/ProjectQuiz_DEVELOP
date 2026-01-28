using TMPro;
using UnityEngine;

public class CurrentScoreView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_Score;

    public void HandleOnChangedCurrentDay(object obj)
    {
        if (obj is not PlayerData data) return;

        // TODO 점수 필드 추가
        //m_Score.text = $"{data.Score}Days";
    }
}
