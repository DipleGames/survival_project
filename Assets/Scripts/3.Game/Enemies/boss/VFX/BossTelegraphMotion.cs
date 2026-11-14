using UnityEngine;
using DG.Tweening;

public class BossTelegraphMotion : MonoBehaviour
{
    [Header("보스 외형")]
    [SerializeField] private Transform visual;

    [Header("움츠리기 설정")]
    [SerializeField, Range(0.1f, 1f)]
    private float squashScale = 0.8f;

    [Header("Recovery 모션 설정")]
    [SerializeField, Range(0.5f, 1f)]
    private float recoveryScale = 0.85f;

    [SerializeField, Min(0f)]
    private float recoveryEnterDuration = 0.15f;

    [SerializeField, Min(0f)]
    private float recoveryExitDuration = 0.2f;

    public float RecoveryExitDuration => recoveryExitDuration;

    private Tween recoveryTween;

    private Vector3 originalScale;
    private Tween motionTween;

    private void Awake()
    {
        if (visual != null)
            originalScale = visual.localScale;
    }

    public void PlayTelegraph(float duration)
    {
        if (visual == null)
            return;

        recoveryTween?.Kill();
        recoveryTween = null;

        motionTween?.Kill();
        visual.localScale = originalScale;

        Vector3 targetScale = originalScale * squashScale;

        motionTween = visual.DOScale(targetScale, duration).SetEase(Ease.OutQuad);
    }

    public void PlayRelease(float duration = 0.12f)
    {
        if (visual == null)
            return;

        motionTween?.Kill();

        motionTween = visual.DOScale(originalScale, duration).SetEase(Ease.OutBack);
    }

    public void PlayRecovery()
    {
        if (visual == null)
            return;

        // 기존 Telegraph 및 Release 모션 종료
        motionTween?.Kill();
        motionTween = null;

        // 이전 Recovery 모션 종료
        recoveryTween?.Kill();
        recoveryTween = null;

        // 항상 최초 Scale에서 시작
        visual.localScale = originalScale;

        Vector3 targetScale = originalScale * recoveryScale;

        recoveryTween = visual.DOScale(targetScale, recoveryEnterDuration).SetEase(Ease.OutQuad);
    }

    public void EndRecovery()
    {
        if (visual == null)
            return;

        recoveryTween?.Kill();

        recoveryTween = visual.DOScale(originalScale, recoveryExitDuration).SetEase(Ease.OutQuad);
    }

    public void ResetMotion()
    {
        motionTween?.Kill();
        motionTween = null;

        recoveryTween?.Kill();
        recoveryTween = null;

        if (visual != null)
            visual.localScale = originalScale;
    }

    private void OnDisable()
    {
        ResetMotion();
    }
}
