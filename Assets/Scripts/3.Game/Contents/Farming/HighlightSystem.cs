using UnityEngine;
using UnityEngine.Tilemaps;

public class HighlightSystem : Singleton<HighlightSystem>
{
    [Header("Player")]
    [SerializeField] private GameObject player;

    [Header("Tilemap")]
    [SerializeField] private Tilemap highlightTilemap;

    [Header("Highlight Tile")]
    [SerializeField] private TileBase highlightTile;
    [SerializeField] private TileBase failHighlightTile;

    private Camera _mainCamera;

    private Vector3Int _currentCell;
    private bool _hasHighlight;

    protected override void Awake()
    {
        base.Awake();

        _mainCamera = Camera.main;
        player = GameObject.Find("Shadow");
    }

    private void Update()
    {
        if (player == null)
            return;


        if (!TilemapFarmSystem.Instance.isFarmMode)
        {
            ClearHighlight();
            return;
        }

        Vector3Int targetCell = GetMouseCellPosition();

        if(!TilemapFarmSystem.Instance.IsFarmableTile(targetCell))
        {
            ClearHighlight();
            return;
        }

        if (CanInteract(targetCell))
        {
            // 같은 칸이면 아무것도 안함
            if (_hasHighlight && _currentCell == targetCell)
                return;

            ShowHighlight(targetCell);
        }
        else
        {
            ClearHighlight();
        }
    }

    private void ShowHighlight(Vector3Int cell)
    {
        ClearHighlight();

        highlightTilemap.SetTile(cell, highlightTile);

        _currentCell = cell;
        _hasHighlight = true;
    }

    private void ClearHighlight()
    {
        if (!_hasHighlight)
            return;

        highlightTilemap.SetTile(_currentCell, null);
        _hasHighlight = false;
    }

    public void ShowHighlightArea(Vector3Int[] cells)
    {
        ClearHighlight();

        foreach (Vector3Int cell in cells)
        {
            highlightTilemap.SetTile(cell, highlightTile);
        }
    }

    private Vector3Int GetMouseCellPosition()
    {
        if (_mainCamera == null)
            _mainCamera = Camera.main;

        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 worldPos = ray.GetPoint(distance);
            return TilemapFarmSystem.Instance.farmPlotTilemap.WorldToCell(worldPos);
        }

        return Vector3Int.zero;
    }

    private bool CanInteract(Vector3Int targetCell)
    {
        Vector3Int playerCell = TilemapFarmSystem.Instance.farmPlotTilemap.WorldToCell(player.transform.position);

        int diffX = Mathf.Abs(targetCell.x - playerCell.x);
        int diffY = Mathf.Abs(targetCell.y - playerCell.y);

        if (diffX == 0 && diffY == 0)
            return false;

        return diffX <= 1 && diffY <= 1;
    }
}