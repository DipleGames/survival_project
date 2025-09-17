using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class QuickSlot : MonoBehaviour, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] Image itemImage;
    [SerializeField] ItemInventoryDrag dragSlot;

    Item item;

    public void OnDrop(PointerEventData eventData)
    {
        item = dragSlot.item;
        itemImage.sprite = Resources.Load<Sprite>($"Item/{item.ItemId}");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (item == null)
            return;

        dragSlot.item = item;
        dragSlot.GetComponentInChildren<Image>().sprite = itemImage.sprite;
        dragSlot.gameObject.SetActive(true);

        item = null;
        itemImage.sprite = null;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragSlot.item == null)
            return;

        dragSlot.GetComponent<ItemInventoryDrag>().RectTransform.anchoredPosition = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragSlot.item == null)
            return;

        dragSlot.gameObject.SetActive(false);
    }
}
