using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class TravelConfirmationPanel : UIBase
{
    [Header("--- UI 연결 ---")]
    public TextMeshProUGUI txtMessage; // 안내 메시지 (도시 이름, 가격)
    public Button btnYes;              // 떠날래 버튼
    public Button btnNo;               // 아직 아니야 버튼

    [Space(10), Header("----- Event Channels (Publish) -----")]
    [SerializeField] private GameStateEventChannelSO m_OnRequestChangeGameState;
    [SerializeField] private IntEventChannelSO m_OnRequestChangePlayerMoney;
    [SerializeField] private VoidEventChannelSO m_OnFailPurchaseTravelPopup;
    [SerializeField] private TravelStartEventChannelSO m_OnStartTravel;

    private TravelData m_TravelData;

    private void Awake()
    {
        btnYes.onClick.AddListener(OnClickYes);
        btnNo.onClick.AddListener(OnClickNo);
    }

    public void SetData(TravelData data)
    {
        m_TravelData = data;
        txtMessage.text = $"{data.Name}로 이동하기 위해 {data.TravelCost} 원 이(가) 필요합니다.\r\n항공권을 구매할까요?";
    }

    private void OnClickNo()
    {
        m_OnRequestChangeGameState?.RaiseEvent(GameState.IDLE);
        gameObject.SetActive(false);
    }

    private void OnClickYes()
    {
        if (m_TravelData.CurrentMoney < m_TravelData.TravelCost)
        {
            Debug.Log($"항공권 구매 실패! 소지금이 부족합니다.");
            m_OnRequestChangeGameState?.RaiseEvent(GameState.IDLE);
            m_OnFailPurchaseTravelPopup?.RaiseEvent();
            gameObject.SetActive(false);
            return;
        }

        Debug.Log($"항공권 구매 완료! 목적지로 이동합니다.");
        m_TravelData.CallBack.Invoke();
        m_OnRequestChangeGameState?.RaiseEvent(GameState.TRAVELING);
        m_OnStartTravel?.RaiseEvent(m_TravelData);
        m_OnRequestChangePlayerMoney?.RaiseEvent(-m_TravelData.TravelCost);
        gameObject.SetActive(false);
    }
}