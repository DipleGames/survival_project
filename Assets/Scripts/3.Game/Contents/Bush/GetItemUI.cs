using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GetItemUI : MonoBehaviour
{
    [SerializeField] Sprite bulletImage;
    [SerializeField] Image uiImage;
    [SerializeField] Text quantityText;
    [SerializeField] float fadeAwaySpeed;

    float imageAlpha = 1;
    int getQuantity = 0;

    private void OnEnable()
    {
        imageAlpha = 1;
        StartCoroutine(FadeAwayUI());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void SetBulletGetImage()
    {
        uiImage.sprite = bulletImage;
    }

    public void SetGetItemImage(Sprite getItemSprite, int _getQuantity)
    {
        uiImage.sprite = getItemSprite;
        getQuantity = _getQuantity;

        if (getQuantity > 0)
        {
            quantityText.text = $"x {getQuantity}";
        }

        quantityText.enabled = getQuantity > 0;
    }

    void AlphaChange()
    {
        Color uiColor = uiImage.color;        // 텍스트의 색상값
        uiColor.a = imageAlpha;              // 색상값의 알파값 변경(직접 변경 불가해서 빼옴)
        uiImage.color = uiColor;           // 변경한 색상을 대입
        quantityText.color = uiColor;
    }

    IEnumerator FadeAwayUI()
    {
        while (imageAlpha > 0)
        {
            AlphaChange();
            imageAlpha -= Time.deltaTime * fadeAwaySpeed;
            yield return null;
        }

        imageAlpha = 0;
        gameObject.SetActive(false);
    }
}
