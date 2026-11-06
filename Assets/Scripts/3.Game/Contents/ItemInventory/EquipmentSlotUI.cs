
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipmentSlotUI : MonoBehaviour,
    IDropHandler, IPointerClickHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private EquipmentManager equipmentManager;
    [SerializeField] private Image itemIcon;
    [SerializeField] private ItemInventoryDrag dragSlot;

    // 퀵슬롯에서 장착 상태를 변경할 때 사용
    public EquipmentManager EquipmentManager => equipmentManager;

    private void OnEnable()
    {
        equipmentManager.OnEquipmentChanged += RefreshUI; //OnEquipmentChanged이벤트에 RefreshUI구독
        RefreshUI();
    }

    private void OnDisable()
    {
        equipmentManager.OnEquipmentChanged -= RefreshUI;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        // 퀵슬롯 -> 장착 슬롯
        QuickSlot fromQuick = eventData.pointerDrag.GetComponent<QuickSlot>();

        if (fromQuick != null)
        {
            ItemInfo previousItem = equipmentManager.GetEquippedItem();

            // 장착에 성공한 경우에만 기존 장비를 퀵슬롯으로 이동
            if (equipmentManager.EquipItem(fromQuick.ItemInfo))
            {
                fromQuick.SetItem(previousItem);
            }

            return;
        }

        // 일반 인벤토리 -> 장착 슬롯
        ItemInvenSlot fromInventory = eventData.pointerDrag.GetComponent<ItemInvenSlot>();

        if (fromInventory != null)
        {
            equipmentManager.EquipItem(dragSlot.itemInfo);
        }
    }

    // 장착된 아이템을 드래그 시작
    public void OnBeginDrag(PointerEventData eventData)
    {
        ItemInfo item = equipmentManager.GetEquippedItem();

        if (item == null) return;

        dragSlot.itemInfo = item;

        dragSlot.GetComponentInChildren<Image>().sprite = Resources.Load<Sprite>($"Item/{item.itemId}");

        dragSlot.gameObject.SetActive(true);
    }

    // 드래그 아이콘 이동
    public void OnDrag(PointerEventData eventData)
    {
        if (!equipmentManager.HasEquippedItem) return;

        RectTransform dragRect = dragSlot.GetComponent<RectTransform>();

        RectTransform parentRect = dragRect.parent as RectTransform;

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(parentRect, eventData.position, eventData.pressEventCamera, out Vector3 worldPos))
        {
            dragRect.position = worldPos;
        }
    }

    // 드래그 종료
    public void OnEndDrag(PointerEventData eventData)
    {
        dragSlot.gameObject.SetActive(false);
    }

    // 우클릭 장착 해제
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            equipmentManager.UnequipItem();
        }
    }

    // 장착 아이템 이미지 갱신
    public void RefreshUI()
    {
        ItemInfo item = equipmentManager.GetEquippedItem();

        itemIcon.enabled = item != null;

        if (item == null)
        {
            itemIcon.sprite = null;
            return;
        }

        itemIcon.sprite = Resources.Load<Sprite>($"Item/{item.itemId}");
    }
}
