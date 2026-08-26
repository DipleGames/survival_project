using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeGridRenderer : MonoBehaviour
{
    [SerializeField] private Grid _grid;

    [Header("그리드 크기")]
    [SerializeField] private int _height = 100;
    [SerializeField] private int _width = 100;

    [Header("선 옵션")]
    [SerializeField] private Material _lineMaterial;
    [SerializeField] private float _lineWidth = 0.03f;
    [SerializeField] private int _sortingOrder = 10;

    void Start()
    {
        DrawGrid();
        gameObject.SetActive(false);
    }

    public void DrawGrid()
    {
        // 세로선
        for (int x = -_width; x <= _width; x++)
        {
            Vector3 start = _grid.CellToWorld(new Vector3Int(x, -_height, 0));
            Vector3 end = _grid.CellToWorld(new Vector3Int(x, _height, 0));

            CreateLine($"Vertical_{x}", start, end);
        }

        // 가로선
        for (int y = -_height; y <= _height; y++)
        {
            Vector3 start = _grid.CellToWorld(new Vector3Int(-_width, y, 0));
            Vector3 end = _grid.CellToWorld(new Vector3Int(_width, y, 0));

            CreateLine($"Horizontal_{y}", start, end);
        }
    }

    private void CreateLine(string lineName, Vector3 start, Vector3 end)
    {
        GameObject lineObject = new GameObject(lineName);
        lineObject.transform.SetParent(transform);

        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        lineRenderer.startWidth = _lineWidth;
        lineRenderer.endWidth = _lineWidth;

        lineRenderer.material = _lineMaterial;

        lineRenderer.useWorldSpace = true;
        lineRenderer.sortingOrder = _sortingOrder;
    }


}
