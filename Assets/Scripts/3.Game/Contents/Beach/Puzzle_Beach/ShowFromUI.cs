using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ShowFromUI : MonoBehaviour
{
    [SerializeField] ItemType targetType;
    [SerializeField] GameObject puzzlePieceObject;

    Transform slotsParent;

    private void Start()
    {
        UpdateShowingInventoryUI();
    }
    public void UpdateShowingInventoryUI()
    {
        GameManager gameManager = GameManager.Instance;

        foreach (var item in gameManager.haveItems)
        {
            if (gameManager.Items[item.Key].CompareType(targetType, false))
            {
                
            }
        }
    }

}