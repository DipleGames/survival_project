using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Room = DungeonRoomNode;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance { get; private set; }

    [Header("Dungeon Settings(ScriptableObject)")]
    [SerializeField] private DungeonSettings settings;
    [SerializeField] private DungeonRoomPositionSettings PositionByRoomTypeSettings;

    [Header("Dungeon GameObject")]
    [SerializeField] private DungeonMapView mapView;
    [SerializeField] private DungeonMonsterSpawn monsterSpawner;

    [Header("Dungeon Prefab")]
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
    public SortedDictionary<int, Room> getDungeonRoom() { return DungeonData.DungeonMap; }
    public Room getDungeonRoom(int id) { return DungeonData.DungeonMap[id]; }
    public Dictionary<int, List<int>> getFloor() { return DungeonData.FloorMap; }
    public List<int> getFloor(int floor) { return DungeonData.FloorMap[floor]; }

    public bool IsPlayingRoom() { return getDungeonRoom(CurrentRoomId).IsComplete == false; }
    public RoomTypePositionData GetRoomPosition(RoomType roomType) { return PositionByRoomTypeSettings.GetRoomPosition(roomType); }

    public void RoomComplete()
    {
        getDungeonRoom(CurrentRoomId).RoomClear();
        Portal.GetComponent<DungeonPortal>().SetDoorState(true);
    }

    public void DisplayMapView()
    {
        GameManager.Instance.Player.GetComponent<PlayerMove>().MovementLocked = true;

        mapView.UpdateInteractableRooms(CurrentRoomId);
        mapView.DisplayMapView(true);
    }

    public void MoveTo(int roomId)
    {
        int prevRoomId = CurrentRoomId;
        // 실제 방 이동
        CurrentRoomId = roomId;
        // 지나온 연결선 색칠
        mapView.SetVisitedLine(prevRoomId, roomId);

        // 이동할 방의 타입 확인
        RoomType roomType = getDungeonRoom(CurrentRoomId).RoomType;

        //포탈 데이터 갱신
        DungeonPortal portal = Portal.GetComponent<DungeonPortal>();
        portal.UpdatePortalInDungeonData(roomType);
        portal.SetDoorState(false);

        // 상점이나 보물방은 이동 즉시 클리어 처리
        if (roomType == RoomType.Shop || roomType == RoomType.Treasure) { RoomComplete(); }

        //플레이어 위치 이동
        GameObject player = GameManager.Instance.Player.gameObject;
        player.transform.position = GetRoomPosition(roomType).PlayerPosition;

        // 카메라 갱신 후 맵 닫기
        StartCoroutine(FinishRoomMove(player));
    }

    private IEnumerator FinishRoomMove(GameObject player)
    {
        // 카메라가 새 플레이어 위치를 반영할 프레임 확보
        yield return null;
        yield return new WaitForEndOfFrame();

        player.GetComponent<PlayerMove>().MovementLocked = false;
        mapView.DisplayMapView(false);

        yield return new WaitForEndOfFrame();
        monsterSpawner.UpdateMonsterSpawner(getDungeonRoom(CurrentRoomId).RoomType);
    }

    //아래는 매니저 전용 함수들
    private void Awake()
    {
        if (Instance == null) { Instance = this; CreateMap(); }
        else Destroy(gameObject);
    }

    public void Start()
    {
        //시작 방은 클리어 상태로 설정 
        RoomComplete();

        //플레이어 전투모드 활성화
        GameObject player = GameManager.Instance.Player.gameObject;
        player.GetComponent<PlayerCombatController>().isCombatMode = true;
        //플레이어 위치 초기화
        player.transform.position = GetRoomPosition(RoomType.Start).PlayerPosition;

        //포탈 위치 초기화
        Portal.transform.position = GetRoomPosition(RoomType.Start).PortalPosition;

        //몬스터 스포너 위치 설정
        monsterSpawner.UpdateMonsterSpawner(RoomType.Start);
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

    public void Update()
    {
        //Debug용으로 스페이스바 누르면 현재 방 클리어 처리
        if (Input.GetKeyDown(KeyCode.Space)) RoomComplete();
    }
}
