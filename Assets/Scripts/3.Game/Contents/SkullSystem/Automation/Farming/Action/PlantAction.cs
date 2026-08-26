using UnityEngine;

/// <summary>
/// 해골의 씨앗 심기 행동을 담당한다.
/// 씨앗을 심을 수 있는 FarmTile인지 확인하고 작물을 심는다.
/// </summary>
public class PlantAction : IFarmingAction
{
    public bool CanExecute(FarmTile tile)
    {
        return tile != null && !tile.HasCrop;
    }

    public void Execute(FarmTile tile, SkullController skullController)
    {
        if (!CanExecute(tile))
            return;
        Debug.Log("씨앗심어삐");
        TilemapFarmSystem.Instance.Plant(tile.CellPosition, skullController.equippedCropSO);
    }
}