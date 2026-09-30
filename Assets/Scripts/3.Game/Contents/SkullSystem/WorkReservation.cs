using UnityEngine;

public sealed class WorkReservation
{
    public Object ReservedBy { get; private set; }
    public bool IsReserved => ReservedBy != null;

    public bool CanReserve(Object worker)
    {
        return worker != null && (!IsReserved || ReservedBy == worker);
    }

    public bool TryReserve(Object worker)
    {
        if (!CanReserve(worker))
            return false;

        ReservedBy = worker;
        return true;
    }

    public bool IsReservedBy(Object worker)
    {
        return worker != null && ReservedBy == worker;
    }

    public void Release(Object worker)
    {
        if (!IsReservedBy(worker))
            return;

        ReservedBy = null;
    }

    // 대상 초기화나 폐기 시 사용.
    public void Clear()
    {
        ReservedBy = null;
    }
}