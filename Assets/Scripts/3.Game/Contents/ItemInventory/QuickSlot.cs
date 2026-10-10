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

    
    // 퀵슬롯에서 아이템을 드래그할 때  마우스 위치에 맞춰 드래그 아이콘을 이동하도록 수정
    
    public void OnDrag(PointerEventData eventData)
    {
        // 드래그할 아이템이 없으면 처리하지 않는다.
        if (dragSlot == null || dragSlot.itemInfo == null) return;

        // 드래그 UI의 RectTransform을 가져온다.
        RectTransform dragRect = dragSlot.GetComponent<RectTransform>();

        RectTransform parentRect = dragRect.parent as RectTransform;

        if (parentRect == null) return;

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(parentRect, eventData.position, eventData.pressEventCamera, out Vector3 worldPos))
        {
            // 변환된 월드 좌표로 드래그 아이콘을 이동시킨다.
            dragRect.position = worldPos;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragSlot.itemInfo == null) return;

        dragSlot.gameObject.SetActive(false);
    }
}
