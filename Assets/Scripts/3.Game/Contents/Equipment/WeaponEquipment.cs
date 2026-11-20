using UnityEngine;
//WeaponData를 참조하는 무기 장착 스크립트. 

public class WeaponEquipment : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private int inventoryItemId;

    [Header("Weapon Data")]
    [SerializeField] private WeaponData weaponData;

    public int InventoryItemId => inventoryItemId;
    public WeaponData Data => weaponData;
}
