// InventoryDisplayer.cs
using System.Collections.Generic;
using UnityEngine;

public class InventoryDisplayer : MonoBehaviour
{
    [Header("Target Item Type")]
    public ItemType targetTypes;
    public bool useAndLogic;

    [Header("Caches")]
    [SerializeField] Transform slotsParent;
    [Header("Prefabs")]
    [SerializeField] GameObject slotPrefab;
    [SerializeField] GameObject itemUIPrefab;

    readonly List<GameObject> active = new();
    readonly Stack<GameObject> pool = new();
    void OnEnable() => Refresh(targetTypes);

    public void Refresh(ItemType targetTypes)
    {
        if (slotsParent == null || slotPrefab == null) return;

        GameManager gm = GameManager.Instance;
        // 타입에 부합하는 아이템만 추림
        var filtered = new List<Item>();
        foreach (var curItem in gm.haveItems)
        {
            if (gm.Items.TryGetValue(curItem.Key, out var def) && def.CompareType(targetTypes, useAndLogic) && curItem.Value > 0)
                filtered.Add(gm.Items[curItem.Key]);
        }

        EnsureSlotCount(filtered.Count);

        // InventoryDisplayer.cs (슬롯 업데이트 부분)
        for (int i = 0; i < filtered.Count; i++)
        {
            if (!active[i].TryGetComponent<InventorySlot>(out var slot))
                continue;

            if (active[i].GetComponentInChildren<ItemUI>() == null)
            {
                slot.itemUI = Instantiate(itemUIPrefab, active[i].transform, false).GetComponent<ItemUI>();
            }

            slot.Init(filtered[i]);
            slot.gameObject.SetActive(true);
        }


        for (int i = filtered.Count; i < active.Count; i++)
            active[i].SetActive(false);
    }

    void EnsureSlotCount(int needed)
    {
        // 부족하면 풀에서 꺼내거나 새로 생성
        while (active.Count < needed)
        {
            GameObject slot = pool.Count > 0 ? pool.Pop() : Instantiate(slotPrefab, slotsParent);
            active.Add(slot);
        }
        // 너무 많으면 뒤에서 빼서 풀에 반납 (비활성화)
        while (active.Count > needed)
        {
            var last = active[active.Count - 1];
            last.SetActive(false);
            pool.Push(last);
            active.RemoveAt(active.Count - 1);
        }
    }
}
