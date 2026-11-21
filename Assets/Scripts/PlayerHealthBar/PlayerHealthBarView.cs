
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBarView : MonoBehaviour
{
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TMP_Text hpText;

    /// <summary>
    /// 현재 HP와 최대 HP를 받아 체력바를 갱신한다.
    /// </summary>
    public void Render(float currentHp, float maxHp)
    {
        float ratio = maxHp > 0f ? Mathf.Clamp01(currentHp / maxHp) : 0f; // 최대 HP가 0보다 큰 경우에만 비율 계산, 그렇지 않으면 0으로 설정

        hpSlider.value = ratio;

        // 텍스트는 선택적으로 사용
        if (hpText != null)
        {
            hpText.text =
                $"{Mathf.CeilToInt(currentHp)} / {Mathf.CeilToInt(maxHp)}";
        }
    }
}
