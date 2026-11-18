using UnityEngine;
using DG.Tweening;

public class HoofSlamWarningVFX : MonoBehaviour
{
    [Header("원형 경고 스프라이트")]
    [SerializeField] private SpriteRenderer circleRenderer;

    [Header("경고 색상")]
    [SerializeField] private Color startColor = new Color(1f, 0f, 0f, 0.15f);

    [SerializeField] private Color endColor = new Color(1f, 0f, 0f, 0.6f);

    private Tween warningTween;

    public void Show(float radius, float duration)
    {
        if (circleRenderer == null)
            return;

        warningTween?.Kill();

        // 원형 경고 크기 설정
        float diameter = radius * 2f;

        Sprite sprite = circleRenderer.sprite;

        if (sprite == null)
            return;

        Vector2 spriteSize = sprite.rect.size / sprite.pixelsPerUnit;

        circleRenderer.transform.localScale = new Vector3(diameter / spriteSize.x, diameter / spriteSize.y, 1f);

        // 경고 색상 초기화
        circleRenderer.color = startColor;
        circleRenderer.enabled = true;

        // 경고 시간이 흐를수록 붉게 변화
        warningTween = circleRenderer.DOColor(endColor, duration).SetEase(Ease.InQuad);
    }

    public void Hide()
    {
        warningTween?.Kill();
        warningTween = null;

        if (circleRenderer != null)
            circleRenderer.enabled = false;
    }

    private void OnDisable()
    {
        Hide();
    }
}
