using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{
    [SerializeField] private GameObject[] monsters;

    public List<MonsterAI> monsterAIs = new List<MonsterAI>(); 
    public void StartMonsterWaveRoutine()
    {
        StartCoroutine(MonsterWaveRoutine());
    }

    IEnumerator MonsterWaveRoutine()
    {
        while(true)
        {
            GameObject monsterInstance = Instantiate(monsters[0],transform.position, Quaternion.Euler(90f,0f,0f));
            monsterAIs.Add(monsterInstance.GetComponent<MonsterAI>());
            yield return CoroutineCaching.WaitForSeconds(1.5f);
        }
    }
}
