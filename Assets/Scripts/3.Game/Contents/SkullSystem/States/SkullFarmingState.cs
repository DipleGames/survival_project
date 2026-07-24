using UnityEngine;

public class SkullFarmingState : IState
{
    private readonly SkullController _skull;

    private CropSO _currentCropSO;

    private float _productionTimer;
    private readonly float _productionInterval = 60f;

    private int _currentCropCount;

    public SkullFarmingState(SkullController skull)
    {
        _skull = skull;
    }

    public void EquipCrop(CropSO cropSO)
    {
        _currentCropSO = cropSO;
    }

    public void Enter()
    {
        Debug.Log("농사 시작");

        _productionTimer = 0f;

        _skull.StartMoveToNextPoint(WorkType.Farming);
    }

    public void Update()
    {
        if (_currentCropSO == null)
            return;

        _productionTimer += Time.deltaTime;

        if (_productionTimer < _productionInterval)
            return;

        _productionTimer -= _productionInterval;

        int productionAmount = WorkAreaManager.Instance.farmAreaList.Count;

        _currentCropCount += productionAmount;

        Debug.Log($"{_currentCropSO.name} 생산량: {_currentCropCount}");
    }

    public void Exit()
    {
        Debug.Log("농사 종료");

        _skull.StopMoveToNextPoint();
    }

    public int CollectCrops()
    {
        int collectedAmount = _currentCropCount;

        _currentCropCount = 0;

        return collectedAmount;
    }
}