using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoomType
{
    Start,
    Normal,
    Boss
}

public class DungeonRoomNode
{
    //현재 방의 위치 층수*100 + 인덱스 : 2층 3번방이면 Id는 203
    public int RoomId { get; private set; }

    //현재 방의 타입
    public RoomType RoomType { get; set; }

    //현재 방을 클리어 했는지 확인
    public bool IsClear { get; private set; }

    //해당 방과 이어진 방의 방향을 저장 (중복으로 값이 들어가는 것을 방지하기 위해 HashSet을 사용)
    public HashSet<int> Neighbors { get; private set; }

    //생성자
    public DungeonRoomNode(int _Id, RoomType _type = RoomType.Normal) 
    {
        RoomId = _Id;
        RoomType = _type;

        IsClear = false;
        Neighbors = new HashSet<int>();
    }

    //방을 클리어 했다면 호출
    public void RoomClear() { IsClear = true; }

    //방 잇기(던전 생성에만 사용)
    public void AddNeighbor(int _neighbor) { Neighbors.Add(_neighbor); }
}
