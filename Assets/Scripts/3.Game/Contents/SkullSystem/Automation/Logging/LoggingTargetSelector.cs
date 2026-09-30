using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LoggingTargetSelector
{
    public static GatherableObj FindTarget(IReadOnlyList<GatherableObj> gatherableObjs, SkullController skull, Vector3 searchCenter, float searchRadius)
    {
        if (gatherableObjs == null || skull == null)
            return null;

        GatherableObj bestGatherableObj = null;
        float bestDistance = float.MaxValue;
        float searchRadiusSqr = searchRadius * searchRadius;

        foreach (GatherableObj gatherableObj in gatherableObjs)
        {
            if (!IsValidTarget(gatherableObj, skull, searchCenter, searchRadiusSqr))
                continue;

            Vector3 distanceDelta = gatherableObj.transform.position - skull.transform.position;
            distanceDelta.y = 0f;

            float distanceSqr = distanceDelta.sqrMagnitude;

            if (IsHigherPriority(gatherableObj, distanceSqr, bestGatherableObj, bestDistance))
            {
                bestGatherableObj = gatherableObj;
                bestDistance = distanceSqr;
            }
        }

        return bestGatherableObj;
    }

    private static bool IsValidTarget(GatherableObj gatherObj, SkullController skull, Vector3 searchCenter, float searchRadiusSqr)
    {
        if (gatherObj == null || !gatherObj.CanBeGatheredBy(skull))
            return false;

        Vector3 rangeDelta = gatherObj.transform.position - searchCenter;
        rangeDelta.y = 0f;

        return rangeDelta.sqrMagnitude <= searchRadiusSqr;
    }

    private static bool IsHigherPriority(GatherableObj candidate, float candidateDistance, GatherableObj current, float currentDistance)
    {
        if (current == null)
            return true;

        if (candidateDistance < currentDistance)
            return true;

        if (!Mathf.Approximately(candidateDistance, currentDistance))
            return false;

        return false;
    }
}
