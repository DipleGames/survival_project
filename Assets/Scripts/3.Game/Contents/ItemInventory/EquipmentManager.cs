
using System;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    private int equippedItemId = -1;

    public int EquippedItemId => equippedItemId;
    public bool HasEquippedItem => equippedItemId >= 0;

    private ItemManager itemManager;
    public event Action OnEquipmentChanged;

    private void Start()
    {
        itemManager = ItemManager.Instance;
    }

    // Tool, Weapon 장착 가능 여부 확인
    public bool CanEquipItem(ItemInfo item)
    {
        ItemType type = (ItemType)item.itemType;

        return type == ItemType.Tool || type == ItemType.Weapon;
    }

    // 아이템 장착 및 교체
    public bool EquipItem(ItemInfo item)
    {
        Debug.Log("[EquipmentManager] EquipItem 호출");

        if (item == null || !CanEquipItem(item) || itemManager == null)
        {
            Debug.Log("장착 실패");
            return false;
        }

        Debug.Log($"아이템 ID: {item.itemId}, Type: {item.itemType}");

        if (!itemManager.haveItems.TryGetValue(item.itemId, out int count) || count <= 0)
        {
            Debug.Log("장착 실패: 보유하지 않은 아이템");
            return false;
        }


        equippedItemId = item.itemId;
        Debug.Log($"장착 성공: {equippedItemId}");
        OnEquipmentChanged?.Invoke();

        return true;
    }

    // 장착 해제
    public void UnequipItem()
    {
        equippedItemId = -1;
        OnEquipmentChanged?.Invoke();
    }

    // 장착된 아이템 정보 반환
    public ItemInfo GetEquippedItem()
    {
        if (!HasEquippedItem) return null;

        return itemManager.itemInfos[equippedItemId];
    }

    // 장착된 아이템이 제거되면 해제
    public void OnItemRemoved(int itemId)
    {
        if (equippedItemId == itemId)
            UnequipItem();
    }

    public void LoadEquipment(int itemId)
    {
        if(itemId == -1)
        {
            UnequipItem();
            return;
        }
        equippedItemId = itemId;
        EquipItem(itemManager.itemInfos[itemId]);
    }
}
