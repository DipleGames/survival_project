using System;
using UnityEngine;

/// <summary>
/// 현재 상황에서 해골이 수행할 농사 세부 행동을 선택한다.
/// 수확 → 물주기 → 씨앗 심기 → 비료 주기 순으로 실행 가능 여부를 검사한다.
/// </summary>
public class FarmingActionSelector
{
    private readonly IFarmingAction _harvestAction = new HarvestAction();
    private readonly IFarmingAction _waterAction = new WaterAction();
    private readonly IFarmingAction _plantAction = new PlantAction();
    private readonly IFarmingAction _fertilizeAction = new FertilizeAction();

    public IFarmingAction SelectAction(bool isFarmingLevel2)
    {   
        if (!isFarmingLevel2)
        {
            if (CanExecute(_waterAction))
                return _waterAction;

            if (CanExecute(_harvestAction))
                return _harvestAction;

            return null;
        }

        if (CanExecute(_harvestAction))
            return _harvestAction;

        if (CanExecute(_waterAction))
            return _waterAction;

        if (CanExecute(_plantAction))
            return _plantAction;

        if (CanExecute(_fertilizeAction))
            return _fertilizeAction;

        return null;
    }

    private bool CanExecute(IFarmingAction action)
    {
        foreach (FarmArea area in TilemapFarmSystem.Instance.GetFarmAreas())
        {
            foreach (FarmTile tile in area.Tiles)
            {
                if (tile.IsReserved)
                    continue;

                if (action.CanExecute(tile))
                    return true;
            }
        }

        return false;
    }
}