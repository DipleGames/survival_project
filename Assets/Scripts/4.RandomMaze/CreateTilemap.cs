/*using UnityEngine;
using UnityEngine.Tilemaps;

public class CreateTilemap : MonoBehaviour
{
    public Tilemap tilemap;  // 씬에서 연결할 Tilemap
    public TileBase tile;    // 적용할 타일 애셋

    public int width = 10;
    public int height = 10;

    void Start()
    {
        GenerateTilemap();
    }

    void GenerateTilemap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
    }
}*/
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CreateTilemap : MonoBehaviour
{
    public int width = 21;  // 미로의 너비 (홀수)
    public int height = 21; // 미로의 높이 (홀수)
    public Tilemap tilemap; // Tilemap 참조
    public TileBase wallTile;  // 벽 타일
    public TileBase pathTile;  // 길 타일

    private int[,] maze;
    private List<Vector2Int> frontier = new List<Vector2Int>(); // 경계 리스트
    private static readonly Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    void Start()
    {
        GenerateMaze();
        RenderMaze();
    }

    void GenerateMaze()
    {
        maze = new int[width, height];

        // 1. 초기화: 모든 셀을 벽(1)으로 채운다
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                maze[x, y] = 1;
            }
        }
    

        // 2. 랜덤한 시작점 선택 (홀수 좌표)
        Vector2Int start = new Vector2Int(1, 0);
        maze[start.x, start.y] = 0; // 길로 설정

        Vector2Int end = new Vector2Int(width - 2, height - 1);
        maze[end.x, end.y] = 0;

        for (int x = 1; x < width - 1; x += 2)
        {
            for (int y = 1; y < height - 1; y += 2)
            {
                maze[x, y] = 0;
            }
        }

        //AddFrontiers(start);

        /*
        // 3. Prim 알고리즘 적용
        while (frontier.Count > 0)
        {
            int randomIndex = Random.Range(0, frontier.Count);
            Vector2Int cell = frontier[randomIndex];
            frontier.RemoveAt(randomIndex);

            List<Vector2Int> neighbors = GetValidNeighbors(cell);
            if (neighbors.Count > 0)
            {
                Vector2Int neighbor = neighbors[Random.Range(0, neighbors.Count)];
                Vector2Int wallBetween = (cell + neighbor) / 2;
                maze[cell.x, cell.y] = 0; // 길로 변경
                maze[wallBetween.x, wallBetween.y] = 0; // 벽을 허물어 길을 연결
                AddFrontiers(cell);
            }
        }*/
    }

    void AddFrontiers(Vector2Int cell)
    {
        foreach (Vector2Int dir in directions)
        {
            Vector2Int next = cell + dir * 2;
            if (IsInside(next) && maze[next.x, next.y] == 1)
            {
                frontier.Add(next);
                maze[next.x, next.y] = 2; // 후보지 표시
            }
        }
    }

    List<Vector2Int> GetValidNeighbors(Vector2Int cell)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        foreach (Vector2Int dir in directions)
        {
            Vector2Int next = cell + dir * 2;
            if (IsInside(next) && maze[next.x, next.y] == 0)
            {
                neighbors.Add(next);
            }
        }
        return neighbors;
    }

    bool IsInside(Vector2Int pos) => pos.x > 0 && pos.x < width - 1 && pos.y > 0 && pos.y < height - 1;

    void RenderMaze()
    {
        tilemap.ClearAllTiles();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                tilemap.SetTile(pos, maze[x, y] == 1 ? wallTile : pathTile);
            }
        }
    }
}