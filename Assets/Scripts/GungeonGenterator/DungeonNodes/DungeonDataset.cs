using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


using Room = DungeonRoomNode;

public class DungeonDataset
{
    //던전에 필요한 데이터를 지니고 있음
    public SortedDictionary<int, Room> DungeonMap = new SortedDictionary<int, Room>();
    //층별로 방 Id를 저장하고 있음
    public Dictionary<int, List<int>> FloorMap = new Dictionary<int, List<int>>();

    // 전부 지워버림
    public void Clear()
    {
        DungeonMap.Clear();
        FloorMap.Clear();
    }
}
