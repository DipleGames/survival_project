using UnityEngine;

public class Node
{
    public bool IsWalkable { get; set; }
    public Vector3 WorldPos { get; }
    public int GridX { get; }
    public int GridY { get; }

    // A*
    public int GCost;
    public int HCost;
    public Node Parent;

    public int FCost => GCost + HCost;

    public Node(bool isWalkable, Vector3 worldPos, int gridX, int gridY)
    {
        IsWalkable = isWalkable;
        WorldPos = worldPos;
        GridX = gridX;
        GridY = gridY;

        ResetPathData();
    }

    public void ResetPathData()
    {
        GCost = int.MaxValue;
        HCost = 0;
        Parent = null;
    }
}
