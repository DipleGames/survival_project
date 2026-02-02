using System.Collections.Generic;
using UnityEngine;

public enum PathResult
{
    Success,

    Blocked,   // 목표 못감 + 막고 있는 셀(울타리) 찾음
}
public class AStarPathfinder : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    public PathControllTower pathControllTower;
    public PathResult pathResult;

    public bool TryFindPath(Vector3 startWorld, Vector3 targetWorld, out Vector3[] waypoints)
    {
        waypoints = null;

        Node startNode = gridManager.WorldToNode(startWorld);
        Node targetNode = gridManager.WorldToNode(targetWorld);

        // if (!startNode.IsWalkable || !targetNode.IsWalkable)
        // {
        //     Debug.Log("실패");
        //     return false;
        // }

        if (!targetNode.IsWalkable) // 도착지점만 신경
        {
            Debug.Log("실패");
            return false;
        }

        // OpenSet: 간단히 List로 (성능 필요하면 Heap/우선순위큐로 교체)
        List<Node> openSet = new List<Node>(gridManager.MaxSize);
        HashSet<Node> closedSet = new HashSet<Node>();

        // 비용 초기화(간단 처리)
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
                return waypoints != null && waypoints.Length > 0;
            }

            foreach (Node neighbor in gridManager.GetNeighbors(current))
            {
                if (!neighbor.IsWalkable || closedSet.Contains(neighbor))
                    continue;

                int newCost = current.GCost + GetDistance(current, neighbor);

                // open에 없으면 무조건 갱신
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
            if (current == null) return null; // 안전장치
        }

        path.Reverse();

        List<Vector3> waypoints = new List<Vector3>(path.Count);
        for (int i = 0; i < path.Count; i++)
            waypoints.Add(path[i].WorldPos);

        return waypoints.ToArray();
    }


    private int GetDistance(Node a, Node b)
    {
        int dstX = Mathf.Abs(a.GridX - b.GridX);
        int dstY = Mathf.Abs(a.GridY - b.GridY);

        // 대각선 14, 직선 10 (전형적인 A*)
        if (dstX > dstY) return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }
}
