using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FarmPlotArea : MonoBehaviour
{
    [SerializeField] private Tilemap _areaTilemap;

    private FarmArea _farmArea;

    private void Start()
    {
        RegisterArea();
    }

    private void RegisterArea()
    {
        List<Vector3Int> areaCells = new();

        foreach (Vector3Int localCell in _areaTilemap.cellBounds.allPositionsWithin)
        {
            if (!_areaTilemap.HasTile(localCell))
                continue;

            Vector3 worldPosition = _areaTilemap.GetCellCenterWorld(localCell);

            Vector3Int fieldCell = TilemapFarmSystem.Instance.groundTilemap.WorldToCell(worldPosition);

            areaCells.Add(fieldCell);
        }

        if (areaCells.Count == 0)
            return;

        // 3x3 FarmPlot의 중앙 위치
        Vector3Int centerCell = TilemapFarmSystem.Instance.groundTilemap.WorldToCell(transform.position);

        _farmArea = new FarmArea(centerCell);

        TilemapFarmSystem.Instance.RegisterFarmArea( _farmArea, areaCells);

        Debug.Log($"FarmArea 등록 완료 / 중심 : {centerCell} / 영역 : {areaCells.Count}칸");
    }
}