using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

public class PrimMazeWithPath : MonoBehaviour
{
    [Header("Maze Settings")]
    public int width = 21;
    public int height = 21;
    private int[,] maze;    // 0: 길, 1: 벽, 2: 랜덤 미로 후보 셀

    [Header("Tilemap Settings")]
    public Tilemap wallTilemap;   // 벽 타일맵
    public Tilemap groundTilemap; // 바닥 타일맵
    public TileBase wallTile;
    public TileBase pathTile;

    [Header("Navigation")]
    public Transform wallParent;
    public GameObject wallObstaclePrefab; // NavMeshObstacle 포함 프리팹
    public NavMeshSurface navMeshSurface; // Bake용
    public string groundLayerName = "Ground"; // 바닥에 사용할 레이어 이름

    [Header("Player")]
    public GameObject player; // NavMeshAgent 포함 오브젝트

    private Vector2Int start;
    private Vector2Int end;

    private List<Vector2Int> frontier = new List<Vector2Int>();
    private static readonly Vector2Int[] directions = {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    void Start()
    {
        // Tilemap에 레이어 적용
        groundTilemap.gameObject.layer = LayerMask.NameToLayer(groundLayerName);

        GenerateMaze();
        EnsurePathExists();
        RenderMaze();
        PlaceNavMeshObstacles();
        BakeNavMesh();
        MovePlayerToStart();
    }

    void GenerateMaze()
    {
        maze = new int[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                maze[x, y] = 1; // 벽으로 초기화

        // 시작과 끝 위치 지정
        start = new Vector2Int(1, 0);
        maze[start.x, start.y] = 0; // 길로 설정
        end = new Vector2Int(width - 2, height - 1);
        maze[end.x, end.y] = 0;

        Vector2Int randomStart = new Vector2Int(1, 1);
        maze[randomStart.x, randomStart.y] = 0;
        AddFrontiers(randomStart);

        while (frontier.Count > 0)
        {
            int index = Random.Range(0, frontier.Count);
            Vector2Int current = frontier[index];
            frontier.RemoveAt(index);

            List<Vector2Int> neighbors = GetValidNeighbors(current);
            if (neighbors.Count > 0)
            {
                Vector2Int neighbor = neighbors[Random.Range(0, neighbors.Count)];
                Vector2Int wallBetween = (current + neighbor) / 2;
                maze[current.x, current.y] = 0;
                maze[wallBetween.x, wallBetween.y] = 0;
                AddFrontiers(current);
            }
        }
    }

    void AddFrontiers(Vector2Int cell)
    {
        foreach (Vector2Int dir in directions)
        {
            Vector2Int next = cell + dir * 2;
            if (IsInside(next) && maze[next.x, next.y] == 1)
            {
                frontier.Add(next);
                maze[next.x, next.y] = 2;
            }
        }
    }

    List<Vector2Int> GetValidNeighbors(Vector2Int cell)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        foreach (Vector2Int dir in directions)
        {
            Vector2Int next = cell + dir * 2;
            if (IsInside(next) && maze[next.x, next.y] == 0)
                result.Add(next);
        }
        return result;
    }

    bool IsInside(Vector2Int p) =>
        p.x > 0 && p.x < width - 1 && p.y > 0 && p.y < height - 1;

    void EnsurePathExists()
    {
        if (BFS(start, end)) return;

        var path = BFSGetPath(start, end);
        foreach (Vector2Int p in path)
            maze[p.x, p.y] = 0;
    }

    bool BFS(Vector2Int s, Vector2Int e)
    {
        var queue = new Queue<Vector2Int>();
        var visited = new HashSet<Vector2Int>();
        queue.Enqueue(s);
        visited.Add(s);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            if (current == e) return true;

            foreach (Vector2Int dir in directions)
            {
                Vector2Int next = current + dir;
                if (IsInside(next) && maze[next.x, next.y] == 0 && !visited.Contains(next))
                {
                    queue.Enqueue(next);
                    visited.Add(next);
                }
            }
        }
        return false;
    }

    List<Vector2Int> BFSGetPath(Vector2Int s, Vector2Int e)
    {
        var parent = new Dictionary<Vector2Int, Vector2Int>();
        var queue = new Queue<Vector2Int>();
        queue.Enqueue(s);
        parent[s] = s;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            if (current == e) break;

            foreach (Vector2Int dir in directions)
            {
                Vector2Int next = current + dir;
                if (IsInside(next) && (maze[next.x, next.y] == 0 || next == e) && !parent.ContainsKey(next))
                {
                    queue.Enqueue(next);
                    parent[next] = current;
                }
            }
        }

        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int step = e;
        while (parent.ContainsKey(step) && step != s)
        {
            path.Add(step);
            step = parent[step];
        }
        return path;
    }

    void RenderMaze()
    {
        wallTilemap.ClearAllTiles();
        groundTilemap.ClearAllTiles();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (maze[x, y] == 1)
                    wallTilemap.SetTile(pos, wallTile);
                else
                    groundTilemap.SetTile(pos, pathTile);
            }
        }
    }

    void PlaceNavMeshObstacles()
    {
        for (int i=0; i< wallParent.childCount; i++)
        {
            Destroy(wallParent.GetChild(i).gameObject);
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (maze[x, y] != 1) continue;

                Vector3 worldPos = wallTilemap.CellToWorld(new Vector3Int(x, y, 0)) + wallTilemap.tileAnchor;
                worldPos.y = 0;
                worldPos.x -= 0.5f;
                worldPos.z += 0.25f;

                GameObject wall = Instantiate(wallObstaclePrefab, worldPos, wallObstaclePrefab.transform.rotation, wallParent);
                if (wall.GetComponentInChildren<ChangeTileAlpha>() != null)
                    wall.GetComponentInChildren<ChangeTileAlpha>().player = player;
            }
        }
    }

    void BakeNavMesh()
    {
        if (navMeshSurface != null)
        {
            navMeshSurface.layerMask = LayerMask.GetMask(groundLayerName);
            navMeshSurface.BuildNavMesh();
        }
    }

    void MovePlayerToStart()
    {
        if (player != null)
        {
            Vector3 worldPos = groundTilemap.CellToWorld(new Vector3Int(1, 1, 0)) + groundTilemap.tileAnchor;
            worldPos.y = 0;
            worldPos.z += 0.5f;

            if (NavMesh.SamplePosition(worldPos, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
            {
                Vector3 pos = hit.position;
                player.transform.position = pos;
                player.GetComponent<NavMeshAgent>().enabled = true;
            }
            else
            {
                Debug.LogWarning("Start 위치가 NavMesh 위에 없습니다.");
            }
        }
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Spawn Boss"))
        {
            GenerateMaze();
            EnsurePathExists();
            RenderMaze();
            PlaceNavMeshObstacles();
        }
    }
}