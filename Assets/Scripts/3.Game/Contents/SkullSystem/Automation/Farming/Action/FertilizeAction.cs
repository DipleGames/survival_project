using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 해골의 비료 주기 행동을 담당한다.
/// 비료 사용이 가능한 FarmTile인지 확인하고 해당 타일에 비료를 사용한다.
/// </summary>

public class FertilizeAction : IFarmingAction
{
    public bool CanExecute(FarmTile tile)
    {
        return tile != null && tile.CanFertilize;
    }

    public void Execute(FarmTile tile, SkullController skullController)
    {
        if (!CanExecute(tile))
            return;
        Debug.Log("비료줘삐");
        TilemapFarmSystem.Instance.Fertilize(tile.CellPosition);
    }
}
