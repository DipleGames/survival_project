using System;

// 보유 아이템과 수량
[Serializable]
public class ItemStackSaveData
{
    public int itemId;
    public int count;
}

// 카테고리별 인벤토리 배치
[Serializable]
public class InventorySlotSaveData
{
    public int category;
    public int slotIndex;
    public int itemId = -1; // 빈 슬롯
}

// 퀵슬롯 배치
[Serializable]
public class QuickSlotSaveData
{
    public int slotIndex;
    public int itemId = -1; // 빈 슬롯
}

[Serializable]
public class MaterialSaveData
{
    public int materialId;
    public int count;
}