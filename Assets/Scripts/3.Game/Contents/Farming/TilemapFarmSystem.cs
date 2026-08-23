using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapFarmSystem : Singleton<TilemapFarmSystem>, IFarmSystem
{
    public enum FarmTileState
    {
        Empty,
        Cultivated,
        Occupied
    }

    [Header("타일맵")]
    public Tilemap groundTilemap;
    public Tilemap farmPlotTilemap;
    public Tilemap wetOverlayTilemap;
    public Tilemap fertilizedTilemap;
    public Tilemap cropTilemap;

    [Header("땅 타일")]
    [SerializeField] private TileBase _cultivatedTile;

    [Header("물 준 땅 오버레이 타일")]
    [SerializeField] private TileBase _wetOverlayTile;

    [Header("비료 준 땅 타일")]
    [SerializeField] private TileBase _fertilizedTile;

    [Header("물 지속 시간 = 하루")]
    [SerializeField] private float _waterDuration = 60f;

    // 실제 경작 타일
    private readonly Dictionary<Vector3Int, FarmTile> _farmTiles = new();

    // 농작지 목록
    private readonly List<FarmArea> _farmAreas = new();

    // 특정 셀이 어느 FarmArea에 속하는지 빠르게 찾기 위한 Dictionary
    private readonly Dictionary<Vector3Int, FarmArea> _farmAreaByCell = new();

    public bool isFarmMode = false;


    private void Update()
    {
        UpdateCrops();

        if (Input.GetKeyDown(KeyCode.F))
            isFarmMode = !isFarmMode;
    }


    // =========================================================
    // FarmArea
    // =========================================================

    public IEnumerable<FarmArea> GetFarmAreas()
    {
        return _farmAreas;
    }

    public FarmArea GetFarmArea(Vector3Int pos)
    {
        _farmAreaByCell.TryGetValue(pos, out FarmArea farmArea);
        return farmArea;
    }

    public void RegisterFarmArea(FarmArea farmArea, IEnumerable<Vector3Int> areaCells)
    {
        if (farmArea == null || _farmAreas.Contains(farmArea))
            return;

        _farmAreas.Add(farmArea);

        foreach (Vector3Int cellPos in areaCells)
        {
            if (_farmAreaByCell.ContainsKey(cellPos))
                continue;

            _farmAreaByCell.Add(cellPos, farmArea);

            // 이미 경작된 타일이라면 FarmArea에도 연결
            FarmTile farmTile = GetFarmTile(cellPos);

            if (farmTile == null)
                continue;

            farmTile.SetArea(farmArea);
            farmArea.AddTile(farmTile);
        }
    }

    public void UnregisterFarmArea(FarmArea farmArea)
    {
        if (farmArea == null)
            return;

        _farmAreas.Remove(farmArea);

        List<Vector3Int> removeCells = new();

        foreach (var pair in _farmAreaByCell)
        {
            if (pair.Value == farmArea)
                removeCells.Add(pair.Key);
        }

        foreach (Vector3Int cellPos in removeCells)
        {
            _farmAreaByCell.Remove(cellPos);

            FarmTile farmTile = GetFarmTile(cellPos);

            if (farmTile != null)
                farmTile.SetArea(null);
        }
    }


    // =========================================================
    // FarmTile
    // =========================================================

    public FarmTile GetFarmTile(Vector3Int pos)
    {
        _farmTiles.TryGetValue(pos, out FarmTile farmTile);
        return farmTile;
    }

    public IEnumerable<FarmTile> GetFarmTiles()
    {
        return _farmTiles.Values;
    }


    // =========================================================
    // 경작
    // =========================================================

    public void Cultivate(Vector3Int pos)
    {
        if (!IsFarmableTile(pos))
            return;

        if (!_farmTiles.TryGetValue(pos, out FarmTile farmTile))
        {
            farmTile = new FarmTile(pos);
            _farmTiles.Add(pos, farmTile);
        }

        if (!farmTile.Cultivate())
            return;

        // 이 좌표가 FarmArea에 속한다면 연결
        FarmArea farmArea = GetFarmArea(pos);

        if (farmArea != null)
        {
            farmTile.SetArea(farmArea);
            farmArea.AddTile(farmTile);
        }

        farmPlotTilemap.SetTile(pos, _cultivatedTile);
        wetOverlayTilemap.SetTile(pos, null);
        cropTilemap.SetTile(pos, null);
    }


    // =========================================================
    // 씨앗 심기
    // =========================================================

    public void Plant(Vector3Int pos, CropSO cropSO)
    {
        if (cropSO == null)
            return;

        FarmTile farmTile = GetFarmTile(pos);

        if (farmTile == null || !farmTile.Plant(cropSO))
            return;

        UpdateCropTile(pos, farmTile.Crop);
    }


    // =========================================================
    // 물주기
    // =========================================================

   public void Water(Vector3Int pos)
    {
        FarmTile farmTile = GetFarmTile(pos);

        if (farmTile == null || !farmTile.Water(_waterDuration))
            return;

        Debug.Log($"{pos}에 물을 주었다.");

        wetOverlayTilemap.SetTile(pos, _wetOverlayTile);
        UpdateCropTile(pos, farmTile.Crop);
    }

    // =========================================================
    // 비료
    // =========================================================

    public void Fertilize(Vector3Int pos)
    {
        FarmTile farmTile = GetFarmTile(pos);

        if (farmTile == null || !farmTile.Fertilize())
            return;

        Debug.Log($"{pos}에 비료를 주었다.");

        // TODO:
        // 비료에 따른 시각적 연출이 생기면 여기서 처리
        // fertilizedTilemap.SetTile(pos, _fertilizedTile);
        // UpdateCropTile(pos, farmTile.Crop);
    }


    // =========================================================
    // 수확
    // =========================================================

    public CropData Harvest(Vector3Int pos)
    {
        FarmTile farmTile = GetFarmTile(pos);

        if (farmTile == null)
            return null;

        CropData harvestedCrop = farmTile.Harvest();

        if (harvestedCrop == null)
            return null;

        cropTilemap.SetTile(pos, null);
        farmPlotTilemap.SetTile(pos, _cultivatedTile);

        return harvestedCrop;
    }


    // =========================================================
    // 작물 성장
    // =========================================================

    private void UpdateCrops()
    {
        foreach (FarmTile farmTile in _farmTiles.Values)
        {
            bool waterExpired = farmTile.UpdateWater(Time.deltaTime);

            if (waterExpired)
                wetOverlayTilemap.SetTile(farmTile.CellPosition, null);

            if (!farmTile.HasCrop)
                continue;

            CropData crop = farmTile.Crop;

            bool didGrow = crop.UpdateGrowth(Time.deltaTime, farmTile.IsWatered);

            if (didGrow)
                UpdateCropTile(farmTile.CellPosition, crop);
        }
    }

    // =========================================================
    // 작물 타일 이미지 갱신
    // =========================================================

    private void UpdateCropTile(Vector3Int pos, CropData crop)
    {
        if (crop == null || crop.cropSO == null)
            return;

        TileBase tile = crop.growthStage switch
        {
            CropGrowthStage.Seed => crop.cropSO.seedTile,
            CropGrowthStage.Sprout => crop.cropSO.sproutTile,
            CropGrowthStage.Growing => crop.cropSO.growingTile,
            CropGrowthStage.Mature => crop.cropSO.matureTile,
            CropGrowthStage.Harvestable => crop.cropSO.harvestableTile,
            _ => crop.cropSO.seedTile
        };

        cropTilemap.SetTile(pos, tile);
        farmPlotTilemap.SetTile(pos, _cultivatedTile);
    }


    // =========================================================
    // 물 초기화
    // =========================================================

    public void ResetWater(Vector3Int pos)
    {
        FarmTile farmTile = GetFarmTile(pos);

        if (farmTile == null || !farmTile.ResetWater())
            return;

        wetOverlayTilemap.SetTile(pos, null);
    }

    public void ResetAllWater()
    {
        foreach (FarmTile farmTile in _farmTiles.Values)
        {
            if (!farmTile.ResetWater())
                continue;

            wetOverlayTilemap.SetTile(farmTile.CellPosition, null);
        }
    }


    // =========================================================
    // 체크
    // =========================================================

    public bool IsFarmableTile(Vector3Int pos)
    {
        return farmPlotTilemap.HasTile(pos);
    }

    public Vector3 GetWorldPosition(Vector3Int pos)
    {
        return farmPlotTilemap.GetCellCenterWorld(pos);
    }

   public FarmTileState GetTileState(Vector3Int cellPosition)
    {
        FarmTile farmTile = GetFarmTile(cellPosition);

        if (farmTile == null)
            return FarmTileState.Empty;

        if (farmTile.HasCrop)
            return FarmTileState.Occupied;

        return FarmTileState.Cultivated;
    }
}