using UnityEngine;

/// <summary>
/// 여러 FarmArea 중 현재 농사 행동을 수행할 농작지를 선택한다.
/// 작업 가능 여부와 농작지 배정 우선순위를 기준으로 대상을 선정한다.
/// </summary>
public class FarmAreaSelector
{
    public FarmArea SelectArea(Vector3 skullPosition, IFarmingAction action)
    {
        FarmArea selectedArea = null;
        float nearestDistance = float.MaxValue;

        foreach (FarmArea area in TilemapFarmSystem.Instance.GetFarmAreas())
        {
            if (!HasAvailableTile(area, action))
                continue;

            Vector3 areaPosition = TilemapFarmSystem.Instance.GetWorldPosition(area.CenterCellPosition);
            float distance = Vector3.SqrMagnitude(areaPosition - skullPosition);

            if (distance >= nearestDistance)
                continue;

            nearestDistance = distance;
            selectedArea = area;
        }

        return selectedArea;
    }

    private bool HasAvailableTile(FarmArea area, IFarmingAction action)
    {
        foreach (FarmTile tile in area.Tiles)
        {
            if (tile.IsReserved)
                continue;

            if (action.CanExecute(tile))
                return true;
        }

        return false;
    }
}