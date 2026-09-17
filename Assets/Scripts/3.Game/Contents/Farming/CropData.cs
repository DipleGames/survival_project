using UnityEngine;

public enum CropGrowthStage
{
    Seed = 0,
    Sprout = 1,
    Growing = 2,
    Mature = 3,
    Harvestable = 4
}

public enum CropGrowthState
{
    CannotGrow,
    CanGrow
}

[System.Serializable]
public class CropData
{
    public CropSO cropSO;

    public CropGrowthStage growthStage;
    public CropGrowthState growthState;

    private float _growthTimer;

    public bool UpdateGrowth(float deltaTime, bool isWatered)
    {
        if (growthStage == CropGrowthStage.Harvestable)
            return false;

        growthState = isWatered ? CropGrowthState.CanGrow : CropGrowthState.CannotGrow;

        if (growthState != CropGrowthState.CanGrow)
            return false;

        _growthTimer += deltaTime;

        if (_growthTimer < cropSO.growthTime)
            return false;

        Growth();
        _growthTimer = 0f;

        return true;
    }

    private void Growth()
    {
        if (growthStage == CropGrowthStage.Harvestable)
            return;

        growthStage++;

        Debug.Log($"{growthStage}로 성장했습니다");
    }
}