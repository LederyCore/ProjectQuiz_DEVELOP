using TMPro;
using UnityEngine;

public class CurrentDayView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_Day;

    public void HandleOnChangedCurrentDay(object obj)
    {
        if (obj is not PlayerData data) return;

        m_Day.text = $"{data.CurrentDay}Days";
    }
}
