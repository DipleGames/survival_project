using UnityEngine;

[CreateAssetMenu(
    fileName = "DungeonSettings",
    menuName = "Dungeon/Dungeon Settings"
)]

public class DungeonSettings : ScriptableObject
{
    [Min(2)] public int Depth = 5;
    [Min(1)] public int Width = 4;
    [Min(1)] public int AntCount = 4;
}
