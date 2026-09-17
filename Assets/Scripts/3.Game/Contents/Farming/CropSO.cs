using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Crop/CropSO")]
public class CropSO : ScriptableObject
{
    [Header("작물 정보")]
    public string cropName;

    [Header("성장 정보")]
    public float growthTime;

    [Header("성장 단계 타일")]
    public TileBase seedTile;
    public TileBase sproutTile;
    public TileBase growingTile;
    public TileBase matureTile;
    public TileBase harvestableTile;
}