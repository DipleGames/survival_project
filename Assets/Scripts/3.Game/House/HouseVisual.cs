using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseVisual : MonoBehaviour
{
    [SerializeField] private HouseManager houseManager;
    [SerializeField] private HouseRecipe recipe;
    [SerializeField] private Transform houseTransform;

    private GameObject currentHouse;

    void Awake()
    {
        if (!houseTransform) houseTransform = transform;
        houseManager.OnLevelChanged += SpawnHouse;
    }
    
    void Start()
    {
        SpawnHouse(houseManager.Level);
    }

    void OnDestroy()
    {
        houseManager.OnLevelChanged -= SpawnHouse;
    }

    void SpawnHouse(int level)
    {
        if (currentHouse)
        {
            Destroy(currentHouse);
            currentHouse = null;
        }
        var prefab = recipe.Get(level).house;
        if (!prefab)
        {
            Debug.LogWarning($"{level} 레벨 외형 프리팹 없음");
            return;
        }
        currentHouse = Instantiate(prefab, houseTransform.position, houseTransform.rotation, houseTransform);
    }
}
