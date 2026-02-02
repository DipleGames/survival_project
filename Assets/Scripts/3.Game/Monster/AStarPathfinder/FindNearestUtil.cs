using System.Collections.Generic;
using UnityEngine;

public static class FindNearestUtil
{
    public static T FindNearest<T>(Transform self, IList<T> targets) where T : Component
    {
        if (self == null || targets == null || targets.Count == 0) return null;

        Vector3 selfPos = self.position;

        T best = null;
        float bestSqr = float.PositiveInfinity;

        for (int i = 0; i < targets.Count; i++)
        {
            T t = targets[i];
            if (t == null) continue;

            float sqr = (t.transform.position - selfPos).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                best = t;
            }
        }

        return best;
    }
}
