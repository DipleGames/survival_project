
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 인벤토리 카테고리 탭의 시각적 상태를 관리하는 스크립트. 선택된 카테고리에 따라 배경, 오버레이, 텍스트 색상을 변경

public class InventoryCategoryTabUI : MonoBehaviour
{
    [Header("카테고리 UI 참조")]
    [SerializeField] private Image[] tabImages; //각 카테고리의 기본 배경 Image
    [SerializeField] private TextMeshProUGUI[] tabTexts; // 각 카테고리의 TextMeshProUGUI
    //[SerializeField] private Image[] tabSelectedOverlays; // 각 카테고리 하위의 Selected Image
    [Header("카테고리 색상")]

    [SerializeField]private Color tabNormalColor = new Color32(0xA0, 0x8F, 0x6D, 0xFF);  // 선택되지 않은 탭 배경 색상
    [SerializeField]private Color tabTextNormalColor = new Color32(0xD8, 0xCF, 0xBC, 0xFF); // 선택되지 않은 탭 텍스트 색상
    [SerializeField]private Color tabSelectedColor = Color.white; // 선택된 탭의 오버레이 색상
    [SerializeField]private Color tabTextSelectedColor = Color.white; // 선택된 탭의 텍스트 색상

    // 선택된 카테고리 번호에 따라 UI를 갱신
    public void SetSelectedCategory(int type)
    {
        if (tabImages == null)
            return;

        for (int i = 0; i < tabImages.Length; i++)
        {
            bool selected = (i == type);

            // 선택된 탭은 원래 배경 색상을 유지
            // 선택되지 않은 탭은 어두운 색상 적용
            if (tabImages[i] != null)
            {
                tabImages[i].color = selected ? tabSelectedColor : tabNormalColor;
            }

            if (tabTexts != null &&
                i < tabTexts.Length &&
                tabTexts[i] != null)
            {
                tabTexts[i].color = selected
                    ? tabTextSelectedColor
                    : tabTextNormalColor;
            }
        }
    }
}
