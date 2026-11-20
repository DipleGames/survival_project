using System;
using UnityEngine;

[Serializable]public class EquipmentItemInstance
{
    public string InstanceId;
    public int ItemId;
    public int CurrentDurability;

    public bool IsBroken => CurrentDurability <= 0;

    public EquipmentItemInstance(int itemId, int maxDurability)
    {
        InstanceId = Guid.NewGuid().ToString("N");
        ItemId = itemId;
        CurrentDurability = maxDurability;
    }

    // 현재 장비 인스턴스의 내구도 감소
    public void ConsumeDurability(int amount)
    {
        CurrentDurability = Mathf.Max(0, CurrentDurability - amount);
    }
}