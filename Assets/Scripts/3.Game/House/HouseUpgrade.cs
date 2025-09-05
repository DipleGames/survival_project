using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseUpgrade : MonoBehaviour
{
    [SerializeField] private HouseManager houseManager;
    [SerializeField] private HouseRecipe recipe;

    Dictionary<MaterialType,int> CalUpgradeCost()
    {
        var costs = new Dictionary<MaterialType,int>();
        var r = recipe.Get(houseManager.Level);
        if (r == null) return costs;
        foreach (var c in r.costs)
        {
            if (c.amount <= 0) continue;
            costs[c.type] = (costs.TryGetValue(c.type, out var prev) ? prev : 0) + c.amount;
        }
        return costs;
    }

    public bool CanUpgrade()
    {
        if (!houseManager.CanIncreaseLevel()) return false;
        var cost = CalUpgradeCost();
        foreach (var c in cost)
        {
            // 키 존재 유무, 개수가 있는지 체크
            if (!GameManager.Instance.idByMaterialType.TryGetValue(c.Key, out int id)) return false;
            if (!GameManager.Instance.haveItems.TryGetValue(id, out int have) || have < c.Value) return false;
        }
        return true;
    }

    public bool TryUpgrade()
    {
        if (!CanUpgrade()) return false;
        foreach (var material in CalUpgradeCost())
        {
            int id = GameManager.Instance.idByMaterialType[material.Key];
            GameManager.Instance.haveItems[id] -= material.Value;
            if (GameManager.Instance.haveItems[id] < 0) GameManager.Instance.haveItems[id] = 0;
        }
        houseManager.IncreaseLevel();
        return true;
    }
}
