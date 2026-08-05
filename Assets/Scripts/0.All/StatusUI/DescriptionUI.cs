using UnityEngine;
using UnityEngine.UI;

public enum StatText
{
    MaxHp,
    ReHp,
    Def,
    Avoid,
    Dmg,
    CAtk,
    LAtk,
    ASpd,
    Spd,
    Cri,
}

public class DescriptionUI : MonoBehaviour
{
    [SerializeField] Text descriptionText;
    [SerializeField] RectTransform rect;

    Vector3 screenPoint;

    public void SetTextInfo(StatText stat)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(rect, Input.mousePosition, Camera.main, out screenPoint);
        transform.position = screenPoint;

        switch(stat)
        {
            case StatText.MaxHp:
                descriptionText.text = "캐릭터의 최대 체력 수치.";
                break;

            case StatText.ReHp:
                descriptionText.text = "회복 게이지를 소모하여 회복되는 체력 수치.";
                break;

            case StatText.Def:
                descriptionText.text = "몬스터에게 공격받을 경우 입는 피해를 감소시켜 주는 수치.\n(최대 6까지 적용)";
                break;

            case StatText.Avoid:
                descriptionText.text = "캐릭터가 몬스터에게 공격받을 경우 피해를 무시하는 확률.";
                break;

            case StatText.Dmg:
                descriptionText.text = "모든 무기에 적용되는 공격력 수치.";
                break;

            case StatText.CAtk:
                descriptionText.text = "커틀러스에 적용되는 공격력 수치.";
                break;

            case StatText.LAtk:
                descriptionText.text = "석궁, 리볼버에 적용되는 공격력 수치.";
                break;

            case StatText.ASpd:
                descriptionText.text = "다음 공격 가능 시간과 관련된 수치.";
                break;

            case StatText.Spd:
                descriptionText.text = "캐릭터가 이동하는 속도.";
                break;

            case StatText.Cri:
                descriptionText.text = "2배 강력한 피해를 입히는 확률. \n석궁(캐치 성공), 커틀러스에만 적용.";
                break;
        }
    }
}
