using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum FadeType
{
    HORIZONTAL,
    VERTICAL,
    CLOCKWISE,
    COUNTER_CLOCKWISE,
}
public class ImageFader : MonoBehaviour
{
    [Header("Cache")]
    [SerializeField] Image filterImage;

    [Header("Option")]
    public FadeType fadeType;

    private void Awake()
    {
        if (!filterImage)
            filterImage = GetComponent<Image>();

        if (!filterImage)
        {
            Debug.LogError($"[ImageFader] Image not assigned on {name}");
            enabled = false;
            return;
        }

        filterImage.type = Image.Type.Filled;

        ApplyFadeType();
    }

    private void ApplyFadeType()
    {
        switch (fadeType)
        {
            case FadeType.HORIZONTAL:
                filterImage.fillMethod = Image.FillMethod.Horizontal;
                filterImage.fillOrigin = 1; // 0: 왼→오, 1: 오→왼
                break;

            case FadeType.VERTICAL:
                filterImage.fillMethod = Image.FillMethod.Vertical;
                filterImage.fillOrigin = 0; // 0: 아래→위, 1: 위→아래
                break;

            case FadeType.CLOCKWISE:
                filterImage.fillMethod = Image.FillMethod.Radial360;
                filterImage.fillOrigin = 2; // 시계 방향 시작 위치 (꽂아보고 맞추면 됨)
                filterImage.fillClockwise = true;
                break;

            case FadeType.COUNTER_CLOCKWISE:
                filterImage.fillMethod = Image.FillMethod.Radial360;
                filterImage.fillOrigin = 2;
                filterImage.fillClockwise = false;
                break;
        }
    }

    public void SetRatio(float ratio01)
    {
        filterImage.fillAmount = Mathf.Clamp01(ratio01);
    }
}
