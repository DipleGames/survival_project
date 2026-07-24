using UnityEngine;
using UnityEngine.Tilemaps;

public class FarmInteraction : MonoBehaviour
{
    [Header("플레이어 기준 축")]
    [SerializeField] private Transform player;

    [Header("심을 작물")]
    [SerializeField] private CropSO selectedCrop;
    [SerializeField] private CropSO tempCrop;

    private Camera _mainCamera;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }

        if (_mainCamera == null)
            return;

        if(!TilemapFarmSystem.Instance.isFarmMode)
            return;

        if (Input.GetKeyDown(KeyCode.T))
        {
            selectedCrop = selectedCrop == null ? tempCrop : null;
        }
        if (Input.GetMouseButtonDown(0))
        {
            if(selectedCrop != null)
            {
                TryPlant();
                TryHarvest();
            }
            else
            {
                TryCultivate();
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            TryWater();
        }
    }
    private void TryCultivate()
    {
        Vector3Int targetCell = GetMouseCellPosition();
        if (!CanInteract(targetCell))
        {
            Debug.Log($"상호작용 불가");
            return;
        }

        TilemapFarmSystem.Instance.Cultivate(targetCell);
    }

    private void TryPlant()
    {
        Vector3Int targetCell = GetMouseCellPosition();
        if (!CanInteract(targetCell))
        {
            Debug.Log($"상호작용 불가");
            return;
        }

        TilemapFarmSystem.Instance.Plant(targetCell, selectedCrop);
    }

    private void TryWater()
    {
        Vector3Int targetCell = GetMouseCellPosition();

        if (!CanInteract(targetCell))
            return;

        TilemapFarmSystem.Instance.Water(targetCell);
    }

    private void TryHarvest()
    {
        Vector3Int targetCell = GetMouseCellPosition();

        if (!CanInteract(targetCell))
            return;

        TilemapFarmSystem.Instance.Harvest(targetCell);
    }

    private Vector3Int GetMouseCellPosition()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 worldPos = ray.GetPoint(distance);
            return TilemapFarmSystem.Instance.fieldTilemap.WorldToCell(worldPos);
        }

        return Vector3Int.zero;
    }
    
    private bool CanInteract(Vector3Int targetCell)
    {
        Vector3Int playerCell = TilemapFarmSystem.Instance.fieldTilemap.WorldToCell(player.transform.position);

        int diffX = Mathf.Abs(targetCell.x - playerCell.x);
        int diffY = Mathf.Abs(targetCell.y - playerCell.y);

        Debug.Log($"targetCell: {targetCell}, playerCell: {playerCell}, diffX: {diffX}, diffY: {diffY}");

        if (diffX == 0 && diffY == 0)
            return false;

        return diffX <= 1 && diffY <= 1;
    }
}