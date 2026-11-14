using UnityEngine;
using DG.Tweening;

public class HoofSlamShockwaveVFX : MonoBehaviour
{
    [Header("충격파 스프라이트")]
    [SerializeField]
    private SpriteRenderer ringRenderer;

    [Header("확산 설정")]
    [SerializeField, Min(0f)]
    private float startRadius = 0.3f;

    [SerializeField, Min(0.01f)]
    private float expandDuration = 0.35f;

    [Header("색상")]
    [SerializeField]
    private Color shockwaveColor = new Color(1f, 0.3f, 0.2f, 0.85f);

    private Sequence shockwaveSequence;

    public void Play(float targetRadius)
    {
        if (ringRenderer == null || ringRenderer.sprite == null)
            return;

        shockwaveSequence?.Kill();

        // 스프라이트의 실제 월드 크기 계산
        Sprite sprite = ringRenderer.sprite;
        Vector2 spriteSize = sprite.rect.size / sprite.pixelsPerUnit;
        Transform ring = ringRenderer.transform;

        // 시작 지름
        float startDiameter = startRadius * 2f;

        // 최종 지름
        float targetDiameter = targetRadius * 2f;

        Vector3 startScale = new Vector3(startDiameter / spriteSize.x, startDiameter / spriteSize.y, 1f);

        Vector3 targetScale = new Vector3(targetDiameter / spriteSize.x, targetDiameter / spriteSize.y, 1f);

        ring.localScale = startScale;
        ringRenderer.color = shockwaveColor;
        ringRenderer.enabled = true;

        // 확대 + 페이드아웃 동시 실행
        shockwaveSequence = DOTween.Sequence();

        shockwaveSequence.Join(ring.DOScale(targetScale, expandDuration).SetEase(Ease.OutQuad));

        shockwaveSequence.Join(ringRenderer.DOFade(0f, expandDuration).SetEase(Ease.InQuad));
        
        shockwaveSequence.OnComplete(() =>
        {
            ringRenderer.enabled = false;
        });
    }

    public void Stop()
    {
        shockwaveSequence?.Kill();
        shockwaveSequence = null;

        if (ringRenderer != null)
            ringRenderer.enabled = false;
    }

    private void OnDisable()
    {
        Stop();
    }
}
