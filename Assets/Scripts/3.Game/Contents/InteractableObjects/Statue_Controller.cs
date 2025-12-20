using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Statue_Controller : InteractableObject
{
    [SerializeField] GameObject statueUI;
    [SerializeField] List<Condition_Type> sentences;

    StatuePanel panel;  // 캐싱

    private void Awake()
    {
        if (statueUI == null)
        {
            Debug.LogError($"{name}: No statueUI assigned!");
            enabled = false;
            return;
        }

        if (!statueUI.TryGetComponent(out panel))
        {
            Debug.LogError($"{name}: statueUI has no StatuePanel!");
            enabled = false;
            return;
        }

        panel.InitInfo(sentences);
        statueUI.SetActive(false);
    }

    public override void InteractionLeftButtonFuc(GameObject hitObject)
    {
        if (isApproach)
            ShowUI();
    }

    private void ShowUI()
    {
        statueUI.SetActive(true);
    }
}


[Serializable]
public class Condition_Type
{
    public PreyType preyType;
    public List<string> condition;
}