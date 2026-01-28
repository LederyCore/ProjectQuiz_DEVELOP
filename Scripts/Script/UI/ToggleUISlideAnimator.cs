using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class ToggleUISlideAnimator : MonoBehaviour
{
    [Header("--- 애니메이션 설정 ---")]
    [SerializeField] private float m_slideDuration = 0.5f;
    [SerializeField] private AnimationCurve m_slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Tooltip("애니메이션 진행률이 이 수치에 도달했을 때 중간 콜백을 실행합니다 (0~1)")]
    [Range(0f, 1f)]
    [SerializeField] private float m_triggerPoint = 0.95f; // 기본값: 95% 지점

    [Header("--- 위치 설정 ---")]
    [SerializeField] private Vector2 m_UIStartPos;
    [SerializeField] private Vector2 m_UIEndPos;

    private RectTransform m_UIRect;
    private Coroutine m_currentRoutine;

    public Vector2 StartPos { set { m_UIStartPos = value; } }
    public Vector2 EndPos { set { m_UIEndPos = value; } }

    private void Awake()
    {
        m_UIRect = GetComponent<RectTransform>();
        // 초기화 시 닫힌 위치로 설정
        m_UIRect.anchoredPosition = m_UIEndPos;
    }

    // updateData 파라미터 추가
    public void Open(Action onComplete = null, Action updateData = null)
        => PlaySlide(m_UIStartPos, onComplete, updateData);

    public void Close(Action onComplete = null, Action updateData = null)
        => PlaySlide(m_UIEndPos, onComplete, updateData);


    private void PlaySlide(Vector2 targetPos, Action onComplete, Action updateData)
    {
        if (gameObject.activeSelf == false) return;
        if (m_currentRoutine != null) StopCoroutine(m_currentRoutine);
        m_currentRoutine = StartCoroutine(SlidePanelRoutine(targetPos, onComplete, updateData));
    }

    private IEnumerator SlidePanelRoutine(Vector2 targetPos, Action onComplete, Action updateData)
    {
        Vector2 startPos = m_UIRect.anchoredPosition;
        float timer = 0f;

        // [중요] 중복 실행 방지용 플래그
        bool isDataUpdated = false;

        while (timer < m_slideDuration)
        {
            timer += Time.deltaTime;

            // 0 ~ 1 사이의 진행률 (t)
            float t = Mathf.Clamp01(timer / m_slideDuration);

            // 커브를 적용하여 위치 이동
            m_UIRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, m_slideCurve.Evaluate(t));

            // [핵심 로직]
            // 1. 아직 업데이트가 안 되었고 (!isDataUpdated)
            // 2. 현재 진행률(t)이 설정한 트리거 포인트(m_triggerPoint)를 넘었을 때
            if (!isDataUpdated && t >= m_triggerPoint)
            {
                isDataUpdated = true; // 플래그를 true로 바꿔서 다시 들어오지 못하게 함
                updateData?.Invoke(); // 콜백 실행
            }

            yield return null;
        }

        m_UIRect.anchoredPosition = targetPos;

        // 만약 루프가 너무 빨리 끝나서(프레임 드랍 등) 중간 콜백이 실행 안 된 경우 안전장치
        if (!isDataUpdated)
        {
            updateData?.Invoke();
        }

        onComplete?.Invoke();
    }
}