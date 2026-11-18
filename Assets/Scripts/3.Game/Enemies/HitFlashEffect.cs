using UnityEngine;
using DG.Tweening;

public class HitFlashEffect : MonoBehaviour
{
    [Header("피격 이펙트 설정")]
    [SerializeField] private SpriteRenderer targetRenderer;
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float flashDuration = 0.2f;

    private Color originalColor;
    private Tween flashTween;

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<SpriteRenderer>();

        if (targetRenderer != null)
            originalColor = targetRenderer.color;
    }

    public void PlayHitFlash()
    {
        if (targetRenderer == null)
            return;

        flashTween?.Kill();
        targetRenderer.color = hitColor;
        flashTween = targetRenderer
            .DOColor(originalColor, flashDuration);
    }

    public void ResetFlash()
    {
        flashTween?.Kill();
        flashTween = null;

        if (targetRenderer != null)
            targetRenderer.color = originalColor;
    }

    private void OnDisable()
    {
        ResetFlash();
    }
}
