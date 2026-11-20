
using System;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    [SerializeField] private ItemInventory itemInventory;
    private int equippedItemId = -1;

    public int EquippedItemId => equippedItemId;
    public bool HasEquippedItem => equippedItemId >= 0;

    private ItemManager itemManager;
    public event Action OnEquipmentChanged;

    //private readonly Dictionary<int, int> durabilityByItemId = new(); //내구도
    private EquipmentItemInstance equippedInstance;


    // 장착 가능한 아이템 타입을 정의, WeaponData를 참조해 무기 장착 여부를 확인
    [Header("Weapon Data")]
    [SerializeField] private WeaponEquipment[] weapons;

    private WeaponEquipment equippedWeapon; // WeaponData를 참조하는 무기 장착 스크립트, 현재 장착된 무기 정보를 저장
    public WeaponData EquippedWeaponData => equippedWeapon == null ? null : equippedWeapon.Data;



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
    // 아이템 장착 및 교체
    public bool EquipItem(ItemInfo item)
    {
        // 1. 장착 가능한 아이템인지 확인
        if (item == null || !CanEquipItem(item) || itemManager == null)
            return false;

        // 2. 보유 여부 확인
        if (itemManager.GetItemCount(item.itemId) <= 0)
            return false;

        // 3. 장착할 무기 데이터 검색
        WeaponEquipment nextWeapon = FindWeapon(item.itemId);

        // 4. 장착할 장비 인스턴스 검색
        EquipmentItemInstance nextInstance = null;

        if (nextWeapon != null)
        {
            nextInstance = itemManager.GetAvailableEquipmentInstance(item.itemId);

            if (nextInstance == null)
            {
                Debug.Log("장착 실패: 사용 가능한 장비 인스턴스 없음");
                return false;
            }
        }

        // 5. 모든 검사 완료 후 장착 상태 변경
        equippedItemId = item.itemId;
        equippedWeapon = nextWeapon;
        equippedInstance = nextInstance;

        Debug.Log($"장착 성공: {equippedItemId}");

        if (equippedInstance != null)
        {
            Debug.Log(
                $"장비 인스턴스: {equippedInstance.InstanceId}, " +
                $"현재 내구도: {equippedInstance.CurrentDurability}"
            );
        }

        // 6. 플레이어 스탯 갱신
        RefreshPlayerStats();

        // 7. 장착 슬롯 UI 갱신
        OnEquipmentChanged?.Invoke();

        return true;
    }

    // 장착 해제
    public void UnequipItem()
    {
        equippedItemId = -1;
        equippedWeapon = null;
        equippedInstance = null;

        // 장비 보너스 제거 및 기본 스탯 복원
        RefreshPlayerStats();
        OnEquipmentChanged?.Invoke();
    }

    // 장착된 아이템 정보 반환
    public ItemInfo GetEquippedItem()
    {
        if (!HasEquippedItem) return null;

        return itemManager.itemInfos[equippedItemId];
    }

    public void LoadEquipment(int itemId)
    {
        itemManager = ItemManager.Instance;
        if (itemManager != null)
        {
            foreach (WeaponEquipment weapon in weapons)
                itemManager.RestoreEquipmentInstances(weapon.InventoryItemId, weapon.Data.MaxDurability);
        }

        if (!EquipItem(itemManager?.GetItemInfo(itemId)))
            UnequipItem();
    }

    // 장착된 아이템이 제거되면 해제
    public void OnItemRemoved(int itemId)
    {
        if (equippedItemId == itemId)
            UnequipItem();
    }

    private WeaponEquipment FindWeapon(int itemId)
    {
        foreach (WeaponEquipment weapon in weapons)
        {
            if (weapon.InventoryItemId == itemId)
                return weapon;
        }

        return null;
    }

    //장착된 무기의 BaseDamage를 플레이어의 AttackPower 보너스로 적용
    public IEnumerable<Stat> GetEquipmentBonuses()
    {
        if (EquippedWeaponData == null)
            return Array.Empty<Stat>();

        return new[]
        {
            new Stat
            {
                  type = StatType.AttackPower, value = EquippedWeaponData.BaseDamage
            }
        };
    }

    //Player의 스탯을 장착된 무기의 보너스를 합산하여 갱신
    private void RefreshPlayerStats()
    {
        Player player = FindObjectOfType<Player>();

        if (player == null)
            return;

        player.ApplyEquipmentStats(GetEquipmentBonuses());

        Debug.Log($"[EquipmentManager] 최종 공격력: " + $"{player.Status.GetStat(StatType.AttackPower)}");
    }


    public int GetCurrentDurability()
    {
        if (equippedInstance == null)
            return 0;

        return equippedInstance.CurrentDurability; //생성된 인스턴스의 내구도를 반환
    }

    public float GetDurabilityNormalized()
    {
        if (equippedInstance  == null || EquippedWeaponData == null || EquippedWeaponData.MaxDurability <= 0)
            return 0f;

        return (float)equippedInstance.CurrentDurability / EquippedWeaponData.MaxDurability;
    }

    public void ConsumeDurability(int amount)
    {
        if (equippedInstance == null || amount <= 0)
            return;

        // 1. 현재 장착한 장비 인스턴스의 내구도 감소
        equippedInstance.ConsumeDurability(amount);

        // 2. 내구도 0 도달 시 장비 파괴
        if (equippedInstance.IsBroken)
        {
            int brokenItemId = equippedInstance.ItemId;
            string brokenInstanceId = equippedInstance.InstanceId;

            // 개별 인스턴스 제거 및 보유 수량 1 감소
            if (!itemManager.RemoveEquipmentInstance(brokenInstanceId))
                return;

            Debug.Log($"아이템 파괴: {brokenItemId}, {brokenInstanceId}");

            // 장착 해제 및 플레이어 스탯 복원
            UnequipItem();

            // 일반 인벤토리 UI 갱신
            itemInventory.RefreshInventory();

            // 마지막 아이템이 파괴되었다면 퀵슬롯에서도 제거
            if (itemManager.GetItemCount(brokenItemId) == 0)
            {
                QuickSlot.ClearItemFromAll(brokenItemId);
            }

            return;
        }

        // 3. 남은 내구도를 장착 UI에 반영
        OnEquipmentChanged?.Invoke();
    }

#if UNITY_EDITOR
    [ContextMenu("Test/내구도 50 감소")]
    private void TestConsumeDurability()
    {
        if (equippedInstance == null)
            return;

        EquipmentItemInstance testInstance = equippedInstance;
        int maxDurability = EquippedWeaponData.MaxDurability;

        ConsumeDurability(50);

        Debug.Log(
            $"인스턴스 ID: {testInstance.InstanceId}, " + $"남은 내구도: {testInstance.CurrentDurability} / " +  $"{maxDurability}"
        );
    }
#endif
}
