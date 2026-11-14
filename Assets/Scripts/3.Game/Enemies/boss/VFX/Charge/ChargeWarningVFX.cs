using UnityEngine;
using DG.Tweening;

public class ChargeWarningVFX : MonoBehaviour
{
    [Header("경고 영역")]
    [SerializeField] private SpriteRenderer backgroundRenderer;
    [SerializeField] private SpriteRenderer fillRenderer;

    [Header("색상")]
    [SerializeField] private Color backgroundColor = new Color(1f, 0f, 0f, 0.2f);
    [SerializeField] private Color fillColor = new Color(1f, 0f, 0f, 0.75f);

    private Tween fillTween;
    private float currentDistance;
    private float currentWidth;

    private void Awake()
    {
        Hide();
    }

    public void Show(Vector3 startPosition, Vector3 direction, float distance, float width, float duration)
    {
        if (backgroundRenderer == null || fillRenderer == null)
            return;

        fillTween?.Kill();
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
            return;

        direction.Normalize();
        currentDistance = distance;
        currentWidth = width;

        // 돌진 경로의 중앙에 배치
        Vector3 center = startPosition + direction * (distance * 0.5f);
        center.y += 0.05f;
        transform.position = center;

        // 기존 XZ 평면 회전 방식 유지
        Vector3 side = new Vector3(-direction.z, 0f, direction.x);
        // 로컬 +X축을 돌진 방향에 맞춰 보스 위치에서부터 채운다.
        transform.rotation = Quaternion.LookRotation(Vector3.down, side);

        backgroundRenderer.color = backgroundColor;
        fillRenderer.color = fillColor;
        backgroundRenderer.enabled = true;
        fillRenderer.enabled = true;
        SetSpriteSize(backgroundRenderer, distance, width);
        SetFillProgress(0f);

        // DOTween으로 0 → 1 진행
        float progress = 0f;
        
        fillTween = DOTween.To(
            () => progress,
            value =>
            {
                progress = value;
                SetFillProgress(value);
            },
            1f,
            duration
        ).SetEase(Ease.Linear);
    }

    private void SetFillProgress(float progress)
    {
        float fillLength = currentDistance * progress;

        SetSpriteSize(fillRenderer, fillLength, currentWidth);

        // 왼쪽 시작점부터 오른쪽으로 확장
        fillRenderer.transform.localPosition = new Vector3(-currentDistance * 0.5f + fillLength * 0.5f, 0f, -0.01f);
    }

    private void SetSpriteSize(SpriteRenderer renderer, float width, float height)
    {
        if (renderer.sprite == null)
            return;

        Vector2 spriteSize = renderer.sprite.bounds.size;

        renderer.transform.localScale = new Vector3(width / spriteSize.x, height / spriteSize.y, 1f);
    }

    public void Hide()
    {
        fillTween?.Kill();
        fillTween = null;

        if (backgroundRenderer != null)
            backgroundRenderer.enabled = false;
            
        if (fillRenderer != null)
            fillRenderer.enabled = false;
    }

    private void OnDisable()
    {
        Hide();
    }
}
