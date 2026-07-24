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

    public enum FarmWorkResult 
    {
        Planted,
        Watered,
        Harvested,
        NoWork
    }

    [Header("타일맵")]
    public Tilemap fieldTilemap;
    public Tilemap wetOverlayTilemap;
    public Tilemap cropTilemap;

    [Header("땅 타일")]
    [SerializeField] private TileBase cultivatedTile;

    [Header("물 준 땅 오버레이 타일")]
    [SerializeField] private TileBase wetOverlayTile;

    private Dictionary<Vector3Int, FarmTileState> _farmTileDict = new();
    private Dictionary<Vector3Int, CropData> _cropDict = new();

    public bool isFarmMode = false; 

    private void Update()
    {
        foreach (var pair in _cropDict)
        {
            CropData crop = pair.Value;

            if (crop.UpdateGrowth(Time.deltaTime))
            {
                UpdateCropTile(pair.Key, crop);
                wetOverlayTilemap.SetTile(pair.Key, null);
            }
        }

        if(Input.GetKeyDown(KeyCode.F))
        {
            isFarmMode = isFarmMode == true ? false : true;
        }
    }

    public void Cultivate(Vector3Int pos)
    {
        if (!IsFarmableTile(pos)) // 밭 (경작할수있는 땅이아니면 리턴)
            return;

        if (_farmTileDict.TryGetValue(pos, out FarmTileState state))
        {
            if (state != FarmTileState.Empty)
                return;
        }

        _farmTileDict[pos] = FarmTileState.Cultivated;

        fieldTilemap.SetTile(pos, cultivatedTile);
        wetOverlayTilemap.SetTile(pos, null);
        cropTilemap.SetTile(pos, null);

        WorkAreaManager.Instance.RegisterWorkArea(pos, WorkType.Farming);
    }

    public void Plant(Vector3Int pos, CropSO cropSO)
    {
        if (cropSO == null)
            return;
        
        if (!IsFarmableTile(pos)) // 밭 (경작할수있는 땅이아니면 리턴)
            return;

        if (!_farmTileDict.TryGetValue(pos, out FarmTileState state))
            return;

        if (state != FarmTileState.Cultivated)
            return;

        if (_cropDict.ContainsKey(pos))
            return;

        CropData crop = new CropData
        {
            cropSO = cropSO,
            growthStage = CropGrowthStage.Seed,
            growthState = CropGrowthState.CannotGrow,
            isWatered = false
        };

        _cropDict.Add(pos, crop);
        _farmTileDict[pos] = FarmTileState.Occupied;

        UpdateCropTile(pos, crop);
    }

    public void Water(Vector3Int pos)
    {
        if (!IsFarmableTile(pos)) // 밭 (경작할수있는 땅이아니면 리턴)
            return;

        if (_cropDict.TryGetValue(pos, out CropData crop))
        {
            Debug.Log($"{crop}에 물을 주었다");
            crop.Water();

            wetOverlayTilemap.SetTile(pos, wetOverlayTile);

            UpdateCropTile(pos, crop);
        }
    }

    public void Harvest(Vector3Int pos)
    {
        if (!IsFarmableTile(pos)) // 밭 (경작할수있는 땅이아니면 리턴)
            return;

        if (!_cropDict.TryGetValue(pos, out CropData crop))
            return;

        if (crop.growthStage != CropGrowthStage.Harvestable)
            return;

        _cropDict.Remove(pos);
        _farmTileDict[pos] = FarmTileState.Cultivated;

        cropTilemap.SetTile(pos, null);
        wetOverlayTilemap.SetTile(pos, null);
        fieldTilemap.SetTile(pos, cultivatedTile);
    }

    public void UpdateCropTile(Vector3Int pos, CropData crop)
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
        fieldTilemap.SetTile(pos, cultivatedTile);
    }

    public bool IsFarmableTile(Vector3Int pos)
    {
        return fieldTilemap.HasTile(pos);
    }

    public void ResetWater(Vector3Int pos)
    {
        if (_cropDict.TryGetValue(pos, out CropData crop))
        {
            crop.isWatered = false;
            wetOverlayTilemap.SetTile(pos, null);
        }
    }

    public void ResetAllWater()
    {
        foreach (var pair in _cropDict)
        {
            pair.Value.isWatered = false;
            wetOverlayTilemap.SetTile(pair.Key, null);
        }
    }
}