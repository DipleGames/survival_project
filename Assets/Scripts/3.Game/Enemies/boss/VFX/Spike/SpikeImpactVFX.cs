using UnityEngine;
using DG.Tweening;

public class SpikeImpactVFX : MonoBehaviour
{
    [Header("가시 스프라이트")]
    [SerializeField]
    private SpriteRenderer spikeRenderer;

    [Header("가시 크기")]
    [SerializeField, Min(0.01f)]
    private float startScale = 0.1f;

    [SerializeField, Min(0.01f)]
    private float targetScale = 1f;

    [Header("연출 시간")]
    [SerializeField, Min(0.01f)]
    private float riseDuration = 0.15f;

    [SerializeField, Min(0f)]
    private float holdDuration = 0.2f;

    [SerializeField, Min(0.01f)]
    private float disappearDuration = 0.2f;

    private Sequence impactSequence;
    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;

        if (spikeRenderer != null)
            spikeRenderer.enabled = false;
    }

    public void Play(Vector3 position)
    {
        if (spikeRenderer == null)
            return;

        impactSequence?.Kill();

        transform.position = position;

        transform.localScale = originalScale * startScale;

        spikeRenderer.color = Color.white;
        spikeRenderer.enabled = true;

        impactSequence = DOTween.Sequence();

        // 가시가 솟아오르는 연출
        impactSequence.Append(
            transform.DOScale(
                originalScale * targetScale,
                riseDuration
            ).SetEase(Ease.OutBack)
        );

        // 가시 유지
        impactSequence.AppendInterval(holdDuration);

        // 가시 소멸
        impactSequence.Append(
            spikeRenderer.DOFade(
                0f,
                disappearDuration
            )
        );

        impactSequence.OnComplete(() =>
        {
            spikeRenderer.enabled = false;
            Destroy(gameObject);
        });
    }

    private void OnDisable()
    {
        impactSequence?.Kill();
    }
}
