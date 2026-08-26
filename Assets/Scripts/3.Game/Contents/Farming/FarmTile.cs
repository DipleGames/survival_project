using UnityEngine;

public class FarmTile
{
    public Vector3Int CellPosition { get; }
    public CropData Crop { get; private set; }

    public bool IsCultivated { get; private set; }
    public bool IsFertilized { get; private set; }
    public bool IsWatered { get; private set; }

    private float _waterTimer;

    public SkullController ReservedBy { get; private set; }

    public bool HasCrop => Crop != null;
    public bool IsReserved => ReservedBy != null;

    public bool CanPlant => IsCultivated && !HasCrop;
    public bool CanWater => IsCultivated && !IsWatered;
    public bool CanFertilize => HasCrop && !IsFertilized;
    public bool CanHarvest => HasCrop && Crop.growthStage == CropGrowthStage.Harvestable;

    public TilemapFarmSystem.FarmTileState State
    {
        get
        {
            return TilemapFarmSystem.Instance.GetTileState(CellPosition);
        }
    }

    public FarmTile(Vector3Int cellPosition)
    {
        CellPosition = cellPosition;
    }

    public bool Cultivate()
    {
        if (IsCultivated)
            return false;

        IsCultivated = true;
        return true;
    }

    public bool Plant(CropSO cropSO)
    {
        if (!CanPlant || cropSO == null)
            return false;

        Crop = new CropData
        {
            cropSO = cropSO,
            growthStage = CropGrowthStage.Seed,
            growthState = CropGrowthState.CannotGrow
        };

        return true;
    }

    public bool Water(float duration)
    {
        if (!CanWater)
            return false;

        IsWatered = true;
        _waterTimer = duration;

        return true;
    }

    public bool UpdateWater(float deltaTime)
    {
        if (!IsWatered)
            return false;

        _waterTimer -= deltaTime;

        if (_waterTimer > 0f)
            return false;

        IsWatered = false;
        _waterTimer = 0f;

        return true;
    }

    public bool ResetWater()
    {
        if (!IsWatered)
            return false;

        IsWatered = false;
        _waterTimer = 0f;

        return true;
    }

    public bool Fertilize()
    {
        if (!CanFertilize)
            return false;

        IsFertilized = true;
        return true;
    }

    public CropData Harvest()
    {
        if (!CanHarvest)
            return null;

        CropData harvestedCrop = Crop;

        Crop = null;
        IsFertilized = false;

        return harvestedCrop;
    }

    public bool TryReserve(SkullController skull)
    {
        if (IsReserved)
            return false;

        ReservedBy = skull;
        return true;
    }

    public void Release(SkullController skull)
    {
        if (ReservedBy != skull)
            return;

        ReservedBy = null;
    }

    public FarmArea Area { get; private set; }

    public void SetArea(FarmArea area)
    {
        Area = area;
    }
}