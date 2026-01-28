using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System; // TextMeshPro 사용을 위해 필수

public class SolutionPanelController : MonoBehaviour
{
    [Header("--- UI 연결 ---")]
    public Button btnClose;          // '확인' 또는 '다음' 버튼
    public TextMeshProUGUI txtTitle; // (선택) "정답입니다!" 같은 제목
    public TextMeshProUGUI txtExplanation; // ★ 해설이 들어갈 텍스트

    private Action m_CheckQuizProgressCallback;

    void Start()
    {
        if (btnClose != null)
        {
            btnClose.onClick.RemoveAllListeners();
            btnClose.onClick.AddListener(OnCloseClicked);
        }
        else
        {
            Debug.LogError("🚨 [SolutionPanel] 닫기 버튼(Btn Close)이 연결되지 않았습니다!");
        }
    }

    // ★ 외부(매니저)에서 이 함수를 불러서 해설을 셋팅합니다.
    public void Setup(bool isCorrect, string explanation, Action callback)
    {
        m_CheckQuizProgressCallback = callback;

        // 1. 제목 설정 (정답/오답 여부에 따라 다르게 표시 가능)
        if (txtTitle != null)
        {
            txtTitle.text = isCorrect ? "정답입니다!" : "오답입니다...";
            txtTitle.color = isCorrect ? Color.green : Color.red;
        }

        // 2. 해설 텍스트 설정
        if (txtExplanation != null)
        {
            // 데이터에 해설이 비어있으면 기본 문구 출력
            if (string.IsNullOrEmpty(explanation))
            {
                txtExplanation.text = "해설 데이터가 없습니다.";
            }
            else
            {
                txtExplanation.text = explanation;
            }
        }
    }

    private void OnCloseClicked()
    {
        // 3. 닫기 버튼 클릭 시 콜백 호출
        m_CheckQuizProgressCallback?.Invoke();
        gameObject.SetActive(false); // 패널 닫기
    }
}