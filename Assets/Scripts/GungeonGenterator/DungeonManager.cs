using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Room = DungeonRoomNode;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance { get; private set; }

    // 생성할 방 총 개수 : 기획자 용도
    [Header("Dungeon Settings")]
    [SerializeField] private int Depth = 5;     //층수
    [SerializeField] private int Width = 4;     //해당 층의 최대 방 생성 개수
    [SerializeField] private int AntCount = 4;  //ant path 알고리즘을 사용

    //현재 플레이어가 있는 방 좌표
    public int CurrentRoomId { get; private set; }
    //보스방 좌표
    public int BossRoomId { get; private set; }
    //던전에 필요한 중요 데이터 모음
    public DungeonDataset DungeonData { get; private set; }

    //getter
    public int getDepth() { return Depth; }
    public int getWidth() { return Width; }
    public SortedDictionary<int, Room> getDungeonRoom() {  return DungeonData.DungeonMap; }
    public Room getDungeonRoom(int id) { return DungeonData.DungeonMap[id]; }
    public Dictionary<int, List<int>> getFloor() { return DungeonData.FloorMap; }
    public List<int> getFloor(int floor) { return DungeonData.FloorMap[floor]; }

    public void MoveTo(int roomId) { CurrentRoomId = roomId; }

    //아래는 매니저 전용 함수들
    private void Awake()
    {
        if (Instance == null){Instance = this; CreateMap(); }
        else Destroy(gameObject);
    }

    private void OnDestroy() { if (Instance == this) Instance = null; }

    private void CreateMap()
    {
        DungeonGenerator generator = new DungeonGenerator(Depth, Width, AntCount);
        DungeonData = new DungeonDataset();
        DungeonData.DungeonMap = generator.DungeonMap;
        CurrentRoomId = generator.StartId;
        BossRoomId = generator.BossId;

        DungeonData.FloorMap.Clear();

        foreach (int roomId in DungeonData.DungeonMap.Keys)
        {
            int floor = roomId / 100;

            if (!DungeonData.FloorMap.ContainsKey(floor)) DungeonData.FloorMap[floor] = new List<int>();
            DungeonData.FloorMap[floor].Add(roomId);
        }
    }
}
