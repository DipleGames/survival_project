using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine;

public class QuickSlotManager : MonoBehaviour
{
    [SerializeField] QuickSlot[] quickSlots;

    private ItemManager itemManager;
    private void Start()
    {
        itemManager = ItemManager.Instance;

        if (itemManager != null)
            itemManager.OnItemCountChanged += HandleItemCountChanged;

        // 처음 등록되어 있는 아이템 수량 표시
        foreach (QuickSlot slot in quickSlots)
            slot.RefreshQuantity();
    }

    // 아이템 수량 변경 이벤트 처리
    private void HandleItemCountChanged(int itemId, int count)
    {
        foreach (QuickSlot slot in quickSlots)
        {
            if (slot.ItemInfo?.itemId == itemId)
                slot.RefreshQuantity();
        }
    }

    private void OnDestroy()
    {
        if (itemManager != null)
            itemManager.OnItemCountChanged -= HandleItemCountChanged;
    }

    public List<QuickSlotSaveData> CallQuickSlotData()
    {
        var data = new List<QuickSlotSaveData>();

        for (int i = 0; i < quickSlots.Length; i++)
        {
            ItemInfo item = quickSlots[i].ItemInfo;

            data.Add(new QuickSlotSaveData
            {
                slotIndex = i,
                itemId = item != null ? item.itemId : -1
            });
        }

        return data;
    }
    // 퀵슬롯 인덱스에 해당하는 슬롯 반환, 퀵슬롯 컨트롤러에서 사용.
    public QuickSlot GetSlot(int index)
    {
        if (index < 0 || index >= quickSlots.Length)
            return null;

        return quickSlots[index];
    }
    public void LoadQuickSlotData(List<QuickSlotSaveData> data)
    {
        // 이전 등록 정보 제거
        foreach (QuickSlot slot in quickSlots)
        {
            slot.SetItem(null);
        }
        // 저장데이터 없으면 리턴
        if (data == null) return;

        foreach (QuickSlotSaveData savedSlot in data)
        {
            int index = savedSlot.slotIndex;

            if (index < 0 || index >= quickSlots.Length)
                continue;

            if (savedSlot.itemId == -1)
                continue;

            ItemInfo item = ItemManager.Instance.GetItemInfo(savedSlot.itemId);

            quickSlots[index].SetItem(item);
        }


    }
    public void HandleDrop(QuickSlot targetSlot, PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        // 장착 슬롯 → 퀵슬롯
        EquipmentSlotUI fromEquipment =
            eventData.pointerDrag.GetComponent<EquipmentSlotUI>();

        if (fromEquipment != null)
        {
            EquipmentManager manager = fromEquipment.EquipmentManager;
            if (manager == null) return;

            ItemInfo equippedItem = manager.GetEquippedItem();
            if (equippedItem == null) return;

            ItemInfo previousItem = targetSlot.ItemInfo;

            if (previousItem != null)
            {
                // 기존 규칙 유지: 장착 가능한 아이템만 교환
                if (!manager.EquipItem(previousItem)) return;
            }
            else
            {
                manager.UnequipItem();
            }

            targetSlot.SetItem(equippedItem);
            return;
        }

        // 퀵슬롯 → 퀵슬롯
        QuickSlot fromQuick =
            eventData.pointerDrag.GetComponent<QuickSlot>();

        if (fromQuick != null)
        {
            if (fromQuick == targetSlot || fromQuick.ItemInfo == null)
                return;

            ItemInfo previousItem = targetSlot.ItemInfo;

            targetSlot.SetItem(fromQuick.ItemInfo);
            fromQuick.SetItem(previousItem);
            return;
        }

        // 일반 인벤토리 → 퀵슬롯
        ItemInvenSlot fromInventory =
            eventData.pointerDrag.GetComponent<ItemInvenSlot>();

        if (fromInventory != null && fromInventory.ItemInfo != null)
        {
            targetSlot.SetItem(fromInventory.ItemInfo);
        }
    }
}