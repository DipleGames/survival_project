using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonMonsterSpawn : MonoBehaviour
{
    //몬스터 스폰 영역을 위한 콜라이더
    private Collider spawnCollider;
    //몬스터가 모두 제거되었는지 확인하기 위한 코루틴
    private Coroutine CompleteRoomCheckCoroutine;

    public void Awake() { spawnCollider = GetComponent<Collider>(); }

    public void UpdateMonsterSpawner(RoomType roomType)
    {
        RoomTypeSettings setting = DungeonManager.Instance.GetDungeonSetting(roomType);

        if (setting == null) return;

        //몬스터 스포너 위치 갱신
        transform.position = setting.MonsterSpawnerPosition;

        //몬스터 스폰을 사용하지 않는 방은 제외
        if (!setting.UseMonsterSpawn) return;

        SpawnMonsters(setting);
    }

    private void SpawnMonsters(RoomTypeSettings setting)
    {
        //스폰할 몬스터 수 결정
        int min = setting.MonsterCountMin;
        int max = Mathf.Max(min, setting.MonsterCountMax);
        int monsterCount = Random.Range(min, max + 1);

        //몬스터 스폰 영역
        Bounds bounds = spawnCollider.bounds;

        for (int i = 0; i < monsterCount; i++)
        {
            GameObject prefab = GetRandomMonsterPrefab(setting);
            if (prefab == null) continue;

            float x = Random.Range(bounds.min.x, bounds.max.x);
            float z = Random.Range(bounds.min.z, bounds.max.z);

            Vector3 spawnPos = new Vector3(x, 0f, z);

            GameObject spawned = Instantiate(
                prefab,
                spawnPos,
                Quaternion.Euler(90f, 0f, 0f),
                transform
            );
        }

        if (CompleteRoomCheckCoroutine != null) StopCoroutine(CompleteRoomCheckCoroutine);
        //몬스터가 모두 제거되었는지 확인하는 코루틴 시작(모두 제거되면 방 클리어 처리)
        CompleteRoomCheckCoroutine = StartCoroutine(CheckMonsterClear());
    }

    private GameObject GetRandomMonsterPrefab(RoomTypeSettings setting)
    {
        // 전체 가중치 합
        int totalWeight = 0;
        foreach (var data in setting.MonsterSpawnList)
        {
            if (data == null || data.MonsterPrefab == null) continue;
            totalWeight += Mathf.Max(0, data.SpawnWeight);
        }

        if (totalWeight <= 0) return null;

        //전체 가중치 범위 내에서 최댓값을 제외한 랜덤한 숫자 선택
        int index = Random.Range(0, totalWeight);

        foreach (var data in setting.MonsterSpawnList)
        {
            //버그 방지: 몬스터 프리팹이 없는 경우 스킵
            if (data == null || data.MonsterPrefab == null) continue;
            //음수 가중치 방지
            int weight = Mathf.Max(0, data.SpawnWeight);
            //랜덤 숫자가 현재 몬스터의 가중치 범위에 속하면 선택
            if (index < weight) return data.MonsterPrefab;
            //현재 몬스터의 가중치만큼 차감하여 다음 몬스터의 범위 검사
            index -= weight;
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