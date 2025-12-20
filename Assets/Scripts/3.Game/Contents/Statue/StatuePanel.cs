using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class StatuePanel : BasicPanel   
{ 
    [SerializeField] TextMeshProUGUI conditionText;

    [SerializeField] Dictionary<PreyType, List<string>> sentenceList;

    [SerializeField] InventorySlot preySlot;

    public void InitInfo(List<Condition_Type> wholeSentences) 
    {
        sentenceList = new();
        foreach(var c_t_pair in wholeSentences)
        {
            if(!sentenceList.TryGetValue(c_t_pair.preyType, out var strings))
            {
                strings = new();
                sentenceList[c_t_pair.preyType] = strings;
            }
            strings.AddRange(c_t_pair.condition);
        }
        RefreshConditionText();
    } 
    public void RefreshConditionText() // 낮으로 바뀔때 호출
    {
        conditionText.text = GetNewSentence(); 
    }
    private string GetNewSentence()
    {
        // 1) 유효 타입 목록을 매번 생성
        var validPreyTypes = BuildValidPreyTypesOnce();

        // 2) 타입 랜덤 선택
        var preytype = validPreyTypes[Random.Range(0, validPreyTypes.Count)];

        // 3) 해당 타입 문장 중 랜덤 선택
        var list = sentenceList[preytype];
        return list[Random.Range(0, list.Count)];
    }

    private List<PreyType> BuildValidPreyTypesOnce()
    {
        var result = new List<PreyType>(sentenceList.Count);
        foreach (var kv in sentenceList)
        {
            var t = kv.Key;
            var sentences = kv.Value;
            if (t == PreyType.NONE) continue;
            if (sentences == null || sentences.Count == 0) continue;
            result.Add(t);
        }
        return result;
    }
    public void ConfirmPrey()
    {
        Item submittedItem = preySlot.CurrentItem;
        if (submittedItem == null) return;

        if (submittedItem.CompareType(ItemType.Preyable, true))
        {
            BuffManager.Instance.Apply<StatueBuff>(
                new StatueBuffArgs
                {
                    Giver = "statue",
                    SubmittedItemId = submittedItem.ItemId,
                    Duration = 5f
                }
            );
        }
        else { Debug.Log("Not preyable item"); }

        GameManager.Instance.haveItems[submittedItem.ItemId]--;

        ClosePanel();
    }

    public override void ClosePanel()
    {
        preySlot.EmptySlot();
        base.ClosePanel();
    }
}