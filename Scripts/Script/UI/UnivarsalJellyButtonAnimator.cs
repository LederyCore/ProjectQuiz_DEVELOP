using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UnivarsalJellyButtonAnimator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [Header("Button Events")]
    [Tooltip("버튼을 누르는 순간 실행됩니다. (예: 사운드, 누름 이펙트)")]
    public UnityEvent OnDown;

    [Tooltip("버튼을 완전히 클릭했을 때 실행됩니다. (예: UI 오픈, 확인 로직)")]
    public UnityEvent OnClick;

    [Header("Animation Settings")]
    [SerializeField] private float shrinkScale = 0.92f;
    [SerializeField] private float overshootScale = 1.29f;
    [SerializeField] private float downDuration = 0.1f;
    [SerializeField] private float upDuration = 0.5f;

    [Header("Ease Settings")]
    [SerializeField] private Ease downEase = Ease.OutQuad;
    [SerializeField] private Ease upEase = Ease.OutElastic;

    private Vector3 _initialScale;

    private void Awake()
    {
        _initialScale = transform.localScale;
    }

    // 1. 누를 때 (Down)
    public void OnPointerDown(PointerEventData eventData)
    {
        transform.DOKill();

        // 이벤트 호출 (사운드 매니저 등과 연동)
        OnDown?.Invoke();

        // 축소 애니메이션
        transform.DOScale(_initialScale * shrinkScale, downDuration)
                 .SetEase(downEase);
    }

    // 2. 뗄 때 (Up)
    public void OnPointerUp(PointerEventData eventData)
    {
        transform.DOKill();

        // 젤리 질감: 크게 튀었다가 원래대로 복귀
        transform.DOScale(_initialScale, upDuration)
                 .From(_initialScale * overshootScale)
                 .SetEase(upEase);
    }

    // 3. 완전한 클릭 완료 (Click)
    public void OnPointerClick(PointerEventData eventData)
    {
        // 실제 게임 로직 실행
        OnClick?.Invoke();
    }

    private void OnDisable()
    {
        // 오브젝트 비활성화 시 트윈 정리 및 크기 복구
        transform.DOKill();
        transform.localScale = _initialScale;
    }
}