using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class WelcomeCardController : MonoBehaviour
{
    [Header("--- UI 연결 ---")]
    public TextMeshProUGUI txtPlaceName;
    public TextMeshProUGUI txtMessage;
    public Image imgWelcome;

    [Header("--- 닫기 버튼 ---")]
    public Button btnNext;


    public void Setup(PlaceDataSO data, Action onClosed)
    {
        if (data == null)
        {
            Debug.LogError("🚨 [WelcomeCard] 전달받은 데이터가 없습니다! (NULL)");
            return;
        }

        // 중요: 이전 리스너들을 모두 제거하여 이벤트 중첩 방지 (유지보수 핵심)
        btnNext.onClick.RemoveAllListeners();
        btnNext.onClick.AddListener(() =>
        {
            gameObject.SetActive(false); // 카드 닫기
            onClosed?.Invoke();          // 등록된 콜백(WaitUntil 해제용) 실행
        });

        if (txtPlaceName != null) txtPlaceName.text = data.placeName_KR;
        if (txtMessage != null) txtMessage.text = data.welcomeMessage;

        if (imgWelcome != null)
        {
            if (data.welcomeImage != null)
            {
                imgWelcome.sprite = data.welcomeImage;
                imgWelcome.gameObject.SetActive(true);
            }
            else
            {
                imgWelcome.gameObject.SetActive(false);
            }
        }
    }
}