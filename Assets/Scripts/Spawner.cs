using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("스폰 대상")]
    [SerializeField] private SpawnWeight[] spawnList;

    [Header("스폰 범위")]
    [SerializeField] private Vector3 maxBounds;
    [SerializeField] private Vector3 minBounds;

    [Header("스폰 주기")]
    [SerializeField] private float spawnInterval = 1.0f;
    private float timer;



    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > spawnInterval)
        {
            timer = 0f;
            SpawnRandomObject();
        }
    }

    private void SpawnRandomObject()
    {
        if (spawnList == null) return;

        float totalWeight = 0f;
        foreach (var item in spawnList)
            totalWeight += item.weight;

        float randomValue = Random.Range(0f, totalWeight);
        Debug.Log("randomValue: " + randomValue);
        GameObject selectedPrefab = null;
        foreach (var item in spawnList)
        {
            randomValue -= item.weight;
            if (randomValue <= 0)
            {
                selectedPrefab = item.prefab;
                break;
            }
        }


        if (selectedPrefab != null)
        {
            float x = Random.Range(minBounds.x, maxBounds.x);
            float y = Random.Range(minBounds.y, maxBounds.y);
            float z = Random.Range(minBounds.z, maxBounds.z);
            Vector3 spawnPos = new Vector3(x, y, z);

            Instantiate(selectedPrefab, spawnPos, Quaternion.identity);
        }
    }
}
