using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct RoomTypePositionData
{
    public RoomType RoomType;
    public Vector3 PlayerPosition;
    public Vector3 PortalPosition;
    public Vector3 MonsterSpawnPosition;
}

[CreateAssetMenu(
    fileName = "DungeonRoomTypePositions",
    menuName = "Dungeon/Dungeon Room Type Positions"
)]
public class DungeonRoomPositionSettings : ScriptableObject
{
    public List<RoomTypePositionData> Rooms;
    public RoomTypePositionData GetRoomPosition(RoomType roomType)
    {
        return Rooms.Find(room => room.RoomType == roomType);
    }
}