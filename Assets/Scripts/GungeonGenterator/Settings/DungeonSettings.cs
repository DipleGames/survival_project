using UnityEngine;

[CreateAssetMenu(
    fileName = "DungeonSettings",
    menuName = "Dungeon/Dungeon Map Settings"
)]

public class DungeonSettings : ScriptableObject
{
    [Min(3)]public int Depth = 5;
    [Min(1)]public int Width = 4;
    [Min(1)]public int AntCount = 4;

    [Range(0,1)]public float StartEliteRoomChance = 0.0f;
    [Range(0,1)]public float EliteRoomChance = 0.05f;

    [Range(0,1)] public float StartTreasureRoomChance = 0.05f;
    [Range(0,1)]public float TreasureRoomChance = 0.05f;

    [Range(0,1)]public float StartShopRoomChance = 0.0f;
    [Range(0,1)]public float ShopRoomChance = 0.05f;
}
