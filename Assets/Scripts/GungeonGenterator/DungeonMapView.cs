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

    public void UpdateInteractableRooms(int roomID)
    {
        int currentId = roomID;
        int currentFloor = currentId / 100;
        var currentRoom = DungeonManager.Instance.getDungeonRoom(currentId);

        //매니저의 데이터셋에서 버튼 딕셔너리 가져오기
        var roomButtons = DungeonManager.Instance.DungeonData.RoomButtons;

        //버튼 비활성화 : 이전 층은 뭐...
        for(int i = currentFloor; i <= DungeonManager.Instance.getDepth(); ++i)
        {
            foreach(var rooms in DungeonManager.Instance.getFloor(i))
            {
                DungeonManager.Instance.DungeonData.RoomButtons[rooms].GetComponent<UnityEngine.UI.Button>().interactable = false;
            }
        }

        roomButtons[currentId].GetComponent<UnityEngine.UI.Button>().interactable = true;

        //현재 방에서 갈 수 있는 방의 버튼들(Neighbors)만 활성화
        foreach (int nextId in currentRoom.Neighbors)
        {
            if (roomButtons.TryGetValue(nextId, out DungeonRoomButton targetBtn))
            {
                targetBtn.GetComponent<UnityEngine.UI.Button>().interactable = true;
            }
        }
    }

    public void SetVisible(bool visible){ gameObject.SetActive(visible); }

    void Start() { StartCoroutine(DrawMapView()); }

    private IEnumerator DrawMapView()
    {
        if (DungeonManager.Instance == null || DungeonManager.Instance.getDungeonRoom() == null)
        {
            Debug.LogError("DungeonMap이 아직 준비 안 됨!");
            yield break;
        }

        var builder = new DungeonMapViewBuilder(
            FloorContainer,
            Floor,
            Btn_Room,
            Btn_EmptyRoom,
            LinesContainer,
            LinePrefab,
            lineWidth
        );

        // 방 UI 생성
        builder.BuildRooms();

        // 레이아웃 갱신 대기
        yield return null;
        Canvas.ForceUpdateCanvases();

        // 연결선 생성
        builder.DrawAllLines();

        // 버튼 상태 갱신
        UpdateInteractableRooms(0);

        // UI 숨기기
        SetVisible(false);
    }
}