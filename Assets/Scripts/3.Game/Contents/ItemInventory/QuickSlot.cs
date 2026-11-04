
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class QuickSlot : MonoBehaviour,
    IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] Image itemImage;
    [SerializeField] ItemInventoryDrag dragSlot;

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
        if (eventData.pointerDrag == null) return;

        // 1. 장착 슬롯 -> 퀵슬롯
        EquipmentSlotUI fromEquipment =
            eventData.pointerDrag.GetComponent<EquipmentSlotUI>();

        if (fromEquipment != null)
        {
            EquipmentManager manager = fromEquipment.EquipmentManager;

            ItemInfo equippedItem = manager.GetEquippedItem();
            ItemInfo previousItem = itemInfo;

            // 퀵슬롯에 기존 아이템이 있다면 장착 가능 여부 검사 후 교환
            if (previousItem != null)
            {
                if (!manager.EquipItem(previousItem)) return;
            }
            else
            {
                // 빈 퀵슬롯으로 옮긴 경우 장착 해제
                manager.UnequipItem();
            }

            SetItem(equippedItem);
            return;
        }

        // 2. 퀵슬롯 -> 퀵슬롯
        QuickSlot fromQuick =
            eventData.pointerDrag.GetComponent<QuickSlot>();

        if (fromQuick != null)
        {
            if (fromQuick == this) return;

            ItemInfo previousItem = itemInfo;

            SetItem(fromQuick.ItemInfo);
            fromQuick.SetItem(previousItem);
            return;
        }

        // 3. 일반 인벤토리 -> 퀵슬롯
        ItemInvenSlot fromInventory = eventData.pointerDrag.GetComponent<ItemInvenSlot>();

        if (fromInventory != null)
        {
            SetItem(dragSlot.itemInfo);
        }
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
