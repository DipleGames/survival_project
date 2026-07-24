using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public enum WorkType
{
    Farming,
    Mining,
    Fishing
}


public class WorkAreaManager : Singleton<WorkAreaManager>
{
    [SerializeField] private Tilemap _workTilemap;

    private readonly Dictionary<Vector3Int, WorkAreaData> _workAreas = new();
    public Dictionary<Vector3Int, WorkAreaData> WorkAreas => _workAreas;
    public List<Vector3Int> farmAreaList = new();

    public void RegisterWorkArea(Vector3Int cellPosition, WorkType workType)
    {
        if (_workAreas.ContainsKey(cellPosition))
        {
            Debug.LogWarning($"이미 등록된 작업 위치입니다: {cellPosition}");
            return;
        }

        WorkAreaData workArea = new WorkAreaData(cellPosition, workType);

        _workAreas.Add(cellPosition, workArea);
        switch(workArea.WorkType)
        {
            case WorkType.Farming:
                farmAreaList.Add(cellPosition);
                break;
            case WorkType.Mining:
                break;
            case WorkType.Fishing:
                break;
        }
        Debug.Log($"{cellPosition.x}, {cellPosition.y} 에 {workType}을 등록하였습니다.");
    }

    public WorkAreaData GetWorkArea(Vector3Int cellPosition)
    {
        _workAreas.TryGetValue(cellPosition, out WorkAreaData workArea);

        return workArea;
    }

    public Vector3 GetWorldPosition(Vector3Int cellPosition)
    {
        return _workTilemap.GetCellCenterWorld(cellPosition);
    }

    public WorkAreaData GetNearestAvailableArea(WorkType workType, Vector3 skullPosition)
    {
        WorkAreaData nearestArea = null;
        float nearestDistance = float.MaxValue;

        foreach (WorkAreaData area in _workAreas.Values)
        {
            if (area.WorkType != workType)
                continue;

            if (area.IsReserved)
                continue;

            Vector3 areaWorldPosition =
                _workTilemap.GetCellCenterWorld(area.CellPosition);

            float distance =
                Vector3.SqrMagnitude(areaWorldPosition - skullPosition);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestArea = area;
            }
        }

        return nearestArea;
    }
}
