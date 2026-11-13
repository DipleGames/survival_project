using UnityEngine;

[CreateAssetMenu(
    fileName = "DungeonSettings",
    menuName = "Dungeon/Dungeon Monster Settings"
)]
public class DungeonMonsterSetting : ScriptableObject
{
    [Min(0)] public int monsterCountMin;
    [Min(0)] public int monsterCountMax;

    [Min(0)] public int SpawnArcherWeight = 0;
    [Min(0)] public int SpawnShieldWeight = 0;
    [Min(0)] public int SpawnSmallBazicWeight = 0;
    [Min(0)] public int SpawnBigBazicWeight = 0;
}