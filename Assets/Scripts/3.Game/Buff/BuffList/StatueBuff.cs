using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatueBuffArgs
{
    public string Giver;
    public int SubmittedItemId;
    public float Duration;
}
public class StatueBuff : Buff<StatueBuffArgs>
{
    public string Giver { get; private set; }
    public int SubmittedItemId { get; private set; }

    public override string DisplayName => "석상버프";
    public override string IconPath => "UI/Buffs/statue";


    protected override void Initialize(StatueBuffArgs args)
    {
        Giver = args.Giver;
        SubmittedItemId = args.SubmittedItemId;
        Duration = args.Duration;
        SetDescription();
    }


    public override void OnApply()
    {
        switch(SubmittedItemId)
        {
            //음식
            case 1010000:
                Debug.Log("몬스터 데미지 2% 증가");
                break;
            case 1010001:
                Debug.Log("몬스터 데미지 5% 증가");
                break;
            case 1010002:
                Debug.Log("몬스터 데미지 10% 증가");
                break;
            //열매
            case 2030004:
                Debug.Log("열매 채집량 확률적으로 1 증가");
                break;
            case 2030005:
                Debug.Log("열매 채집량 1 증가");
                break;
            case 2030006:
                Debug.Log("열매 채집량 2 증가");
                break;
            //물고기
            case 2030007:
                GameManager.Instance.substatus[SubStatus.Fishing_AdditionalChance] += 50f;
                GameManager.Instance.substatus[SubStatus.Fishing_AdditionalAmount_ByChance] += 1;
                break;
            case 2030008:
                GameManager.Instance.substatus[SubStatus.Fishing_AdditionalAmount] += 1;
                break;
            case 2030009:
                GameManager.Instance.substatus[SubStatus.Fishing_AdditionalAmount] += 2;
                break;

            default:
                Debug.Log("Preyable but no Logic");
                break;
        }
    }

    public override void OnRemove()
    {
        switch (SubmittedItemId)
        {
            //음식
            case 1010000:
                break;
            case 1010001:
                break;
            case 1010002:
                break;
            //열매
            case 2030004:
                break;
            case 2030005:
                break;
            case 2030006:
                break;
            //물고기
            case 2030007:
                GameManager.Instance.substatus[SubStatus.Fishing_AdditionalChance] -= 50f;
                GameManager.Instance.substatus[SubStatus.Fishing_AdditionalAmount_ByChance] -= 1;
                break;
            case 2030008:
                GameManager.Instance.substatus[SubStatus.Fishing_AdditionalAmount] -= 1;
                break;
            case 2030009:
                GameManager.Instance.substatus[SubStatus.Fishing_AdditionalAmount] -= 2;
                break;

            default:
                break;
        }
    }
    private void SetDescription()
    {
        description = "";
        switch(SubmittedItemId)
        {
            //음식
            case 1010000:
                description = "몬스터에 대한 데미지 2% 증가";
                break;
            case 1010001:
                description = "몬스터에 대한 데미지 5% 증가";
                break;
            case 1010002:
                description = "몬스터에 대한 데미지 10% 증가";
                break;
            //열매
            case 2030004:
                description = "채집 성공시 50% 확률로 추가 열매 획득 +1";
                break;
            case 2030005:
                description = "채집 성공시 추가 열매 획득 +1";
                break;
            case 2030006:
                description = "채집 성공시 추가 열매 획득 +2";
                break;
            //물고기
            case 2030007:
                description = "낚시 성공시 50% 확률로 물고기 추가 획득 +1";
                break;
            case 2030008:
                description = "낚시 성공시 물고기 추가 획득 +1";
                break;
            case 2030009:
                description = "낚시 성공시 물고기 추가 획득 +2";
                break;

            default:
                break;
        }
    }
}

