using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance;

    [Header("--- UI 연결 ---")]
    public GameObject notificationPanel;   // 배경 패널 (Image)
    public TextMeshProUGUI txtMessage;     // 메시지 텍스트
    public CanvasGroup canvasGroup;        // 투명도 조절용 (없으면 패널에 Add Component)

    [Header("--- 설정 ---")]
    public float showDuration = 2.0f;      // 메시지가 떠있는 시간
    public float fadeDuration = 0.5f;      // 사라지는 시간

    void Awake()
    {
        if (Instance == null) Instance = this;
        // 씬 전환 시 파괴되지 않게 하려면 아래 주석 해제 (단, UI 캔버스 구조에 따라 주의)
        // DontDestroyOnLoad(gameObject); 
    }

    void Start()
    {
        // 시작할 때는 숨김
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
            if (canvasGroup != null) canvasGroup.alpha = 0;
        }
    }

    // 📢 외부에서 호출하는 함수
    public void Show(string message)
    {
        if (notificationPanel == null) return;

        // 기존에 떠있던 메시지가 있다면 코루틴 초기화
        StopAllCoroutines();

        txtMessage.text = message;
        notificationPanel.SetActive(true);

        // 투명도 1 (완전 불투명)
        if (canvasGroup != null) canvasGroup.alpha = 1f;

        StartCoroutine(FadeOutRoutine());
    }

    IEnumerator FadeOutRoutine()
    {
        // 1. 지정된 시간만큼 대기
        yield return new WaitForSeconds(showDuration);

        // 2. 서서히 사라지기 (Fade Out)
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            if (canvasGroup != null)
            {
                // alpha 값을 1 -> 0으로 부드럽게 줄임
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            }
            yield return null;
        }

        // 3. 완전히 꺼주기
        notificationPanel.SetActive(false);
    }
}