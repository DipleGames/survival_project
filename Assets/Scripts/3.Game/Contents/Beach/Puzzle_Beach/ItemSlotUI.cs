using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour, IDropHandler
{
    public static event Action<Transform, Transform> OnItemDropped;

    public virtual void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        Transform draggedItem = eventData.pointerDrag.transform;
        DraggableSlotItem dragHandler = draggedItem.GetComponent<DraggableSlotItem>();

        Transform fromSlot = dragHandler.originalParent;
        Transform toSlot = transform;

        if (toSlot.childCount > 0)
        {
            Transform existingItem = toSlot.GetChild(0);
            existingItem.SetParent(fromSlot);
            existingItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }

        draggedItem.SetParent(toSlot);
        draggedItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        dragHandler.droppedOnSlot = true;

        OnItemDropped?.Invoke(fromSlot, toSlot);
    }
}
