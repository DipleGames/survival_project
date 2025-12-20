using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuffDescription : MonoBehaviour
{
    [SerializeField] Text descriptionText;
    [SerializeField] RectTransform rect;

    Vector3 screenPoint;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void SetBuffTextInfo(OldBuff buff, int buff1, int buff2 = 0, int buff3 = 0)
    {
        string descText = "물고기를 구워먹고 생긴 버프.\n";

        switch(buff)
        {
            case OldBuff.MAXHEALTH:
                descText += $"최대 체력이 {buff1}% 증가한다..";
                break;

            case OldBuff.POWER:
                descText += $"공격력이 {buff1}%, 방어력이 {buff2}% 증가한다.";
                break;

            case OldBuff.SPEED:
                descText += $"이동 속도가 {buff1}%, 회피율이 {buff2}% 증가한다.";
                if(buff3 != 0)
                    descText += $"\n대쉬 스킬이 추가된다.";
                break;

            case OldBuff.RECOVERY_HEALTH:
                descText += $"회복 게이지의 회복량이 {buff1}% 증가한다.";
                break;
        }

        descriptionText.text = descText;
    }

    public void SetDeBuffTextInfo(Debuff debuff)
    {
        string descText = "모닥불을 때우지 않아 추위로 생긴 디버프.\n";

        switch (debuff)
        {
            case Debuff.ATTACK_SPEED:
                descText += "공격 속도가 10% 감소한다.";
                break;

            case Debuff.POWER:
                descText += "공격력이 20% 감소한다.";
                break;

            case Debuff.RECOVERY_HEALTH:
                descText += "회복 게이지의 회복량이 10% 감소한다.";
                break;

            case Debuff.SPEED:
                descText += "이동 속도가 20% 감소한다.";
                break;
        }

        descriptionText.text = descText;
    }
    
    public void ShowDescriptionPanel()
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(rect, Input.mousePosition, Camera.main, out screenPoint);
        transform.position = screenPoint;
    }
}
