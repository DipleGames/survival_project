
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class QuickSlot : MonoBehaviour,
    IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] Image itemImage;
    [SerializeField] ItemInventoryDrag dragSlot;
    [SerializeField] QuickSlotManager quickSlotManager;
    ItemInfo itemInfo;

    // 다른 슬롯에서 현재 아이템을 조회하기 위한 프로퍼티
    public ItemInfo ItemInfo => itemInfo;

    // 퀵슬롯에 아이템 설정
    public void SetItem(ItemInfo item)
    {
        itemInfo = item;

        itemImage.sprite = item == null
            ? null
            : Resources.Load<Sprite>($"Item/{item.itemId}");
    }

    public void OnDrop(PointerEventData eventData)
    {
        // 코드 QuickSlotManager에서 처리
        quickSlotManager.HandleDrop(this, eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemInfo == null) return;

        dragSlot.itemInfo = itemInfo;
        dragSlot.GetComponentInChildren<Image>().sprite = itemImage.sprite;
        dragSlot.gameObject.SetActive(true);

        // 원본 아이템은 드롭 성공 전까지 유지
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (itemInfo == null) return;

        RectTransform dragRect = dragSlot.GetComponent<RectTransform>();

        RectTransform parentRect = dragRect.parent as RectTransform;

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(parentRect, eventData.position, eventData.pressEventCamera, out Vector3 worldPos))
        {
            dragRect.position = worldPos;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        dragSlot.gameObject.SetActive(false);
    }
}
