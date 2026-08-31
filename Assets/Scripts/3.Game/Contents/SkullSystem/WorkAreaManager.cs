using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum WorkType
{
    Farming,
    Mining,
    Logging
}


/// <summary>
/// 게임 내 등록된 WorkArea들을 관리한다.
/// 작업 영역의 등록, 제거, 조회 기능을 제공한다.
/// </summary>
public class WorkAreaManager : Singleton<WorkAreaManager>
{
    [SerializeField] private Tilemap _workTilemap;

    private readonly Dictionary<Vector3Int, WorkAreaData> _workAreas = new();

    public IReadOnlyDictionary<Vector3Int, WorkAreaData> WorkAreas => _workAreas;


    // 작업 지역 등록
    public bool RegisterWorkArea(Vector3Int cellPosition, WorkType workType)
    {
        if (_workAreas.ContainsKey(cellPosition))
        {
            Debug.LogWarning($"이미 등록된 작업 위치입니다: {cellPosition}");
            return false;
        }

        WorkAreaData workArea = new WorkAreaData(
            cellPosition,
            workType
        );

        _workAreas.Add(cellPosition, workArea);

        Debug.Log(
            $"{cellPosition.x}, {cellPosition.y}에 {workType}을 등록했습니다."
        );

        return true;
    }


    // 작업 지역 제거
    public bool RemoveWorkArea(Vector3Int cellPosition)
    {
        return _workAreas.Remove(cellPosition);
    }


    // 특정 위치 작업 지역 조회
    public WorkAreaData GetWorkArea(Vector3Int cellPosition)
    {
        _workAreas.TryGetValue(
            cellPosition,
            out WorkAreaData workArea
        );

        return workArea;
    }


    // 특정 WorkType의 작업 지역들 조회
    public IEnumerable<WorkAreaData> GetWorkAreas(WorkType workType)
    {
        foreach (WorkAreaData area in _workAreas.Values)
        {
            if (area.WorkType == workType)
                yield return area;
        }
    }


    // 셀 → 월드 좌표
    public Vector3 GetWorldPosition(Vector3Int cellPosition)
    {
        return _workTilemap.GetCellCenterWorld(cellPosition);
    }
}