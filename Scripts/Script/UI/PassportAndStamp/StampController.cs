using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// 이 컴포넌트는 여권창에서 스탬프 찍기 연출의 제어를 담당합니다.
/// </summary>
public class StampController : MonoBehaviour
{
    [SerializeField] private ToggleUISlideAnimator m_animator;


    // 스탬프 찍는 연출 함수
    public void StartStampSequence(Vector2 targetPos, Action onFinished = null, Action updateData = null)
    {
        Debug.Log("ShowEntryStampSequence called");
        m_animator.StartPos = targetPos;

        m_animator.Open(() =>
        {
            StartCoroutine(WaitAndCloseSequence(0.5f, onFinished));
            m_animator.Close(() => onFinished?.Invoke());
        },
        () =>
        {
            updateData?.Invoke();
        });
    }

    // 대기 및 종료를 처리하는 코루틴
    private IEnumerator WaitAndCloseSequence(float waitTime, Action onFinished)
    {
        // 1. 지정된 시간만큼 대기
        yield return new WaitForSeconds(waitTime);

        // 2. 닫기 애니메이션 실행
        m_animator.Close(() => onFinished?.Invoke());
    }
}
