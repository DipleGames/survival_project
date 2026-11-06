using System.Collections.Generic;
using UnityEngine;

using Room = DungeonRoomNode;

public class DungeonMapViewBuilder
{
    private readonly Transform floorContainer;
    private readonly GameObject floorPrefab;
    private readonly GameObject roomPrefab;
    private readonly GameObject emptyRoomPrefab;

    private readonly Transform linesContainer;
    private readonly GameObject linePrefab;
    private readonly float lineWidth;

    public DungeonMapViewBuilder(
        Transform floorContainer,
        GameObject floorPrefab,
        GameObject roomPrefab,
        GameObject emptyRoomPrefab,
        Transform linesContainer,
        GameObject linePrefab,
        float lineWidth)
    {
        this.floorContainer = floorContainer;
        this.floorPrefab = floorPrefab;
        this.roomPrefab = roomPrefab;
        this.emptyRoomPrefab = emptyRoomPrefab;
        this.linesContainer = linesContainer;
        this.linePrefab = linePrefab;
        this.lineWidth = lineWidth;
    }

    public void BuildRooms()
    {
        int maxDepth = DungeonManager.Instance.getDepth();

        for (int i = maxDepth; i >= 0; i--)
        {
            bool isAlone = (i == maxDepth || i == 0);
            MakeFloor(i, isAlone);
        }
    }

    private void CreateRoomButton(int roomId, Transform parent)
    {
        GameObject roomBtn = Object.Instantiate(roomPrefab, parent);

        var btnScript = roomBtn.GetComponent<DungeonRoomButton>();
        if (btnScript != null)
        {
            btnScript.SetId(roomId);
            DungeonManager.Instance.DungeonData.RoomButtons[roomId] = btnScript;
        }
    }

    private void MakeFloor(int floorIndex, bool alone = false)
    {
        GameObject floorObj = Object.Instantiate(floorPrefab, floorContainer);

        var floorMap = DungeonManager.Instance.getFloor();
        if (!floorMap.TryGetValue(floorIndex, out List<int> roomList)) return;

        if (alone){CreateRoomButton(roomList[0], floorObj.transform);return; }

        int maxWidth = DungeonManager.Instance.getWidth();

        for (int idx = 0; idx < maxWidth; idx++)
        {
            int roomId = (floorIndex * 100) + idx;

            if (roomList.Contains(roomId)) CreateRoomButton(roomId, floorObj.transform);
            else
            {
                GameObject emptyObj = Object.Instantiate(emptyRoomPrefab, floorObj.transform);
            }
        }
    }

    public void DrawAllLines()
    {
        var map = DungeonManager.Instance.getDungeonRoom();
        var roomButtons = DungeonManager.Instance.DungeonData.RoomButtons;

        foreach (var pair in map)
        {
            int currentId = pair.Key;
            Room currentRoom = pair.Value;

            if (!roomButtons.ContainsKey(currentId)) continue;

            RectTransform startNode = roomButtons[currentId].GetComponent<RectTransform>();

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
        GameObject lineObj = Object.Instantiate(linePrefab, linesContainer);

        RectTransform lineRect = lineObj.GetComponent<RectTransform>();

        Vector3 startWorldPos = startNode.position;
        Vector3 targetWorldPos = targetNode.position;

        Vector3 direction = targetWorldPos - startWorldPos;
        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        lineRect.position = startWorldPos;
        lineRect.rotation = Quaternion.Euler(0, 0, angle - 90f);
        lineRect.sizeDelta = new Vector2(lineWidth, distance);
    }
}