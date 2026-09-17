using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 해골의 물주기 행동을 담당한다.
/// 물주기가 가능한 FarmTile인지 확인하고 해당 타일에 물을 준다.
/// </summary>

public class WaterAction : IFarmingAction
{
    public bool CanExecute(FarmTile tile)
    {
        return tile != null && tile.CanWater;
    }

    public void Execute(FarmTile tile, SkullController skullController)
    {
        if (!CanExecute(tile))
            return;
        Debug.Log("물줘삐");
        TilemapFarmSystem.Instance.Water(tile.CellPosition);
    }
}
