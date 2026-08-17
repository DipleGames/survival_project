using UnityEngine;


/// <summary>
/// 선택된 FarmArea 내부에서 실제 작업할 FarmTile을 선택한다.
/// 행동 수행 가능 여부, 예약 여부, 해골과의 거리 등을 기준으로 타일을 선정한다.
/// </summary>
public class FarmTileSelector
{
    public FarmTile SelectNearestTile(Vector3 skullPosition, FarmArea area, IFarmingAction action)
    {
        FarmTile nearestTile = null;
        float nearestDistance = float.MaxValue;

        foreach (FarmTile tile in area.Tiles)
        {
            if (tile.IsReserved || !action.CanExecute(tile))
                continue;

            Vector3 tilePosition = TilemapFarmSystem.Instance.farmPlotTilemap.GetCellCenterWorld(tile.CellPosition);
            float distance = Vector3.SqrMagnitude(tilePosition - skullPosition);

            if (distance >= nearestDistance)
                continue;

            nearestDistance = distance;
            nearestTile = tile;
        }
        Debug.Log(nearestTile);
        return nearestTile;
    }
}