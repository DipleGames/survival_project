using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class QuickSlot : MonoBehaviour, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] Image itemImage;
    [SerializeField] ItemInventoryDrag dragSlot;

    ItemInfo itemInfo;

    public void OnDrop(PointerEventData eventData)
    {
        itemInfo = dragSlot.itemInfo;
        itemImage.sprite = Resources.Load<Sprite>($"Item/{itemInfo.itemId}");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemInfo == null)
            return;

        dragSlot.itemInfo = itemInfo;
        dragSlot.GetComponentInChildren<Image>().sprite = itemImage.sprite;
        dragSlot.gameObject.SetActive(true);

        itemInfo = null;
        itemImage.sprite = null;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragSlot.itemInfo == null)
            return;

        dragSlot.GetComponent<ItemInventoryDrag>().RectTransform.anchoredPosition = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragSlot.itemInfo == null)
            return;

        dragSlot.gameObject.SetActive(false);
    }
}
