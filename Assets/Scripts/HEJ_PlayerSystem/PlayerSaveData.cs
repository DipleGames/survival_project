using UnityEngine;
using System.Collections.Generic;
using System;


// 다른 부위들도 추가할거면 이런 형식으로 사용
/*public enum EquipmentSlot
{
    Weapon = 0,
    Head = 1,
    Body = 2,
    Legs = 3,
    Count = 4
}*/
// 플레이어 정보 => 연관된 데이터끼리 붙여서 관리하고 3개이상 늘어나면 새롭게 class를 만들어서 관리
[Serializable]
public class PlayerSaveData
{
    // 플레이어의 위치값
    public Vector3 position;
    public float currentHp;

    // 현재 장착한 아이템의 ID
    public int equippedItemId = -1;
    // 다른부위도 장착하게 된다면 아래 코드 사용
    //  public int[] equippedItemIds = { -1, -1, -1, -1 };
    // 기존 코드. 에러가 일어나면 확인하고 없다면 삭제
    //public List<string> equippedItemIDs = new();


    // 인벤토리 정보 연결 => 정보가 늘어나면 아래처럼 class를 만들고 이렇게 연결해서 관리
    // 외부에서 접근은 
    public InventorySaveData inventory = new();
}

// 인벤토리 관련 정보
[System.Serializable]
public class InventorySaveData
{
    // 보유 아이템의 ID와 수량
    public List<ItemStackSaveData> items = new();
    // 인벤토리의 아이템 배치
    public List<InventorySlotSaveData> inventorySlots = new();
    // 퀵슬롯의 아이템 배치
    public List<QuickSlotSaveData> quickSlots = new();
    // 별도로 관리하는 재료 종류와 수량
    public List<MaterialSaveData> materials = new();

}