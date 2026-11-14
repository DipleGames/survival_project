using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MonsterSpawnData
{
    public GameObject MonsterPrefab;
    [Min(0)] public int SpawnWeight = 1;
}

[System.Serializable]
public class RoomTypeSettings
{
    public RoomType RoomType;

    [Header("Spawn Positions")]
    public Vector3 PlayerSpawnPosition = Vector3.zero;
    public Vector3 PortalSpawnPosition = Vector3.zero;
    public Vector3 MonsterSpawnerPosition = Vector3.zero;

    [Header("Generator Room Chance")]
    public bool UseRoomChance = false;
    [Range(0f, 1f)] public float StartChance = 0f;
    [Range(0f, 1f)] public float IncreaseChance = 0f;


    [Header("Monster Settings")]
    public bool UseMonsterSpawn = false;
    [Min(0)] public int MonsterCountMin;
    [Min(0)] public int MonsterCountMax;
    public List<MonsterSpawnData> MonsterSpawnList = new();
}

[CreateAssetMenu(
     fileName = "DungeonSettings",
    menuName = "Dungeon/Dungeon Map Settings"
)]
public class DungeonSettings : ScriptableObject
{
    [Min(3)]public int Depth = 5;
    [Min(1)]public int Width = 4;
    [Min(1)]public int AntCount = 4;

    public List<RoomTypeSettings> Rooms = new();
    public RoomTypeSettings GetRoomSettings(RoomType roomType)
    {
        return Rooms.Find(room => room.RoomType == roomType);
    }
}
