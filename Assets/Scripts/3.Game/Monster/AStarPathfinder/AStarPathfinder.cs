using System.Collections.Generic;
using UnityEngine;

public enum PathResult
{
    Success,
    Blocked,
}

public class AStarPathfinder : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    public PathControllTower pathControllTower;
    public PathResult pathResult;

    // 기존 시그니처 유지
    public bool TryFindPath(Vector3 startWorld, Vector3 targetWorld, out Vector3[] waypoints)
        => TryFindPath(startWorld, targetWorld, 1, out waypoints);

    // 몬스터 크기 적용 버전
    public bool TryFindPath(Vector3 startWorld, Vector3 targetWorld, int agentSize, out Vector3[] waypoints)
    {
        waypoints = null;

        Node startNode = gridManager.WorldToNode(startWorld);
        Node targetNode = gridManager.WorldToNode(targetWorld);

        if (!gridManager.IsAreaWalkable(new Vector2Int(targetNode.GridX, targetNode.GridY), agentSize))
        {
            pathResult = PathResult.Blocked;
            return false;
        }

        // Open/Closed
        List<Node> openSet = new List<Node>(gridManager.MaxSize);
        HashSet<Node> closedSet = new HashSet<Node>();

        // (간단) 시작 비용 초기화
        startNode.GCost = 0;
        startNode.HCost = GetDistance(startNode, targetNode);
        startNode.Parent = null;

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node current = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                Node n = openSet[i];
                if (n.FCost < current.FCost || (n.FCost == current.FCost && n.HCost < current.HCost))
                    current = n;
            }

            openSet.Remove(current);
            closedSet.Add(current);

            if (current == targetNode)
            {
                waypoints = RetraceWaypoints(startNode, targetNode);
                pathResult = PathResult.Success;
                return waypoints != null && waypoints.Length > 0;
            }

            foreach (Node neighbor in gridManager.GetNeighbors(current))
            {
                if (closedSet.Contains(neighbor))
                    continue;

                // size + 대각 코너끼임 방지 포함
                if (!gridManager.CanTraverse(current, neighbor, agentSize))
                    continue;

                int newCost = current.GCost + GetDistance(current, neighbor);

                if (!openSet.Contains(neighbor) || newCost < neighbor.GCost)
                {
                    neighbor.GCost = newCost;
                    neighbor.HCost = GetDistance(neighbor, targetNode);
                    neighbor.Parent = current;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        pathResult = PathResult.Blocked;
        return false;
    }

    private Vector3[] RetraceWaypoints(Node start, Node end)
    {
        List<Node> path = new List<Node>();
        Node current = end;

        while (current != start)
        {
            path.Add(current);
            current = current.Parent;
            if (current == null) return null;
        }

        path.Reverse();

        Vector3[] waypoints = new Vector3[path.Count];
        for (int i = 0; i < path.Count; i++)
            waypoints[i] = path[i].WorldPos;

        return waypoints;
    }

    private int GetDistance(Node a, Node b)
    {
        int dstX = Mathf.Abs(a.GridX - b.GridX);
        int dstY = Mathf.Abs(a.GridY - b.GridY);

        if (dstX > dstY) return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }
}
