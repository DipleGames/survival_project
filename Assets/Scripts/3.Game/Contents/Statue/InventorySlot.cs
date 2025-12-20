// InventorySlot.cs
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropTarget
{
    public ItemUI itemUI;
    Item curItem;
    public Item CurrentItem => curItem;
    public bool IsEmpty => itemUI == null;

    public void Init(Item item)
    {
        GameManager gm = GameManager.Instance;

        // 슬롯이 표시될 때마다 아이템 UI 표시/초기화
        if (itemUI != null)
        {
            curItem = item;

            itemUI.gameObject.SetActive(true);
            itemUI.Init(this, item, gm.haveItems[item.ItemId]);
        }
        else Debug.Log(name + " has No itemUI!");
    }

    public bool CanDrop(ItemUI dragging, PointerEventData _) => true;

    public void Drop(ItemUI dragging, PointerEventData _)
    {
        if (IsEmpty)
        {
            if (dragging.currentSlot != null)
                dragging.currentSlot.TakeItem(); // 원래 슬롯 비우고
            SetItem(dragging);                   // 이 슬롯에 배치
        }
        else
        {
            var origin = dragging.currentSlot;
            var myPrevItem = TakeItem();         // 내 아이템 잠시 빼두고
            if (origin != null) origin.TakeItem();
            SetItem(dragging);                   // 드래그 아이템 들어오고
            if (origin != null) origin.SetItem(myPrevItem); // 기존 아이템은 원래 슬롯으로
        }
    }

    public void SetItem(ItemUI newItem)
    {
        itemUI = newItem;

        curItem = newItem != null ? newItem.Item : null;

        if (itemUI)
        {
            itemUI.transform.SetParent(transform, false);
            itemUI.currentSlot = this;
            (itemUI.transform as RectTransform).anchoredPosition = Vector2.zero;
        }
    }

    public ItemUI TakeItem()
    {
        var t = itemUI;
        itemUI = null;

        curItem = null;

        if (t != null) t.currentSlot = null;
        return t;
    }

    public void EmptySlot()
    {
        if (itemUI != null)
        {
            Destroy(itemUI.gameObject);
            itemUI = null;
        }
        curItem = null;
    }
}
