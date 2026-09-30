using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Room = DungeonRoomNode;

public class DungeonMapView : MonoBehaviour
{
    [Header("Dungeon Map Room")]
    [SerializeField] private Transform FloorContainer;
    [SerializeField] private GameObject Floor;
    [SerializeField] private GameObject Btn_Room;
    [SerializeField] private GameObject Btn_EmptyRoom;

    [Header("Dungeon Map Line")]
    [SerializeField] private Transform LinesContainer;
    [SerializeField] private GameObject LinePrefab;
    [SerializeField] private float lineWidth = 6f; // 선 두께

    public void UpdateInteractableRooms()
    {
        int currentId = DungeonManager.Instance.CurrentRoomId;
        var currentRoom = DungeonManager.Instance.getDungeonRoom(currentId);

        //매니저의 데이터셋에서 버튼 딕셔너리 가져오기
        var roomButtons = DungeonManager.Instance.DungeonData.RoomButtons;

        //모든 버튼 비활성화
        foreach (var btnScript in roomButtons.Values)
        {
            btnScript.GetComponent<UnityEngine.UI.Button>().interactable = false;
        }

        //현재 방에서 갈 수 있는 방의 버튼들(Neighbors)만 활성화
        foreach (int nextId in currentRoom.Neighbors)
        {
            if (roomButtons.TryGetValue(nextId, out DungeonRoomButton targetBtn))
            {
                targetBtn.GetComponent<UnityEngine.UI.Button>().interactable = true;
            }
        }
    }

    void Start() { StartCoroutine(DrawMapView()); }

    private IEnumerator DrawMapView()
    {
        if (DungeonManager.Instance == null || DungeonManager.Instance.getDungeonRoom() == null)
        {
            Debug.LogError("DungeonMap이 아직 준비 안 됨!");
            yield break;
        }

        var map = DungeonManager.Instance.getDungeonRoom();
        int maxDepth = DungeonManager.Instance.getDepth();
        int maxWidth = DungeonManager.Instance.getWidth();

        // 방 버튼 & 더미 배치
        for (int i = maxDepth; i >= 0; i--)
        {
            bool isAlone = (i == maxDepth || i == 0);
            MakeFloor(i, isAlone);
        }

        // 1프레임 대기(버튼 정렬을 위해서)
        yield return null;
        Canvas.ForceUpdateCanvases();

        // 방들끼리 선 잇기
        DrawAllLines();

        //방만 활성화
        UpdateInteractableRooms();
    }

    private void CreateRoomButton(int roomId, Transform parent)
    {
        GameObject roomBtn = Instantiate(Btn_Room, parent);
        roomBtn.name = $"Room_{roomId}";

        var btnScript = roomBtn.GetComponent<DungeonRoomButton>();
        if (btnScript != null)
        {
            btnScript.SetId(roomId);
            // 데이터셋에 등록 완료
            DungeonManager.Instance.DungeonData.RoomButtons[roomId] = btnScript;
        }
    }

    private void MakeFloor(int floorIndex, bool alone = false)
    {
        GameObject floorObj = Instantiate(Floor, FloorContainer);
        floorObj.name = $"Floor_{floorIndex}";

        // 매니저에서 해당 층 방 리스트 가져옴
        var floorMap = DungeonManager.Instance.getFloor();
        if (!floorMap.TryGetValue(floorIndex, out List<int> roomList)) return;

        // 단독 방인 경우 (ex : 시작방 or 보스방)
        if (alone) { CreateRoomButton(roomList[0], floorObj.transform); return; }

        // 일반 층인 경우
        int maxWidth = DungeonManager.Instance.getWidth();
        for (int idx = 0; idx < maxWidth; idx++)
        {
            int roomId = (floorIndex * 100) + idx;

            // map 뒤질 필요 없이 해당 층 리스트만 훑기
            if (roomList.Contains(roomId))
            {
                CreateRoomButton(roomId, floorObj.transform);
            }
            else
            {
                GameObject emptyObj = Instantiate(Btn_EmptyRoom, floorObj.transform);
                emptyObj.name = $"Empty_{floorIndex}_{idx}";
            }
        }
    }

    private void DrawAllLines()
    {
        var map = DungeonManager.Instance.getDungeonRoom();
        var roomButtons = DungeonManager.Instance.DungeonData.RoomButtons;

        foreach (var pair in map)
        {
            int currentId = pair.Key;
            Room currentRoom = pair.Value;

            // 데이터셋 딕셔너리에 있는지 확인
            if (!roomButtons.ContainsKey(currentId)) continue;

            // DungeonRoomButton 스크립트에서 RectTransform 뽑아오기
            RectTransform startNode = roomButtons[currentId].GetComponent<RectTransform>();

            // 연결된 다음 방들(Neighbors)로 선 연결
            foreach (int neighborId in currentRoom.Neighbors)
            {
                if (roomButtons.TryGetValue(neighborId, out DungeonRoomButton targetBtn))
                {
                    RectTransform targetNode = targetBtn.GetComponent<RectTransform>();
                    CreateLine(startNode, targetNode);
                }
            }
        }
    }

    private void CreateLine(RectTransform startNode, RectTransform targetNode)
    {
        GameObject lineObj = Instantiate(LinePrefab, LinesContainer);
        RectTransform lineRect = lineObj.GetComponent<RectTransform>();

        // 시작점과 목표점의 월드 좌표 가져오기
        Vector3 startWorldPos = startNode.position;
        Vector3 targetWorldPos = targetNode.position;

        // 두 점 사이의 방향 벡터와 거리 계산
        Vector3 direction = targetWorldPos - startWorldPos;
        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 시작 노드 위치에 선 배치
        lineRect.position = startWorldPos;

        // Pivot Y가 0인 상태에서 목표 방향을 향하도록 -90도 보정 (위쪽 방향 기준)
        lineRect.rotation = Quaternion.Euler(0, 0, angle - 90f);

        // 선 두께와 길이 반영
        lineRect.sizeDelta = new Vector2(lineWidth, distance);
    }
}