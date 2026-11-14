using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Room = DungeonRoomNode;

public class DungeonGenerator
{
    public SortedDictionary<int, Room> DungeonMap { get; private set; }
    public Dictionary<int, List<int>> FloorMap { get; private set; }
    public int StartId { get; private set; }
    public int BossId { get; private set; }

    private DungeonSettings settings;
    private HashSet<int> lastFloorRooms;

    //방 Id 규격
    public int SetRoomId(int floor, int index) { return (floor * 100) + index; }

    public DungeonGenerator(DungeonSettings _settings)
    {
        settings = _settings;

        DungeonMap = new SortedDictionary<int, Room>();
        FloorMap = new Dictionary<int, List<int>>();

        //시작방
        StartId = SetRoomId(0, 0);
        DungeonMap.Add(StartId, new Room(StartId, RoomType.Start));

        //보스방
        BossId = SetRoomId(settings.Depth, 0);
        DungeonMap.Add(BossId, new Room(BossId, RoomType.Boss));

        lastFloorRooms = new HashSet<int>();

        //개미 수만큼 경로 탐색
        for (int i = 0; i < settings.AntCount; i++)
        {
            int startIdx = Random.Range(0, settings.Width);
            RunAntPath(startIdx);
        }

        //모아둔 마지막 층 방들만 뽑아서 보스방이랑 연결
        foreach (int roomId in lastFloorRooms) DungeonMap[roomId].AddNeighbor(BossId);

        //층별로 방 Id를 저장
        foreach (int roomId in DungeonMap.Keys)
        {
            int floor = roomId / 100;

            if (!FloorMap.ContainsKey(floor)) FloorMap[floor] = new List<int>();
            FloorMap[floor].Add(roomId);
        }

        //방 타입 지정
        AssignRoomTypes();
    }

    private void RunAntPath(int _start)
    {
        int curIndex = _start;
        int prevRoomId = StartId;

        //1층부터 보스 직전 층(depth - 1)까지 위로 올라가며 경로 탐색
        for (int i = 1; i < settings.Depth; i++)
        {
            //위로 움직일때 (-1: 왼쪽, 0: 직진, 1: 오른쪽)
            if (i > 1)
            {
                int step = Random.Range(-1, 2);
                int nextIndex = Mathf.Clamp(curIndex + step, 0, settings.Width - 1);

                //step의 방향이 이미 생성된 기존 길과 교차하는지 체크, 교차하면 직진함(step = 0)
                if (IsCrossingPath(i, curIndex, nextIndex)) nextIndex = curIndex;

                curIndex = nextIndex;
            }

            int curRoomId = SetRoomId(i, curIndex);

            //이동한 방이 생성된 방이 아니면 신규 방 노드 생성 후 맵에 등록
            if (!DungeonMap.ContainsKey(curRoomId)) DungeonMap.Add(curRoomId, new Room(curRoomId));

            //이전 방에서 현재 방으로 이동 가능한 단방향 길 연결
            DungeonMap[prevRoomId].AddNeighbor(curRoomId);
            prevRoomId = curRoomId;

            //보스 직전 마지막 층 방이면 후보 추가
            if (i == settings.Depth - 1) lastFloorRooms.Add(curRoomId);
        }
    }

    private bool IsCrossingPath(int floor, int fromIndex, int toIndex)
    {
        //직진은 교차하지 않음
        if (fromIndex == toIndex) return false;

        //오른쪽 이동: 오른쪽 위 방에서 왼쪽 아래로 향하는 길 검사
        if (toIndex > fromIndex)
        {
            int otherRoomId = SetRoomId(floor - 1, fromIndex + 1);
            int otherTargetId = SetRoomId(floor, fromIndex);

            return DungeonMap.ContainsKey(otherRoomId)
                && DungeonMap[otherRoomId].Neighbors.Contains(otherTargetId);
        }

        //왼쪽 이동: 왼쪽 위 방에서 오른쪽 아래로 향하는 길 검사
        if (toIndex < fromIndex)
        {
            int otherRoomId = SetRoomId(floor - 1, fromIndex - 1);
            int otherTargetId = SetRoomId(floor, fromIndex);

            return DungeonMap.ContainsKey(otherRoomId)
                && DungeonMap[otherRoomId].Neighbors.Contains(otherTargetId);
        }

        return false;
    }

    private void AssignRoomTypes()
    {
        float weightChance = 1f / (settings.Depth - 2);
        Dictionary<RoomType, float> chances = new();
        HashSet<RoomType> registeredTypes = new();

        //사용할 특수방 설정만 추출
        List<RoomTypeSettings> validRooms = new();

        foreach (var data in settings.Rooms)
        {
            //사용하지 않는 방 타입은 제외
            if (!data.UseRoomChance) continue;

            //확률이 모두 0인 경우 제외
            if (data.StartChance <= 0f && data.IncreaseChance <= 0f) continue;

            //시작방, 보스방, 일반방 제외
            if (data.RoomType == RoomType.Start ||
                data.RoomType == RoomType.Boss ||
                data.RoomType == RoomType.Normal)
                continue;

            //중복 등록 방지
            if (!registeredTypes.Add(data.RoomType)) continue;

            //특수방 리스트에 추가하고 시작 확률 등록
            chances.Add(data.RoomType, data.StartChance);
            validRooms.Add(data);
        }

        //층별 특수방 생성
        for (int floor = 1; floor < settings.Depth; floor++)
        {
            foreach (var data in validRooms)
            {
                chances[data.RoomType] = RollingChance(
                    floor,
                    chances[data.RoomType],
                    data.RoomType,
                    weightChance + data.IncreaseChance
                );
            }
        }
    }

    private float RollingChance(int floor, float chance, RoomType roomType, float weight)
    {
        //생성에 실패하면 다음 층으로 넘어가며 확률 증가
        if (chance < 1f && Random.value >= chance) return Mathf.Min(1f, chance + weight);

        //생성할 수 있는 방이 있는지 확인
        int cnt = 0;
        foreach (int roomId in FloorMap[floor]) if (DungeonMap[roomId].RoomType == RoomType.Normal) cnt++;

        //생성할 수 있는 방이 없으면 다음층에서 무조건 생성되도록 확률을 1로 설정
        if (cnt == 0) return 1.0f;
        
        int roomIdx = Random.Range(0, cnt);
        int index = 0;
        foreach (int roomId in FloorMap[floor])
        {
            //이미 특수방이 지정된 방은 제외
            if (DungeonMap[roomId].RoomType != RoomType.Normal) continue;

            //특수방이 지정될 방을 찾으면 RoomType 변경 후 종료
            if (index == roomIdx) { DungeonMap[roomId].RoomType = roomType; break; }
            index++;
        }

        return 0.0f;
    }
}
 