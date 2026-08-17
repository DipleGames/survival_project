using UnityEngine;


/// <summary>
/// 해골이 작업할 수 있는 작업 위치의 데이터를 나타낸다.
/// 작업 종류, 위치, 예약 상태 등의 정보를 관리한다.
/// </summary>
public class WorkAreaData
{
    public Vector3Int CellPosition { get; }
    public WorkType WorkType { get; }

    public SkullController ReservedBy { get; private set; }

    public bool IsReserved => ReservedBy != null;


    public WorkAreaData(Vector3Int cellPosition, WorkType workType)
    {
        CellPosition = cellPosition;
        WorkType = workType;
    }


    public bool TryReserve(SkullController skull)
    {
        if (IsReserved)
            return false;

        ReservedBy = skull;

        return true;
    }


    public void Release(SkullController skull)
    {
        if (ReservedBy != skull)
            return;

        ReservedBy = null;
    }
}