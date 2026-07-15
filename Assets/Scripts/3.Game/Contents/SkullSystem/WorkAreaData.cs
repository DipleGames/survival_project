using UnityEngine;

public class WorkAreaData
{
    public Vector3Int CellPosition { get; private set; }
    public WorkType WorkType { get; private set; }

    public bool IsReserved { get; private set; }
    public SkullController AssignedSkull { get; private set; }

    public WorkAreaData(Vector3Int cellPosition, WorkType workType)
    {
        CellPosition = cellPosition;
        WorkType = workType;

        IsReserved = false;
        AssignedSkull = null;
    }

    public bool Reserve(SkullController skull)
    {
        if (IsReserved)
            return false;

        IsReserved = true;
        AssignedSkull = skull;

        return true;
    }

    public void Release()
    {
        IsReserved = false;
        AssignedSkull = null;
    }
}