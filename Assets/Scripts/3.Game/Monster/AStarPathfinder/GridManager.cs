using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Isometric Tile (World Units)")]
    public float TileWidth = 2f;
    public float TileHeight = 1f;

    [Header("Grid Size (in tiles)")]
    public int GridSizeI = 50;
    public int GridSizeJ = 50;

    [Header("Origin (tile index to world mapping)")]
    public Vector2Int OriginIJ = new Vector2Int(0, 0);

    [Header("Optional: Physics-based unwalkable check")]
    public bool UsePhysicsUnwalkable = false;
    public LayerMask UnwalkableMask;
    public float PhysicsCheckRadius = 0.2f;

    [Header("Neighbors")]
    public bool UseDiagonal = true;

    private Node[,] _grid;

    private readonly HashSet<Vector2Int> _blockedNodeCoords = new HashSet<Vector2Int>();
    public List<Vector2Int> blockedNodeList = new List<Vector2Int>();

    public int MaxSize => GridSizeI * GridSizeJ;

    private float HalfW => TileWidth * 0.5f;
    private float HalfH => TileHeight * 0.5f;

    private void Awake()
    {
        CreateGrid();
    }

    private Vector3 IJToWorld(int i, int j)
    {
        int ii = i - OriginIJ.x;
        int jj = j - OriginIJ.y;

        float x = (ii - jj) * HalfW;
        float z = (ii + jj) * HalfH;

        return transform.position + new Vector3(x, 0f, z);
    }

    private Vector2Int WorldToIJ(Vector3 world)
    {
        Vector3 local = world - transform.position;

        float invHalfW = Mathf.Approximately(HalfW, 0f) ? 0f : (1f / HalfW);
        float invHalfH = Mathf.Approximately(HalfH, 0f) ? 0f : (1f / HalfH);

        float a = local.z * invHalfH;
        float b = local.x * invHalfW;

        float iFloat = (a + b) * 0.5f;
        float jFloat = (a - b) * 0.5f;

        int i = Mathf.RoundToInt(iFloat) + OriginIJ.x;
        int j = Mathf.RoundToInt(jFloat) + OriginIJ.y;

        return new Vector2Int(i, j);
    }

    private void CreateGrid()
    {
        _grid = new Node[GridSizeI, GridSizeJ];

        for (int i = 0; i < GridSizeI; i++)
        {
            for (int j = 0; j < GridSizeJ; j++)
            {
                Vector3 worldPoint = IJToWorld(i, j) + new Vector3(0, 0, 0.5f);

                bool walkable = true;

                if (_blockedNodeCoords.Contains(new Vector2Int(i, j)))
                {
                    walkable = false;
                }
                else if (UsePhysicsUnwalkable)
                {
                    walkable = !Physics.CheckSphere(worldPoint, PhysicsCheckRadius, UnwalkableMask);
                }

                _grid[i, j] = new Node(walkable, worldPoint, i, j);
            }
        }
    }

    public void SetBlockedByNodeCoords(IEnumerable<Vector2Int> blockedNodeCoords, bool rebuildAll = false)
    {
        _blockedNodeCoords.Clear();

        foreach (var blockedNode in blockedNodeCoords)
        {
            Vector2Int c = blockedNode + new Vector2Int(OriginIJ.x, OriginIJ.y);
            _blockedNodeCoords.Add(c);
        }

        if (rebuildAll) CreateGrid();
        else ApplyBlockedToGrid();
    }

    public void SetBlockedByWorldPositions(IEnumerable<Vector3> blockedWorldPositions, bool rebuildAll = false)
    {
        _blockedNodeCoords.Clear();

        foreach (var pos in blockedWorldPositions)
        {
            Node n = WorldToNode(pos);
            _blockedNodeCoords.Add(new Vector2Int(n.GridX, n.GridY));
        }

        if (rebuildAll) CreateGrid();
        else ApplyBlockedToGrid();
    }

    private void ApplyBlockedToGrid()
    {
        if (_grid == null) return;

        blockedNodeList = _blockedNodeCoords.ToList();

        for (int i = 0; i < GridSizeI; i++)
        {
            for (int j = 0; j < GridSizeJ; j++)
            {
                bool walkable = !_blockedNodeCoords.Contains(new Vector2Int(i, j));
                _grid[i, j].IsWalkable = walkable;
            }
        }
    }

    public Node WorldToNode(Vector3 worldPosition)
    {
        Vector2 ijf = WorldToIJFloat(worldPosition);

        int i0 = Mathf.FloorToInt(ijf.x);
        int j0 = Mathf.FloorToInt(ijf.y);

        Node best = null;
        float bestDistSq = float.PositiveInfinity;

        TryPick(i0, j0);
        TryPick(i0 + 1, j0);
        TryPick(i0, j0 + 1);
        TryPick(i0 + 1, j0 + 1);

        if (best == null)
        {
            int i = Mathf.Clamp(Mathf.RoundToInt(ijf.x), 0, GridSizeI - 1);
            int j = Mathf.Clamp(Mathf.RoundToInt(ijf.y), 0, GridSizeJ - 1);
            return _grid[i, j];
        }

        return best;

        void TryPick(int i, int j)
        {
            i = Mathf.Clamp(i, 0, GridSizeI - 1);
            j = Mathf.Clamp(j, 0, GridSizeJ - 1);

            Node n = _grid[i, j];

            Vector3 a = worldPosition; a.y = 0f;
            Vector3 b = n.WorldPos; b.y = 0f;

            float d2 = (a - b).sqrMagnitude;
            if (d2 < bestDistSq)
            {
                bestDistSq = d2;
                best = n;
            }
        }
    }

    private Vector2 WorldToIJFloat(Vector3 world)
    {
        Vector3 local = world - transform.position;

        float invHalfW = Mathf.Approximately(HalfW, 0f) ? 0f : (1f / HalfW);
        float invHalfH = Mathf.Approximately(HalfH, 0f) ? 0f : (1f / HalfH);

        float a = local.z * invHalfH;
        float b = local.x * invHalfW;

        float iFloat = (a + b) * 0.5f + OriginIJ.x;
        float jFloat = (a - b) * 0.5f + OriginIJ.y;

        return new Vector2(iFloat, jFloat);
    }

    public Vector2Int WorldToNodeCoord(Vector3 worldPosition)
    {
        Node n = WorldToNode(worldPosition);
        return new Vector2Int(n.GridX, n.GridY);
    }

    public List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>(UseDiagonal ? 8 : 4);

        int i = node.GridX;
        int j = node.GridY;

        if (UseDiagonal)
        {
            for (int di = -1; di <= 1; di++)
            {
                for (int dj = -1; dj <= 1; dj++)
                {
                    if (di == 0 && dj == 0) continue;

                    int ni = i + di;
                    int nj = j + dj;

                    if (ni >= 0 && ni < GridSizeI && nj >= 0 && nj < GridSizeJ)
                        neighbors.Add(_grid[ni, nj]);
                }
            }
        }
        else
        {
            TryAdd(i + 1, j);
            TryAdd(i - 1, j);
            TryAdd(i, j + 1);
            TryAdd(i, j - 1);
        }

        return neighbors;

        void TryAdd(int ni, int nj)
        {
            if (ni >= 0 && ni < GridSizeI && nj >= 0 && nj < GridSizeJ)
                neighbors.Add(_grid[ni, nj]);
        }
    }

    public bool IsInBounds(Vector2Int c)
        => c.x >= 0 && c.y >= 0 && c.x < GridSizeI && c.y < GridSizeJ;

    /// <summary>
    /// size=1이면 해당 셀만,
    /// size=2이면 anchor(좌하단) 기준 2×2가 전부 walkable일 때만 true
    /// </summary>
    public bool IsAreaWalkable(Vector2Int anchor, int size)
    {
        size = Mathf.Max(1, size);

        for (int dy = 0; dy < size; dy++)
        {
            for (int dx = 0; dx < size; dx++)
            {
                Vector2Int c = new Vector2Int(anchor.x + dx, anchor.y + dy);
                if (!IsInBounds(c)) return false;
                if (!_grid[c.x, c.y].IsWalkable) return false;
            }
        }

        return true;
    }

    /// <summary>
    /// from -> to 이동 가능? (footprint + 대각 코너끼임 방지)
    /// </summary>
    public bool CanTraverse(Node from, Node to, int size)
    {
        Vector2Int toC = new Vector2Int(to.GridX, to.GridY);
        if (!IsAreaWalkable(toC, size))
            return false;

        int dx = to.GridX - from.GridX;
        int dy = to.GridY - from.GridY;

        bool isDiagonal = (dx != 0 && dy != 0);
        if (UseDiagonal && isDiagonal)
        {
            // 코너 컷 방지: 가로/세로 step도 가능해야 함
            Vector2Int stepA = new Vector2Int(from.GridX + dx, from.GridY);
            Vector2Int stepB = new Vector2Int(from.GridX, from.GridY + dy);

            if (!IsAreaWalkable(stepA, size)) return false;
            if (!IsAreaWalkable(stepB, size)) return false;
        }

        return true;
    }

    // ---- Gizmo ----
    [Header("Gizmos")]
    public bool DrawGizmos = true;
    public float GizmoY = 0.05f;

    private void OnDrawGizmos()
    {
        if (!DrawGizmos) return;
        if (_grid == null) return;

        for (int i = 0; i < GridSizeI; i++)
        {
            for (int j = 0; j < GridSizeJ; j++)
            {
                Node n = _grid[i, j];
                if (n == null) continue;

                Vector3 p = n.WorldPos;
                p.y = transform.position.y + GizmoY;

                Gizmos.color = n.IsWalkable
                    ? new Color(0f, 1f, 0f, 0.25f)
                    : new Color(1f, 0f, 0f, 0.45f);

                DrawDiamond(p, HalfW * 0.95f, HalfH * 0.95f);
            }
        }
    }

    private void DrawDiamond(Vector3 center, float halfW, float halfH)
    {
        Vector3 top = center + new Vector3(0f, 0f, halfH);
        Vector3 right = center + new Vector3(halfW, 0f, 0f);
        Vector3 bottom = center + new Vector3(0f, 0f, -halfH);
        Vector3 left = center + new Vector3(-halfW, 0f, 0f);

        Gizmos.DrawLine(top, right);
        Gizmos.DrawLine(right, bottom);
        Gizmos.DrawLine(bottom, left);
        Gizmos.DrawLine(left, top);
    }
}
