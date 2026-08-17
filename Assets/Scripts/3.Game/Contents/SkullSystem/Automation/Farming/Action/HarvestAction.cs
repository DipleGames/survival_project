using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 해골의 수확 행동을 담당한다.
/// 수확 가능한 FarmTile인지 확인하고 작물을 수확한다.
/// </summary>

public class HarvestAction : IFarmingAction
{
    public bool CanExecute(FarmTile tile)
    {
        return tile != null && tile.CanHarvest;
    }

    public void Execute(FarmTile tile, SkullController skullController)
    {
        if (!CanExecute(tile))
            return;
        Debug.Log("수확해삐");
        TilemapFarmSystem.Instance.Harvest(tile.CellPosition);
    }
}
