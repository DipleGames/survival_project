using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Isometric Tile (World Units)")]
    public float TileWidth  = 2f;   // 가로 2
    public float TileHeight = 1f;   // 세로 1 (XZ 평면에서 Z축 방향 스케일)

    [Header("Grid Size (in tiles)")]
    public int GridSizeI = 50;      // i 방향 타일 개수
    public int GridSizeJ = 50;      // j 방향 타일 개수

    [Header("Origin (tile index to world mapping)")]
    public Vector2Int OriginIJ = new Vector2Int(0, 0); // (0,0) 타일이 놓일 기준

    [Header("Optional: Physics-based unwalkable check")]
    public bool UsePhysicsUnwalkable = false;
    public LayerMask UnwalkableMask;
    public float PhysicsCheckRadius = 0.2f; // 타일 중심에서 체크 반경

    [Header("Neighbors")]
    public bool UseDiagonal = true;  // 8방 허용 여부

    private Node[,] _grid;

    // 막힌 노드 좌표(i,j)
    private readonly HashSet<Vector2Int> _blockedNodeCoords = new HashSet<Vector2Int>();
    public List<Vector2Int> blockedNodeList = new List<Vector2Int>();

    public int MaxSize => GridSizeI * GridSizeJ;

    private float HalfW => TileWidth * 0.5f;
    private float HalfH => TileHeight * 0.5f;

    private void Awake()
    {
        CreateGrid();
    }

    /// <summary>
    /// (i,j) -> 월드(X,Z) 이소메트릭 변환
    /// worldX = (i - j) * (w/2)
    /// worldZ = (i + j) * (h/2)
    /// </summary>
    private Vector3 IJToWorld(int i, int j)
    {
        int ii = i - OriginIJ.x;
        int jj = j - OriginIJ.y;

        float x = (ii - jj) * HalfW;
        float z = (ii + jj) * HalfH;

        // transform.position을 "그리드 원점"으로 사용
        return transform.position + new Vector3(x, 0f, z);
    }

    /// <summary>
    /// 월드 -> (i,j) 역변환
    /// i = (z/(h/2) + x/(w/2)) / 2
    /// j = (z/(h/2) - x/(w/2)) / 2
    /// </summary>
    private Vector2Int WorldToIJ(Vector3 world)
    {
        Vector3 local = world - transform.position;

        // 분모 0 방지
        float invHalfW = Mathf.Approximately(HalfW, 0f) ? 0f : (1f / HalfW);
        float invHalfH = Mathf.Approximately(HalfH, 0f) ? 0f : (1f / HalfH);

        float a = local.z * invHalfH; // = i + j
        float b = local.x * invHalfW; // = i - j

        float iFloat = (a + b) * 0.5f;
        float jFloat = (a - b) * 0.5f;

        // “가장 가까운 타일”로 매핑
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
                Vector3 worldPoint = IJToWorld(i, j) + new Vector3(0,0,0.5f);

                bool walkable = true;

                // 1) 좌표 기반 블록 우선
                if (_blockedNodeCoords.Contains(new Vector2Int(i, j)))
                {
                    walkable = false;
                }
                else
                {
                    // 2) 옵션: Physics 기반 체크
                    if (UsePhysicsUnwalkable)
                    {
                        walkable = !Physics.CheckSphere(worldPoint, PhysicsCheckRadius, UnwalkableMask);
                    }
                }

                _grid[i, j] = new Node(walkable, worldPoint, i, j);
            }
        }
    }

    /// <summary>
    /// 노드 좌표(i,j)를 직접 넘겨 막힘을 설정한다.
    /// </summary>
    public void SetBlockedByNodeCoords(IEnumerable<Vector2Int> blockedNodeCoords, bool rebuildAll = false)
    {
        _blockedNodeCoords.Clear();

        foreach (var blockedNode in blockedNodeCoords)
        {
            Vector2Int c = blockedNode + new Vector2Int(OriginIJ.x, OriginIJ.y);
            _blockedNodeCoords.Add(c);
        }

        if (rebuildAll)
            CreateGrid();
        else
            ApplyBlockedToGrid();
    }

    /// <summary>
    /// 월드 좌표 목록을 받아 해당 노드들을 "막힘"으로 설정한다.
    /// </summary>
    public void SetBlockedByWorldPositions(IEnumerable<Vector3> blockedWorldPositions, bool rebuildAll = false)
    {
        _blockedNodeCoords.Clear();

        foreach (var pos in blockedWorldPositions)
        {
            Node n = WorldToNode(pos);
            _blockedNodeCoords.Add(new Vector2Int(n.GridX, n.GridY));
        }

        if (rebuildAll)
            CreateGrid();
        else
            ApplyBlockedToGrid();
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

                _grid[i, j] = new Node(walkable, _grid[i, j].WorldPos, i, j);
            }
        }
    }

    // public Node WorldToNode(Vector3 worldPosition)
    // {
    //     Vector2Int ij = WorldToIJ(worldPosition);

    //     int i = Mathf.Clamp(ij.x, 0, GridSizeI - 1);
    //     int j = Mathf.Clamp(ij.y, 0, GridSizeJ - 1);
    //     Debug.Log($"{i}, {j}");
    //     return _grid[i, j];
    // }

    public Node WorldToNode(Vector3 worldPosition)
    {
        // 1) 월드 -> (iFloat, jFloat) "실수" 좌표로 변환 (Round/Clamp X)
        Vector2 ijf = WorldToIJFloat(worldPosition);

        // 2) 주변 4칸 후보 구성 (floor 기준)
        int i0 = Mathf.FloorToInt(ijf.x);
        int j0 = Mathf.FloorToInt(ijf.y);

        // 후보 4개
        Node best = null;
        float bestDistSq = float.PositiveInfinity;

        TryPick(i0,     j0);
        TryPick(i0 + 1, j0);
        TryPick(i0,     j0 + 1);
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

            // xz기준
            Vector3 a = worldPosition; a.y = 0f;
            Vector3 b = n.WorldPos;    b.y = 0f;

            float d2 = (a - b).sqrMagnitude;
            if (d2 < bestDistSq)
            {
                bestDistSq = d2;
                best = n;
            }
        }
    }

    // WorldToIJ()랑 동일한 역변환인데, RoundToInt 하지 않고 실수 좌표를 반환
    private Vector2 WorldToIJFloat(Vector3 world)
    {
        Vector3 local = world - transform.position;

        float invHalfW = Mathf.Approximately(HalfW, 0f) ? 0f : (1f / HalfW);
        float invHalfH = Mathf.Approximately(HalfH, 0f) ? 0f : (1f / HalfH);

        float a = local.z * invHalfH; // = i + j
        float b = local.x * invHalfW; // = i - j

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
            // 4방
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

    

    // ---- (선택) 디버그 Gizmo: 다이아 격자 표시 ----
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
        Vector3 top    = center + new Vector3(0f, 0f,  halfH);
        Vector3 right  = center + new Vector3( halfW, 0f, 0f);
        Vector3 bottom = center + new Vector3(0f, 0f, -halfH);
        Vector3 left   = center + new Vector3(-halfW, 0f, 0f);

        Gizmos.DrawLine(top, right);
        Gizmos.DrawLine(right, bottom);
        Gizmos.DrawLine(bottom, left);
        Gizmos.DrawLine(left, top);
    }
}
