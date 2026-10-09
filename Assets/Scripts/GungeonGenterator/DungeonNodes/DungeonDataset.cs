using System.Collections;
using System.Collections.Generic;
using UnityEngine;



using Room = DungeonRoomNode;

public class DungeonDataset
{
    //던전에 필요한 데이터를 지니고 있음
    public SortedDictionary<int, Room> DungeonMap = new SortedDictionary<int, Room>();
    public Dictionary<int, List<int>> FloorMap = new Dictionary<int, List<int>>();

    // [중요 런타임 UI 데이터] - 방 번호랑 실제 버튼 매핑
    public Dictionary<int, DungeonRoomButton> RoomButtons = new Dictionary<int, DungeonRoomButton>();

    // 전부 지워버림
    public void Clear()
    {
        DungeonMap.Clear();
        FloorMap.Clear();
        RoomButtons.Clear();
    }
}
