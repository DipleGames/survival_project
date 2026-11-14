using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(SpriteRenderer))]
public class SpikeWarningVFX : MonoBehaviour
{
    [Header("경고 색상")]
    [SerializeField]
    private Color startColor = new Color(1f, 0f, 0f, 0.15f);

    [SerializeField]
    private Color endColor = new Color(1f, 0f, 0f, 0.75f);

    private SpriteRenderer spriteRenderer;
    private Tween warningTween;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false;
    }

    public void Show(Vector3 position, float radius, float duration)
    {
        warningTween?.Kill();

        // 기록된 월드 위치로 이동
        transform.position = position;

        // 원형 스프라이트의 실제 크기를 기준으로
        // 목표 지름에 맞게 Scale 계산
        if (spriteRenderer.sprite != null)
        {
            Vector2 spriteSize = spriteRenderer.sprite.rect.size / spriteRenderer.sprite.pixelsPerUnit;

            float diameter = radius * 2f;

            transform.localScale = new Vector3(diameter / spriteSize.x, diameter / spriteSize.y, 1f);
        }

        spriteRenderer.color = startColor;
        spriteRenderer.enabled = true;

        warningTween = spriteRenderer.DOColor(endColor, duration).SetEase(Ease.InQuad);
    }

    public void Hide()
    {
        warningTween?.Kill();
        warningTween = null;

        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
    }

    private void OnDisable()
    {
        Hide();
    }
}
