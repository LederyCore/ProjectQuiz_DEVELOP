using UnityEngine;
using DG.Tweening;

public class UniversalNoticeReddotAnimator : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 40f;      // 위로 튀어오를 높이
    [SerializeField] private float jumpDuration = 0.15f;   // 튀어오르는 시간 (상승/하강 각각)
    [SerializeField] private float intervalTime = 3.0f;   // 애니메이션 사이 대기 시간

    [Header("Squash & Stretch Settings")]
    [SerializeField] private Vector3 stretchScale = new Vector3(0.8f, 1.5f, 0.8f); // 튀어오를 때 (홀쭉 길쭉)

    [Header("Ease Settings")]
    [SerializeField] private Ease jumpEase = Ease.OutQuad;
    [SerializeField] private Ease landEase = Ease.InQuad;

    private Vector3 _initialScale;
    private Vector3 _initialLocalPos;
    private Sequence _reddotSequence;

    private void Awake()
    {
        _initialScale = transform.localScale;
        _initialLocalPos = transform.localPosition;
    }

    private void OnEnable()
    {
        StartReddotAnimation();
    }

    private void StartReddotAnimation()
    {
        _reddotSequence?.Kill();

        // 1.0 기준의 상대적 스케일 계산 (초기 스케일 반영)
        Vector3 targetStretch = new Vector3(
            _initialScale.x * stretchScale.x,
            _initialScale.y * stretchScale.y,
            _initialScale.z * stretchScale.z
        );

        _reddotSequence = DOTween.Sequence()
            // --- 상승 및 길어짐 (Stretch) ---
            .Append(transform.DOLocalMoveY(_initialLocalPos.y + jumpHeight, jumpDuration).SetEase(jumpEase))
            .Join(transform.DOScale(targetStretch, jumpDuration).SetEase(jumpEase))

            // --- 하강 및 원복 ---
            .Append(transform.DOLocalMoveY(_initialLocalPos.y, jumpDuration).SetEase(landEase))
            .Join(transform.DOScale(_initialScale, jumpDuration).SetEase(landEase))

            // --- 대기 및 반복 ---
            .AppendInterval(intervalTime)
            .SetLoops(-1, LoopType.Restart)
            .SetUpdate(true);
    }

    private void OnDisable()
    {
        ResetState();
    }

    private void OnDestroy()
    {
        _reddotSequence?.Kill();
    }

    private void ResetState()
    {
        _reddotSequence?.Kill();
        transform.localScale = _initialScale;
        transform.localPosition = _initialLocalPos;
    }
}