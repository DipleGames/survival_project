using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseRepair : MonoBehaviour, IDamageable
{
    [SerializeField] private HouseManager houseManager;
    [SerializeField] private HouseRecipe recipe;

    float LostRatio()
    {
        int max = Mathf.Max(1, houseManager.MaxDurability);
        int lost = Mathf.Clamp(houseManager.MaxDurability - houseManager.CurrentDurability, 0, max);
        return (float)lost / max;
    }

    public Dictionary<MaterialType, int> CalRepairCost()
    {
        var costs = new Dictionary<MaterialType, int>();
        if (houseManager.Level <= 0) return costs;
        var prev = recipe.Get(houseManager.Level - 1);
        if (prev == null) return costs;
        float ratio = LostRatio();
        if (ratio <= 0f) return costs;
        foreach (var c in prev.costs)
        {
            if (c.amount <= 0) continue;
            int need = Mathf.CeilToInt(c.amount * ratio);
            if (need == 0) need = 1;
            costs[c.type] = (costs.TryGetValue(c.type, out var prevAmt) ? prevAmt : 0) + need;
        }
        return costs;
    }

    public bool CanRepair()
    {
        var cost = CalRepairCost();
        if (cost.Count == 0) return false;
        foreach (var c in cost)
        {
            if (!GameManager.Instance.idByMaterialType.TryGetValue(c.Key, out int id)) return false;
            if (!GameManager.Instance.haveItems.TryGetValue(id, out int have) || have < c.Value) return false;
        }
        return true;
    }

    public bool TryRepair()
    {
        if (!CanRepair()) return false;
        foreach (var material in CalRepairCost())
        {
            int id = GameManager.Instance.idByMaterialType[material.Key];
            GameManager.Instance.haveItems[id] -= material.Value;
            if (GameManager.Instance.haveItems[id] < 0) GameManager.Instance.haveItems[id] = 0;
        }
        houseManager.RepairFull();
        return true;
    }
    
    public void Attacked(float damage, GameObject hitObject)
    {
        if(houseManager.CurrentDurability <= 0) return;
        houseManager.Damage(Mathf.CeilToInt(damage));
        Debug.Log($"현재 집 내구도: {houseManager.CurrentDurability}");
        if (houseManager.CurrentDurability <= 0)
        {
            Debug.Log("집이 파괴되었습니다.");
            // gameObject.SetActive(false); // 필요 시
        }
    }

    public void RendDamageUI(float damage, Vector3 rendPos, bool canCri, bool isCri)
    {
        
    }
}
