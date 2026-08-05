using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDictionarySlot : MonoBehaviour
{
    int index;
    ItemDictionay itemDict;

    private void Start()
    {
        itemDict = transform.root.Find("ItemDictionary").GetComponent<ItemDictionay>();
        index = transform.GetSiblingIndex();
    }

    public void ChangeDiscription()
    {
        itemDict.ChangeItemInfoDescriptionText(index);
    }
}
