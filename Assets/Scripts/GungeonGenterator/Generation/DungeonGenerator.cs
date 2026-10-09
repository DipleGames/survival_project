using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Room = DungeonRoomNode;

public class DungeonGenerator
{
    public SortedDictionary<int, Room> DungeonMap { get; private set; }
    public int StartId { get; private set; }
    public int BossId { get; private set; }


    private int depth;
    private int width;
    private int ant;

    private HashSet<int> lastFloorRooms;

    //방 Id 규격
    public int SetRoomId(int floor, int index) { return (floor * 100) + index; }

    public DungeonGenerator(int _depth, int _width, int _ant)
    {
        depth = _depth;
        width = _width;
        ant = _ant;
        lastFloorRooms = new HashSet<int>();

        DungeonMap = new SortedDictionary<int, Room>();

        //시작방
        StartId = SetRoomId(0, 0);
        DungeonMap.Add(StartId, new Room(StartId, RoomType.Start));

        //보스방
        BossId = SetRoomId(_depth, 0);
        DungeonMap.Add(BossId, new Room(BossId, RoomType.Boss));

        // 출발 지점에서 균형에 맞춰 출발시키게 함
        BalancedRandomPicker startPicker = new BalancedRandomPicker(width);
        for (int i = 0; i < ant; i++)
        {
            int startIdx = startPicker.GetNextIndex();
            RunAntPath(startIdx);
        }

        // 모아둔 마지막 층 방들만 뽑아서 보스방이랑 연결
        foreach (int roomId in lastFloorRooms) DungeonMap[roomId].AddNeighbor(BossId);
    }

    private void RunAntPath(int _start)
    {
        int curIndex = _start;
        int prevRoomId = StartId;

        // 1층부터 보스 직전 층(depth - 1)까지 위로 올라가며 경로 탐색
        for (int i = 1; i < depth; i++)
        {
            //위로 움직일때 (-1: 왼쪽, 0: 직진, 1: 오른쪽)
            if (i > 1)
            {
                int step = Random.Range(-1, 2);
                curIndex = Mathf.Clamp(curIndex + step, 0, width - 1);
            }

            int curRoomId = SetRoomId(i, curIndex);

            // 이동한 방이 생성된 방이 아니면 신규 방 노드 생성 후 맵에 등록
            if (!DungeonMap.ContainsKey(curRoomId)) DungeonMap.Add(curRoomId, new Room(curRoomId));

            // 이전 방에서 현재 방으로 이동 가능한 단방향 길 연결
            DungeonMap[prevRoomId].AddNeighbor(curRoomId);
            prevRoomId = curRoomId;

            // 보스 직전 마지막 층 방이면 후보 추가
            if (i == depth - 1) lastFloorRooms.Add(curRoomId);
        }
    }
}
 