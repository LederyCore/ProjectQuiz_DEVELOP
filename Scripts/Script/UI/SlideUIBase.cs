using UnityEngine;

/// <summary>
/// 이 컴포넌트는 슬라이드 연출을 나타나는 UI들의 공통 요소를 정의합니다.
/// </summary>
public abstract class SlideUIBase : UIBase
{
    [SerializeField] protected ToggleUISlideAnimator m_animator;

    public override void OpenPanel()
    {
        base.OpenPanel();

        // 애니메이터에게 위임
        if (m_animator != null) m_animator.Open();
        else gameObject.SetActive(true); // 애니메이터 없으면 그냥 켜기
    }

    public override void ClosePanel()
    {
        base.ClosePanel();

        if (m_animator != null) m_animator.Close(() => gameObject.SetActive(false));
        else gameObject.SetActive(false);
    }
}