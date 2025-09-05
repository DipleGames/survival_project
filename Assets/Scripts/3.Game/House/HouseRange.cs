using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HouseRange : MonoBehaviour
{
    [SerializeField] private HouseManager houseManager;
    [SerializeField] private HouseRecipe recipe;
    [SerializeField] private Tilemap groundTilemap;
    private Vector3Int originCell;

    void Start()
    {
        if (groundTilemap) originCell = groundTilemap.WorldToCell(transform.position);
        else Debug.LogWarning($"[{name}] groundTilemap이 비어있습니다. 범위 체크 비활성화");
    }

    public bool CanInteractAtWorld(Vector3 worldPos)
    {
        if (!groundTilemap) return false;
        return CanInteractAtCell(groundTilemap.WorldToCell(worldPos));
    }

    public bool CanInteractAtCell(Vector3Int cell)
    {
        var r = recipe.Get(houseManager.Level);
        var range = r.activeRange;
        int dx = cell.x - originCell.x;
        int dy = cell.y - originCell.y;
        bool insideX = (-range.left  <= dx) && (dx <= range.right);
        bool insideY = (-range.down  <= dy) && (dy <= range.up);
        return insideX && insideY;
    }
}
