using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfirmPrey : MonoBehaviour
{
    [Header("Slot")]
    [SerializeField] InventorySlot preySlot;

    public void Confirm()
    {
        Item submittedItem = preySlot.CurrentItem;
        if (submittedItem == null) return;

        if (submittedItem.CompareType(ItemType.Preyable, true))
        {
            switch(submittedItem.Preytype)
            {
                case PreyType.FISH:
                    Debug.Log("fish");
                    break;
                case PreyType.ORE:
                    Debug.Log("ore");
                    break;
                case PreyType.FRUIT:
                    Debug.Log("fruit");
                    break;
                case PreyType.FOOD:
                    Debug.Log("food");
                    break;
                default:
                    Debug.Log("Sorted preyable, but has no preytype");
                    break;
            }    
        }

        GameManager.Instance.haveItems[submittedItem.ItemId]--;
        Debug.Log($"Remaining {submittedItem.ItemName} : {GameManager.Instance.haveItems[submittedItem.ItemId]}");
    }
}
