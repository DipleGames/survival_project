using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;

public class MonsterSpawn : MonoBehaviour
{
    [SerializeField] Collider[] spawnPoses;
    [SerializeField] GameObject[] normalMonsterPrefab;
    [SerializeField] GameObject[] bossMonsterPrefab;
    [SerializeField] Transform storageParent;
    [SerializeField] Transform bosssParent;
    [SerializeField] GameObject spawnImage;
    [SerializeField] GameObject bossSpawnImage;
    [SerializeField] int poolCount;
    [SerializeField] int spawnRange;
    [SerializeField] float spawnDelay;
    [SerializeField] float spawnDelayDecrease;
    [SerializeField] int spawnAmount;

    private IObjectPool<Monster> pool;

    float[] weightValue;
    float totalWeight = 0;

    GameManager gameManager;
    GamesceneManager gamesceneManager;

    Coroutine currentCoroutine;

    float currentDelayTime;
    int currentSpawnAmount;

    Color leaderColor = new Color(0, 1, 0.43f, 1);


    private void Awake()
    {
        pool = new ObjectPool<Monster>(CreateMonster, OnGetMonster, OnReleaseMonster, OnDestroyMonster, maxSize: poolCount);
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        gamesceneManager = GamesceneManager.Instance;

        weightValue = new float[] { 100, 0, 0, 100, 100};

        for (int i = 0; i < weightValue.Length; i++)
        {
            totalWeight += weightValue[i];
        }

        currentDelayTime = spawnDelay;
        currentSpawnAmount = spawnAmount - 1;

        StartCoroutine(UpdateSpawn());
    }

    IEnumerator UpdateSpawn()
    {
        while (true)
        {
            yield return CoroutineCaching.WaitWhile(() => !gamesceneManager.isNight);

            if (gameManager.round % 3 == 1 && gameManager.round != 1)
                currentSpawnAmount++;

            currentDelayTime = spawnDelay;
            currentCoroutine = StartCoroutine(RendSpawnImage());
            Coroutine delayCoroutine = StartCoroutine(DecreaseSpawnDelay(10));

            yield return CoroutineCaching.WaitWhile(() => gamesceneManager.isNight);

            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
                currentCoroutine = null;
            }

            if(delayCoroutine != null)
            {
                StopCoroutine(delayCoroutine);
            }

            pool.Clear();

            for (int i = 0; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
    }

    Collider spawnTransform;

    IEnumerator RendSpawnImage()
    {
        while (gamesceneManager.currentGameTime >= currentDelayTime)
        {
            Vector3 pos = SpawnPosition();
            GameObject spawnMark = Instantiate(spawnImage, pos, spawnImage.transform.rotation, storageParent);
            Destroy(spawnMark, 1f);
            //StartCoroutine(SpawnMonster(true, pos, leaderColor));
            StartCoroutine(SpawnMonster(false, pos, Color.white));

            SpawnSubordinateMonster(pos, currentSpawnAmount);

            yield return CoroutineCaching.WaitForSeconds(currentDelayTime);
        }
    }

    IEnumerator DecreaseSpawnDelay(float time)
    {
        while (true)
        {
            yield return CoroutineCaching.WaitForSeconds(time);

            currentDelayTime -= spawnDelayDecrease + ((gameManager.round / 2) * 0.05f);
        }
    }

    Vector3 SpawnPosition()
    {
        spawnTransform = spawnPoses[Random.Range(0, spawnPoses.Length)];

        float randX = spawnTransform.bounds.size.x;
        float randZ = spawnTransform.bounds.size.z;

        randX = Random.Range(-(randX / 2), (randX / 2));
        randZ = Random.Range(-(randZ / 2), (randZ / 2));
       
        Vector3 spawnPos = new Vector3(randX, 0, randZ) + spawnTransform.bounds.center;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(spawnPos, out hit, 500, NavMesh.AllAreas))
        {
            spawnPos = hit.position;
        }

        return spawnPos;
    }

    IEnumerator SpawnMonster(bool isLeader, Vector3 pos, Color color)
    {
        yield return CoroutineCaching.WaitForSeconds(1);

        /*int rand = RandomMonster();
        bool isInPool = false;

        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.activeSelf)
                continue;

            if (transform.GetChild(i).GetComponent<Monster>()?.monsterNum == rand)
            {
                GetPoolMonster(isLeader, pos, color, transform.GetChild(i).GetComponent<Monster>());
                isInPool = true;
                Debug.Log("1");
                break;
            }
        }

        if (!isInPool)
        {
            Debug.Log("2");
            GetNewMonster(isLeader, pos, color, rand);
        }*/

        Monster monster = pool.Get();

        monster.stat = isLeader ? MonsterInfo.Instance.monsterInfos[monster.monsterNum + normalMonsterPrefab.Length] : MonsterInfo.Instance.monsterInfos[monster.monsterNum];
        monster.ChangeOutline(color);
        monster.InitMonsterSetting(isLeader);

        NavMeshHit hit;

        if (NavMesh.SamplePosition(pos, out hit, 500, NavMesh.AllAreas))
        {
            monster.transform.position = hit.position;
        }

        if (monster.GetComponent<NavMeshAgent>())
        {
            monster.GetComponent<NavMeshAgent>().enabled = true;

            if (!monster.GetComponent<NavMeshAgent>().isOnNavMesh)
            {
#if UNITY_EDITOR
                Debug.Log("not");
#endif
                Debug.Break();
            }
        }

        /*if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);*/
    }
    void GetPoolMonster(bool isLeader, Vector3 pos, Color color, Monster poolMonster)
    {
        //Monster monster = pool.Get();
        Monster monster = poolMonster;

        OnGetMonster(monster);

        monster.stat = isLeader ? MonsterInfo.Instance.monsterInfos[monster.monsterNum + (normalMonsterPrefab.Length)] : MonsterInfo.Instance.monsterInfos[monster.monsterNum];
        monster.ChangeOutline(color);
        monster.InitMonsterSetting(isLeader);

        NavMeshHit hit;

        if (NavMesh.SamplePosition(pos, out hit, 500, NavMesh.AllAreas))
        {
            monster.transform.position = hit.position;
        }

        if (monster.GetComponent<NavMeshAgent>())
        {
            monster.GetComponent<NavMeshAgent>().enabled = true;

            if (!monster.GetComponent<NavMeshAgent>().isOnNavMesh)
            {
#if UNITY_EDITOR
                Debug.Log("not");
#endif
                Debug.Break();
            }
        }
    }

    void GetNewMonster(bool isLeader, Vector3 pos, Color color , int monsterNum)
    {
        Monster monster = CreateMonster(monsterNum);

        monster.stat = isLeader ? MonsterInfo.Instance.monsterInfos[monster.monsterNum + (normalMonsterPrefab.Length)] : MonsterInfo.Instance.monsterInfos[monster.monsterNum];
        monster.ChangeOutline(color);
        monster.InitMonsterSetting(isLeader);

        NavMeshHit hit;

        if (NavMesh.SamplePosition(pos, out hit, 500, NavMesh.AllAreas))
        {
            monster.transform.position = hit.position;
        }

        if (monster.GetComponent<NavMeshAgent>())
        {
            monster.GetComponent<NavMeshAgent>().enabled = true;

            if (!monster.GetComponent<NavMeshAgent>().isOnNavMesh)
            {
#if UNITY_EDITOR
                Debug.Log("not");
#endif
                Debug.Break();
            }
        }
    }

    void SpawnSubordinateMonster(Vector3 pos, int count)
    {
        Vector3 spawnPos;

        for (int i = 0; i < count; i++)
        {
            spawnPos = pos + Random.onUnitSphere * 2;
            spawnPos.y = 0;

            if (!spawnTransform.bounds.Contains(spawnPos))
            {
                spawnPos = spawnTransform.bounds.ClosestPoint(spawnPos);
            }

            GameObject spawnMark = Instantiate(spawnImage, spawnPos, spawnImage.transform.rotation, storageParent);
            Destroy(spawnMark, 1f);
            StartCoroutine(SpawnMonster(false, spawnPos, Color.white));
        }
    }

    void RendBossSpawnImage(int round)
    {
        if (round != 30)
        {
            GameObject spawnMark = Instantiate(bossSpawnImage, new Vector3(3, 0, -37), bossSpawnImage.transform.rotation, bosssParent);
            Destroy(spawnMark, 2.1f);
            StartCoroutine(CreateBossMonster(round));
        }

        else if (round == 30)
        {
            GameObject spawnMark = Instantiate(bossSpawnImage, new Vector3(3, 0, -37), bossSpawnImage.transform.rotation, bosssParent);
            Destroy(spawnMark, 2.1f);
            GameObject spawnMark2 = Instantiate(bossSpawnImage, new Vector3(-3, 0, -37), bossSpawnImage.transform.rotation, bosssParent);
            Destroy(spawnMark2, 2.1f);
            StartCoroutine(CreateBossMonster(round));
        }
    }

    private Monster CreateMonster()
    {
        int num = RandomMonster();
        Monster monster = Instantiate(normalMonsterPrefab[num]).GetComponent<Monster>();
        monster.monsterNum = num;
        monster.SetManagedPool(pool);
        monster.transform.SetParent(storageParent);

        return monster;
    }

    private Monster CreateMonster(int num)
    {
        Monster monster = Instantiate(normalMonsterPrefab[num]).GetComponent<Monster>();
        monster.monsterNum = num;
        monster.SetManagedPool(pool);
        monster.transform.SetParent(storageParent);

        return monster;
    }

    private IEnumerator CreateBossMonster(int round)
    {
        yield return new WaitForSeconds(2);

        if (round != 30)
        {
            GameObject inst = Instantiate(bossMonsterPrefab[(round / 10) - 1]);
            Monster monster = inst.GetComponent<Monster>();
            monster.stat = MonsterInfo.Instance.monsterInfos[normalMonsterPrefab.Length + ((round / 10) - 1)];
            monster.transform.position = new Vector3(3, 0, -37);
            monster.transform.SetParent(bosssParent);
            monster.GetComponent<NavMeshAgent>().enabled = true;
        }

        else if (round == 30)
        {
            GameObject inst1 = Instantiate(bossMonsterPrefab[0]);
            GameObject inst2 = Instantiate(bossMonsterPrefab[1]);
            Monster monster1 = inst1.GetComponent<Monster>();
            Monster monster2 = inst2.GetComponent<Monster>();
            monster1.stat = MonsterInfo.Instance.monsterInfos[normalMonsterPrefab.Length];
            monster1.transform.position = new Vector3(3, 0, -37);
            monster2.stat = MonsterInfo.Instance.monsterInfos[normalMonsterPrefab.Length + 1];
            monster2.transform.position = new Vector3(-3, 0, -37);
            monster2.transform.SetParent(bosssParent);
            monster1.transform.SetParent(bosssParent);
        }
    }

    int RandomMonster()
    {
        weightValue[0] = Mathf.Clamp(100 - (gameManager.round * 4f), 10, 100);
        weightValue[1] = gameManager.round >= 10 ? 10 + (gameManager.round - 10) * 6 : 0;
        weightValue[2] = gameManager.round >= 5 ? (gameManager.round * 0.2f + 1) * 10f : 0;
        weightValue[3] = Mathf.Clamp(100 - (gameManager.round * 4f), 10, 100);
        weightValue[4] = Mathf.Clamp(100 - (gameManager.round * 4f), 10, 100);

        /*Debug.Log(gameManager.round);
        Debug.Log(weightValue[0] + " " + weightValue[2] + " " + weightValue[1]);*/

        float rand = Random.Range(0, totalWeight);
        int spawnNum = 0;
        float total = 0;

        for (int i = 0; i < normalMonsterPrefab.Length; i++)
        {
            total += weightValue[i];
            if (rand < total)
            {
                spawnNum = i;
                break;
            }
        }

        return spawnNum;
    }

    private void OnGetMonster(Monster monster)
    {
        monster.gameObject.SetActive(true);
    }

    private void OnReleaseMonster(Monster monster)
    {
        monster.gameObject.SetActive(false);
    }

    private void OnDestroyMonster(Monster monster)
    {
        Destroy(monster.gameObject);
    }
}
