using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine;

public class QuickSlotManager : MonoBehaviour
{
    [SerializeField] QuickSlot[] quickSlots;
    [SerializeField] ItemManager itemManager;
    private void Start()
    {
        itemManager = ItemManager.Instance;
        // ItemManager 인스턴스가 null이거나 활성화되지 않은 경우 처리
        if (itemManager == null || !itemManager.isActiveAndEnabled)
        {
            Debug.LogError("ItemManager를 찾을 수 없습니다.", this);
            enabled = false;
            return;
        }

        if (itemManager != null)
            itemManager.OnItemCountChanged += HandleItemCountChanged;
    }
    private void Update()
    {
        for (int i = 0; i < 6; i++)
        {
            if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i)))
                UseQuickSlotItem(i);
        }
    }

    // OnDestroy에서 이벤트 구독 해제
    private void OnDestroy()
    {
        if (itemManager != null)
            itemManager.OnItemCountChanged -= HandleItemCountChanged;
    }
    // 아이템 사용 아이템 매니저에서 처리
    private void HandleItemCountChanged(int itemId, int remaining)
    {
        Debug.Log($"아이템 {itemId}의 남은 수량: {remaining}");
        // 남은 수량이 0보다 크면 퀵슬롯에서 제거하지 않음
        if (remaining > 0)
            return;
        // 남은 수량이 0이면 퀵슬롯에서 제거
        foreach (QuickSlot slot in quickSlots)
        {
            if (slot != null && slot.ItemInfo?.itemId == itemId)
            {
                slot.SetItem(null);
            }
        }
    }

    public void UseQuickSlotItem(int index)
    {
        if (quickSlots == null || index < 0 || index >= quickSlots.Length)
            return;

        if (itemManager == null)
            return;

        if (quickSlots[index] == null || quickSlots[index].ItemInfo == null)
        {
            Debug.LogWarning($"퀵슬롯 {index}에 아이템이 없습니다.");
            return;
        }
        itemManager.UseItem(quickSlots[index].ItemInfo.itemId, 1);
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