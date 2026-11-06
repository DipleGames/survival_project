using UnityEngine;
using DG.Tweening;

public class SceneFade : MonoBehaviour
{
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 2f;

    private Tween fadeTween;

    private void Awake()
    {
        fadeCanvasGroup.alpha = 1f;
        fadeCanvasGroup.blocksRaycasts = true;
    }

    public void FadeIn()
    {
        fadeTween?.Kill();

        fadeCanvasGroup.alpha = 1f;
        fadeCanvasGroup.blocksRaycasts = true;

        fadeTween = fadeCanvasGroup.DOFade(0f, fadeDuration)
            .SetUpdate(true)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                fadeCanvasGroup.blocksRaycasts = false;
            });
    }

    public void ShowBlack()
    {
        fadeTween?.Kill();

        fadeCanvasGroup.alpha = 1f;
        fadeCanvasGroup.blocksRaycasts = true;
    }

    private void OnDestroy()
    {
        fadeTween?.Kill();
    }
}