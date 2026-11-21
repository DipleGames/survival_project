using UnityEngine;

public class QuickSlotController : MonoBehaviour
{
    [SerializeField] private QuickSlotManager quickSlotManager;
    [SerializeField] private ItemUseController itemUseController;
    [SerializeField] private EquipmentManager equipmentManager;

    // 퀵슬롯 단축키 설정, 1~6번 키에 해당하는 퀵슬롯 사용
    private static readonly KeyCode[] slotKeys =
    {
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
        KeyCode.Alpha5,
        KeyCode.Alpha6
    };

    private void Update()
    {
        for (int i = 0; i < slotKeys.Length; i++)
        {
            if (Input.GetKeyDown(slotKeys[i]))
            {
                TryUseSlot(quickSlotManager.GetSlot(i));
                return;
            }
        }
    }

    public void TryUseSlot(QuickSlot slot)
    {
        if (slot == null || slot.ItemInfo == null)
            return;

        ItemInfo item = slot.ItemInfo;
        ItemType type = (ItemType)item.itemType;

        // 장비는 기존 EquipmentManager 사용
        if (type == ItemType.Weapon || type == ItemType.Tool)
        {
            ItemInfo previousItem = equipmentManager.GetEquippedItem();

            if (equipmentManager.EquipItem(item))
            {
                // 장착 성공 시 기존 장비와 교환
                slot.SetItem(previousItem);
            }

            return;
        }

        // 소비 아이템은 별도 처리
        if (type == ItemType.Consumable)
        {
            itemUseController.TryUseConsumable(item);
        }
    }
}