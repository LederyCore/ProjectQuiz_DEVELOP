using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionSlot : MonoBehaviour
{
    [Header("--- UI 연결 ---")]
    [SerializeField] private TextMeshProUGUI m_txtTitle;
    [SerializeField] private TextMeshProUGUI m_txtDesc;
    [SerializeField] private TextMeshProUGUI m_txtProgress;
    [SerializeField] private TextMeshProUGUI m_txtReward;
    [SerializeField] private Slider m_sliderGauge;
    [SerializeField] private Button m_Button;
    [SerializeField] private TextMeshProUGUI m_btnText;
    [SerializeField] private GameObject m_CheckIcon;
    [SerializeField] private Sprite m_ReadySprite;
    [SerializeField] private Sprite m_RewardSprite;
    [SerializeField] private Sprite m_CheckSprite;

    public int DataIndex { get; set; }

    private MissionDataSO m_Data;
    private MissionStatisticsData m_CurrentStat;

    private void OnEnable()
    {
        m_Button.onClick.AddListener(OnClickClaimReward);
    }

    private void OnDisable()
    {
        m_Button.onClick.RemoveListener(OnClickClaimReward);
    }

    public void UpdateData(MissionDataSO data, MissionStatisticsData stat)
    {
        m_Data = data;
        m_CurrentStat = stat;
        UpdateUI(data, stat);
    }

    public void UpdateData(MissionStatisticsData stat)
    {
        m_CurrentStat = stat;
        UpdateUI(m_Data, stat);
    }

    private void UpdateUI(MissionDataSO data, MissionStatisticsData stat)
    {
        if (data == null || stat == null) return;

        int rawValue = data.GetCurrentValue(stat);
        int targetValue = data.Target;

        // 1. 이미 보상을 받았는지 확인 (StatusTracker 활용)
        bool isAlreadyClaimed = stat.StatusTracker.IsCompleted(data.Id);
        // 2. 목표 수치에 도달했는지 확인
        bool isReached = rawValue >= targetValue;

        // UI 표현용 값 고정
        int clampedValue = Mathf.Clamp(rawValue, 0, targetValue);
        m_txtProgress.text = $"{clampedValue:N0} / {targetValue:N0}";
        m_sliderGauge.value = (float)clampedValue / targetValue;

        // 기본 정보 갱신
        m_txtTitle.text = data.Title;
        m_txtDesc.text = data.Description;
        m_txtReward.text = data.RewardMoney.ToString("N0");

        // 3. 상태에 따른 버튼 제어
        RefreshButtonState(isAlreadyClaimed, isReached);
    }

    private void RefreshButtonState(bool isAlreadyClaimed, bool isReached)
    {
        if (isAlreadyClaimed)
        {
            m_Button.interactable = false;
            m_Button.image.sprite = m_CheckSprite;
            m_btnText.text = "";
            m_CheckIcon.SetActive(true);
        }
        else if (isReached)
        {
            m_Button.interactable = true;
            m_Button.image.sprite = m_RewardSprite;
            m_btnText.text = "Reward";
            m_CheckIcon.SetActive(false);
        }
        else
        {
            m_Button.interactable = false;
            m_Button.image.sprite = m_ReadySprite;
            m_btnText.text = "Reward";
            m_CheckIcon.SetActive(false);
        }
    }

    // 버튼의 OnClick 이벤트에 연결
    public void OnClickClaimReward()
    {
        if (m_Data == null || m_CurrentStat == null) return;

        // 통계 데이터에 보상 수령 사실을 알림
        // 이 안에서 StatusTracker에 ID가 추가되고 OnUpdateMissionStat 이벤트가 발생함
        m_CurrentStat.ClaimMission(m_Data.Id);

        // TODO: 여기서 실제 재화(Money) 증가 로직이나 인벤토리 추가 로직을 연결하세요.
        Debug.Log($"[Mission] {m_Data.Title} 보상 수령!");
    }
}