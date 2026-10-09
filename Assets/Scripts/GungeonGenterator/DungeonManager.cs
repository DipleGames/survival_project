using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Room = DungeonRoomNode;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance { get; private set; }

    [SerializeField] private DungeonSettings settings;
    [SerializeField] private DungeonMapView mapView;

    //현재 플레이어가 있는 방 좌표
    public int CurrentRoomId { get; private set; }
    //보스방 좌표
    public int BossRoomId { get; private set; }
    //던전에 필요한 중요 데이터 모음
    public DungeonDataset DungeonData { get; private set; }

    //getter
    public int getDepth() { return settings.Depth; }
    public int getWidth() { return settings.Width; }
    public SortedDictionary<int, Room> getDungeonRoom() {  return DungeonData.DungeonMap; }
    public Room getDungeonRoom(int id) { return DungeonData.DungeonMap[id]; }
    public Dictionary<int, List<int>> getFloor() { return DungeonData.FloorMap; }
    public List<int> getFloor(int floor) { return DungeonData.FloorMap[floor]; }

    public void MoveTo(int roomId) 
    { 
        CurrentRoomId = roomId;
        mapView.SetVisible(false);
    }

    public void RoomClear()
    {
        getDungeonRoom(CurrentRoomId).RoomClear();
        mapView.UpdateInteractableRooms(CurrentRoomId);
        mapView.SetVisible(true);
    }

    public bool IsPlayingRoom(){ return getDungeonRoom(CurrentRoomId).IsClear == false; }

    //아래는 매니저 전용 함수들
    private void Awake()
    {
        if (Instance == null){ Instance = this; CreateMap(); }
        else Destroy(gameObject);
    }

    private void OnDestroy() { if (Instance == this) Clear(); }

    private void CreateMap()
    {
        DungeonGenerator generator = new DungeonGenerator(settings.Depth, settings.Width, settings.AntCount);
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

    private void Clear()
    {
        DungeonData.Clear();
        Instance = null;
    }


    //디버그용
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log($"현재 방: {CurrentRoomId}, 보스방: {BossRoomId}");
            Debug.Log($"현재 방 클리어 여부: {getDungeonRoom(CurrentRoomId).IsClear}");

        }
        if (Input.GetKeyDown(KeyCode.Z)){ RoomClear(); }
    }
}
