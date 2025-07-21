using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Prefab_Ratio
{
    public GameObject dropItem;
    public float ratio;
}
public class SpawnItem : MonoBehaviour
{
    [SerializeField] Collider area;
    [SerializeField] List<Prefab_Ratio> list;
    [SerializeField] Transform parent;
    [SerializeField] int spawnAmount;
    [SerializeField] float minDistance = 2f;

    Vector3 minBounds;
    Vector3 maxBounds;

    protected void Awake()
    {
        minBounds = new Vector3(area.bounds.min.x, 0, area.bounds.min.z);
        maxBounds = new Vector3(area.bounds.max.x, 0, area.bounds.max.z);
    }

    public IEnumerator SpawnItems()
    {
        List<Vector3> positions = GeneratePositions(spawnAmount, minDistance, minBounds, maxBounds);

        foreach (var pos in positions)
        {
            int randomIndex;
            if (list.Count > 1) randomIndex = GetWeightedRandomIndex();
            else randomIndex = 0;

            Instantiate(list[randomIndex].dropItem, pos, Quaternion.Euler(90, 0, 0), parent);

        }
        yield return null;
    }
    public void RemoveFromList(string itemToRemove)
    {
        list.RemoveAll(item => item.dropItem.name == itemToRemove);
    }

    List<Vector3> GeneratePositions(int count, float minDistance, Vector3 minBounds, Vector3 maxBounds)
    {
        List<Vector3> positions = new List<Vector3>();
        int maxAttemptsPerPosition = 100;

        for (int i = 0; i < count; i++)
        {
            bool positionAccepted = false;

            for (int attempt = 0; attempt < maxAttemptsPerPosition; attempt++)
            {
                Vector3 candidate = new Vector3(
                    Random.Range(minBounds.x, maxBounds.x),
                    0,
                    Random.Range(minBounds.z, maxBounds.z)
                );

                bool tooClose = false;
                foreach (var pos in positions)
                {
                    if (Vector3.Distance(candidate, pos) < minDistance)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (!tooClose)
                {
                    positions.Add(candidate);
                    positionAccepted = true;
                    break;
                }
            }

            if (!positionAccepted)
            {
                Debug.LogWarning($"Failed creating {i}th position for {list[0].dropItem.name}. Please try lesser amount");
                continue;
            }
        }

        return positions;
    }

    int GetWeightedRandomIndex()
    {
        float sum = 0f;
        foreach (var r in list)
        {
            sum += r.ratio;
        }

        float rand = Random.Range(0f, sum);
        float accum = 0f;

        for (int i = 0; i < list.Count; i++)
        {
            accum += list[i].ratio;
            if (rand <= accum)
            {
                return i;
            }
        }

        // 안전망
        return list.Count - 1;
    }

}
