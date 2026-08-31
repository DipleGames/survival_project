using System.Collections.Generic;
using MineTest;
using UnityEngine;

/// <summary>
/// 채광 가능한 광물 중 우선순위가 가장 높은 광물을 선택한다.
/// 거리 → 현재 HP → 생성 순서 순으로 비교한다.
/// </summary>
public static class MiningTargetSelector
{
    public static MiningNode FindTarget(IReadOnlyList<MiningNode> nodes, SkullController skull, Vector3 searchCenter, float searchRadius)
    {
        if (nodes == null || skull == null)
            return null;

        MiningNode bestNode = null;
        float bestDistance = float.MaxValue;
        float searchRadiusSqr = searchRadius * searchRadius;

        foreach (MiningNode node in nodes)
        {
            if (!IsValidTarget(node, skull, searchCenter, searchRadiusSqr))
                continue;

            Vector3 distanceDelta = node.transform.position - skull.transform.position;
            distanceDelta.y = 0f;

            float distanceSqr = distanceDelta.sqrMagnitude;

            if (IsHigherPriority(node, distanceSqr, bestNode, bestDistance))
            {
                bestNode = node;
                bestDistance = distanceSqr;
            }
        }

        return bestNode;
    }

    private static bool IsValidTarget(
        MiningNode node,
        SkullController skull,
        Vector3 searchCenter,
        float searchRadiusSqr)
    {
        if (node == null || !node.CanBeMinedBy(skull))
            return false;

        Vector3 rangeDelta = node.transform.position - searchCenter;
        rangeDelta.y = 0f;

        return rangeDelta.sqrMagnitude <= searchRadiusSqr;
    }

    private static bool IsHigherPriority(MiningNode candidate, float candidateDistance, MiningNode current, float currentDistance)
    {
        if (current == null)
            return true;

        if (candidateDistance < currentDistance)
            return true;

        if (!Mathf.Approximately(candidateDistance, currentDistance))
            return false;

        if (candidate.CurrentHealth < current.CurrentHealth)
            return true;

        if (candidate.CurrentHealth > current.CurrentHealth)
            return false;

        return candidate.SpawnOrder < current.SpawnOrder;
    }
}