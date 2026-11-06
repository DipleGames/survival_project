using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Room = DungeonRoomNode;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance { get; private set; }

    [Header("Dungeon Settings")]
    [SerializeField] private DungeonSettings settings;
    [SerializeField] private DungeonMapView mapView;

    [Header("Dungeon Room Settings")]
    [SerializeField] private GameObject Player;
    [SerializeField] private GameObject Portal;

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
        mapView.SetVisible(false);
        CurrentRoomId = roomId;

        RoomType roomType = getDungeonRoom(CurrentRoomId).RoomType;
        float XPos = RoomTypeNumber.GetRoomXPosition(roomType);

        Portal.transform.position = new Vector3(XPos, 10.5f, Portal.transform.position.z);
        //포탈 타입 설정
        Portal.GetComponent<DungeonPortal>().SetPortalType(roomType == RoomType.Boss ?
            DungeonPortal.PortalType.ExitDungeon : DungeonPortal.PortalType.RoomComplete);

        Player.transform.position = new Vector3(XPos, -2.0f, Player.transform.position.z);
        Player.GetComponent<TestPlayerMove>().SetMoving(true);
    }

    public void RoomComplete()
    {
        //임시 : 포탈에 접촉하면 클리어 처리, 추후 클리어 판정이 변경될 수 있음
        Player.GetComponent<TestPlayerMove>().SetMoving(false);

        getDungeonRoom(CurrentRoomId).RoomClear();
        
        mapView.UpdateInteractableRooms(CurrentRoomId);
        mapView.SetVisible(true);
    }

    public bool IsPlayingRoom(){ return getDungeonRoom(CurrentRoomId).IsComplete == false; }

    //아래는 매니저 전용 함수들
    private void Awake()
    {
        if (Instance == null){ Instance = this; CreateMap(); }
        else Destroy(gameObject);
    }

    private void OnDestroy() { if (Instance == this) Clear(); }

    private void CreateMap()
    {
        DungeonGenerator generator = new DungeonGenerator(settings);
        DungeonData = new DungeonDataset();
        DungeonData.DungeonMap = generator.DungeonMap;
        CurrentRoomId = generator.StartId;
        BossRoomId = generator.BossId;
        DungeonData.FloorMap = generator.FloorMap;
    }

    private void Clear()
    {
        DungeonData.Clear();
        Instance = null;
    }
}
