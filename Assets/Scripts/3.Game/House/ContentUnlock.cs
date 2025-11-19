using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContentUnlock : MonoBehaviour
{
    [SerializeField] private HouseManager houseManager;
    [SerializeField] private int minLevel = 1;   // 해금 레벨
    [SerializeField] private GameObject[] targets;

    void OnEnable()
    {
        if (houseManager)
        {
            houseManager.OnLevelChanged += Apply;
            Apply(houseManager.Level); // 현재 레벨로 즉시 초기화
        }
    }

    void OnDisable()
    {
        if (houseManager)
            houseManager.OnLevelChanged -= Apply;
    }

    void Apply(int level)
    {
        bool active = level >= minLevel;
        foreach (var go in targets)
        {
            if (go) go.SetActive(active);
        }
    }
}
