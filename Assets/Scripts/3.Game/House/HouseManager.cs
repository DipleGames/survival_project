using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseManager : MonoBehaviour
{
    [SerializeField] private int houseLevel = 0;
    [SerializeField] private HouseRecipe recipe;
    [SerializeField] private int currentDurability;
    public int Level => houseLevel;
    public int CurrentDurability => currentDurability;
    public int MaxDurability
    {
        get
        {
            var r = recipe.Get(houseLevel);
            int v = r.houseDurability;
            return (v > 0) ? v : 30;
        }
    }

    public event System.Action<int> OnLevelChanged;           // 레벨 이벤트
    public event System.Action<int,int> OnDurabilityChanged;  // 내구도 변경 이벤트

    void Start()
    {
        currentDurability = MaxDurability;
        OnLevelChanged?.Invoke(houseLevel);
        OnDurabilityChanged?.Invoke(currentDurability, MaxDurability);
    }

    public bool CanIncreaseLevel()
    {
        return recipe && (houseLevel < recipe.LevelCount - 1);
    }

    public void IncreaseLevel()
    {
        houseLevel++;
        currentDurability = MaxDurability;
        OnLevelChanged?.Invoke(houseLevel);
        OnDurabilityChanged?.Invoke(currentDurability, MaxDurability);
    }

    public void Damage(int amount)
    {
        if (amount <= 0 || currentDurability <= 0) return;
        currentDurability = Mathf.Max(0, currentDurability - amount);
        Debug.Log($"현재 집 내구도: {currentDurability}");
        OnDurabilityChanged?.Invoke(currentDurability, MaxDurability);
    }

    public void RepairFull()
    {
        currentDurability = MaxDurability;
        OnDurabilityChanged?.Invoke(currentDurability, MaxDurability);
    }
}
