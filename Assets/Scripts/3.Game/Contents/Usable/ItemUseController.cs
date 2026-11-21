using UnityEngine;

public class ItemUseController : MonoBehaviour
{
    [SerializeField] private ItemInventory itemInventory;
    [SerializeField] private ConsumableItemDefinition[] consumables;

    public bool TryUseConsumable(ItemInfo item)
    {
        if (item == null || item.itemType != (int)ItemType.Consumable || item.isConsumable != 1)
            return false;

        ItemManager manager = ItemManager.Instance;

        if (manager == null || manager.GetItemCount(item.itemId) <= 0)
            return false;

        ConsumableItemDefinition prefab = FindConsumable(item.itemId);

        if (prefab == null)
            return false;

        Player player = FindObjectOfType<Player>();

        if (player == null)
            return false;

        // 사용할 때만 프리팹 인스턴스 생성
        ConsumableItemDefinition instance = Instantiate(prefab);

        if (!instance.CanUse(player))
        {
            Destroy(instance.gameObject);
            return false;
        }

        // 효과 적용 전 보유 수량 차감
        if (!manager.TryRemoveItem(item.itemId, 1))
        {
            Destroy(instance.gameObject);
            return false;
        }

        // 효과 실행
        instance.Use(player);

        Destroy(instance.gameObject);

        // 일반 인벤토리 갱신
        itemInventory.RefreshInventory();

        // 마지막 아이템을 사용했다면 퀵슬롯 등록 해제
        if (manager.GetItemCount(item.itemId) == 0)
            QuickSlot.ClearItemFromAll(item.itemId);

        return true;
    }

    private ConsumableItemDefinition FindConsumable(int itemId)
    {
        foreach (ConsumableItemDefinition consumable in consumables)
        {
            if (consumable != null && consumable.ItemId == itemId)
                return consumable;
        }

        return null;
    }
}