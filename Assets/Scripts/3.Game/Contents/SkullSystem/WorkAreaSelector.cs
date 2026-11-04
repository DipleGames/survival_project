using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 등록된 WorkArea 중 해골이 사용할 작업 영역을 선택한다.
/// 작업 종류, 예약 상태, 거리 등의 조건을 기준으로 작업 대상을 선정한다.
/// </summary>
public static class WorkAreaSelector
{
    public static WorkAreaData FindNearestAvailableArea(WorkType workType, Vector3 skullPosition)
    {
        WorkAreaData nearestArea = null;
        float nearestDistance = float.MaxValue;

        foreach (WorkAreaData area in WorkAreaManager.Instance.GetWorkAreas(workType))
        {
            if (area.IsReserved)
                continue;

            Vector3 areaPosition =
                WorkAreaManager.Instance.GetWorldPosition(
                    area.CellPosition);

            float distance = Vector3.SqrMagnitude(
                areaPosition - skullPosition);

            if (distance >= nearestDistance)
                continue;

            nearestDistance = distance;
            nearestArea = area;
        }

        return nearestArea;
    }
}