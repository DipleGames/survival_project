using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HouseRange : MonoBehaviour
{
    [SerializeField] private HouseManager houseManager;
    [SerializeField] private HouseRecipe recipe;
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private GameObject wallObject;
    private Vector3Int originCell;
    private readonly List<GameObject> currentWalls = new();

    void Start()
    {
        if (groundTilemap) originCell = groundTilemap.WorldToCell(transform.position);
        else Debug.LogWarning($"[{name}] groundTilemap이 비어있습니다. 범위 체크 비활성화");
        if (houseManager)
        {
            houseManager.OnLevelChanged += UpdateWalls;
            UpdateWalls(houseManager.Level);
        }
    }

    void OnDestroy()
    {
        if (houseManager) houseManager.OnLevelChanged -= UpdateWalls;
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
        bool insideX = (-range.left <= dx) && (dx <= range.right);
        bool insideY = (-range.down <= dy) && (dy <= range.up);
        return insideX && insideY;
    }
    
    private void UpdateWalls(int level)
    {
        if(level < 1) return;
        // 기존 벽 제거
        foreach (var wall in currentWalls)
            if (wall) Destroy(wall);
        currentWalls.Clear();
        var r = recipe.Get(level);
        var range = r.activeRange;
        // 테두리 셀 계산
        for (int x = -range.left; x <= range.right; x++)
        {
            for (int y = -range.down; y <= range.up; y++)
            {
                bool isEdge = (x == -range.left || x == range.right || y == -range.down || y == range.up);
                if (!isEdge) continue;
                var cell = new Vector3Int(originCell.x + x, originCell.y + y, originCell.z);
                var pos = groundTilemap.GetCellCenterWorld(cell);
                if (wallObject)
                {
                    var wall = Instantiate(wallObject, pos, Quaternion.Euler(90f, 0f, 0f), transform);
                    currentWalls.Add(wall);
                }
            }
        }
        Debug.Log($"레벨 {level} 울타리 범위 갱신 완료 ({currentWalls.Count}개)");
    }
}
