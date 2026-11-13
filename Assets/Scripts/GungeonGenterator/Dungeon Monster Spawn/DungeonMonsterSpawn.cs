using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonMonsterSpawn : MonoBehaviour
{
    enum MonsterType
    {
        Archer,
        Shield,
        SmallBazic,
        BigBazic
    }

    struct MonsterData
    {
        public int Weight;
        public GameObject Prefab;
    }

    [Header("Monster Settings")]
    [SerializeField] private DungeonMonsterSetting NormalMonsterSetting;
    [SerializeField] private DungeonMonsterSetting EliteMonsterSetting;
    [SerializeField] private DungeonMonsterSetting BossMonsterSetting;

    [Header("Monster Prefabs")]
    [SerializeField] private GameObject PrfArcher;
    [SerializeField] private GameObject PrfShield;
    [SerializeField] private GameObject PrfSmallBazic;
    [SerializeField] private GameObject PrfBigBazic;

    //몬스터 스폰 영역을 위한 콜라이더
    private Collider spawnCollider;
    //몬스터가 모두 제거되었는지 확인하기 위한 코루틴
    private Coroutine CompleteRoomCheckCoroutine;


    private Dictionary<MonsterType, MonsterData> monster = new();

    public void Awake()
    {
        spawnCollider = GetComponent<Collider>();
    }

    public void UpdateMonsterSpawner(RoomType roomType)
    {
        transform.position = DungeonManager.Instance.GetRoomPosition(roomType).MonsterSpawnPosition;

        switch (roomType)
        {
            case RoomType.Normal:
                SpawnMonsters(NormalMonsterSetting); break;

            case RoomType.Elite:
                SpawnMonsters(EliteMonsterSetting); break;

            case RoomType.Boss:
                SpawnMonsters(BossMonsterSetting); break;
        }
    }

    private void SpawnMonsters(DungeonMonsterSetting setting)
    {
        if(setting == null) return;

        //스폰할 몬스터 수 결정
        int min = setting.monsterCountMin;
        int max = Mathf.Max(min, setting.monsterCountMax);
        int monsterCount = Random.Range(min, max + 1);

        SetMonsterData(setting);

        for (int i = 0; i < monsterCount; i++)
        {
            GameObject prefab = GetRandomMonsterPrefab();
            if (prefab == null) continue;

            Bounds bounds = spawnCollider.bounds;

            float x = Random.Range(bounds.min.x, bounds.max.x);
            float z = Random.Range(bounds.min.z, bounds.max.z);

            Vector3 spawnPos = new Vector3(x, 0f, z);

            Instantiate(prefab, spawnPos, Quaternion.Euler(90f, 0f, 0f), transform);
        }

        if(CompleteRoomCheckCoroutine != null) StopCoroutine(CompleteRoomCheckCoroutine);
        CompleteRoomCheckCoroutine = StartCoroutine(CheckMonsterClear());
    }

    private void SetMonsterData(DungeonMonsterSetting setting)
    {
        monster.Clear();

        monster.Add(MonsterType.Archer,
            new MonsterData
            {
                Weight = setting.SpawnArcherWeight,
                Prefab = PrfArcher
            });

        monster.Add(MonsterType.Shield,
            new MonsterData
            {
                Weight = setting.SpawnShieldWeight,
                Prefab = PrfShield
            });

        monster.Add(MonsterType.SmallBazic,
            new MonsterData
            {
                Weight = setting.SpawnSmallBazicWeight,
                Prefab = PrfSmallBazic
            });

        monster.Add(MonsterType.BigBazic,
            new MonsterData
            {
                Weight = setting.SpawnBigBazicWeight,
                Prefab = PrfBigBazic
            });
    }

    private GameObject GetRandomMonsterPrefab()
    {
        //가중치 전체 합하기
        int totalWeight = 0;
        foreach (var data in monster.Values) totalWeight += data.Weight;

        if (totalWeight <= 0) return null;

        //랜덤 인덱스 생성
        int Index = Random.Range(0, totalWeight);

        foreach (var data in monster.Values)
        {
            if (Index < data.Weight) return data.Prefab;
            Index -= data.Weight;
        }

        return null;
    }

    private IEnumerator CheckMonsterClear()
    {
        while (transform.childCount > 0) yield return new WaitForSeconds(0.2f);

        CompleteRoomCheckCoroutine = null;
        DungeonManager.Instance.RoomComplete();
    }
}
