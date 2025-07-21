using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class GamesceneManager : Singleton<GamesceneManager>
{
    [SerializeField] public GameObject monsterSpawner;
    [SerializeField] public Transform treeParent;
    [SerializeField] Transform bushParent;
    [SerializeField] Transform beachParent;
    [SerializeField] GameObject treePrefab;
    [SerializeField] GameObject bushPrefab;
    [SerializeField] GameObject campFire;
    [SerializeField] LayerMask interactionLayer;
    [SerializeField] public Collider walkableArea;
    [SerializeField] Collider treeBushSpawnArea;
    [SerializeField] GameObject nightFilter;
    [SerializeField] GameObject cardSelecter;
    [SerializeField] GameObject multicellInvenPanel;
    [SerializeField] GameObject initSpawnPos;
    [SerializeField] Sprite weaponChangeTutoImage;
    [SerializeField] Sprite multicellTutoImage;

    [HideInInspector] public float currentGameTime;

    [HideInInspector] public bool isNight = false;

    bool isSetPieceEnd = true;

    GameManager gameManager;
    Character character;
    GameSceneUI gameSceneUI;
    ItemManager itemManager;
    [SerializeField]SpawnItem beach;
    //ItemSpawner beach;
    SoundManager soundManager;

    bool canCharacterContact = false;

    public bool CanCharacterContact => canCharacterContact;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;

        gameManager = GameManager.Instance;
        character = Character.Instance;
        gameSceneUI = GameSceneUI.Instance;
        itemManager = ItemManager.Instance;
        //beach = ItemSpawner.Instance;
        soundManager = SoundManager.Instance;

        character.GetComponent<NavMeshAgent>().enabled = true;

        itemManager.pieceItemsList = itemManager.startPieceList;

        currentGameTime = 60f;

        soundManager.StopBGM();

        nightFilter.SetActive(false);

        StartCoroutine(DayRoutine());
    }

    private void Update()
    {
        if (character.currentHp > 0 && currentGameTime > 0 && !gameManager.isPause && isSetPieceEnd)
        {
            currentGameTime -= Time.deltaTime;

            if (currentGameTime < 0)
                currentGameTime = 0;
        }

        else if(character.currentHp <= 0)
        {
            StopAllCoroutines();
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator DayRoutine()
    {
        gameManager.bloodDamage = 0;
        gameSceneUI.CursorChange(CursorType.Normal);

        isNight = false;
        character.ChangeAnimationController(0);

        character.UpdateStat();
        character.weaponParent.gameObject.SetActive(false);

#if UNITY_EDITOR
        //StartCoroutine(SpawnTree());
        if (gameManager.round % 3 == 0)
        {
            StartCoroutine(SpawnTree());
        }
#else
        if (gameManager.round % 3 == 0)
        {
            StartCoroutine(SpawnTree());
        }
#endif

        StartCoroutine(SpawnBush());
        StartCoroutine(beach.SpawnItems());

        isSetPieceEnd = false;
        character.isCanControll = false;

        gameSceneUI.ActiveTutoPanel(TutoType.StartTuto, null, TutoType.StartItemTuto);

        yield return CoroutineCaching.WaitWhile(() => TutorialManager.Instance.tutorialCheck);

        cardSelecter.SetActive(true);

        //yield return new WaitWhile(() => cardSelecter.activeSelf);
        yield return CoroutineCaching.WaitWhile(() => cardSelecter.activeSelf);

        character.transform.position = gameManager.round == 0 ? gameManager.characterSpawnPos : new Vector3(-1f, 0f, -41f);

        gameManager.round++;

        if (gameManager.round > 1)
        {
#if UNITY_EDITOR
#else
            gameManager.gameDayTime -= 7;
#endif
            soundManager.PlayBGM(5, true);
            multicellInvenPanel.SetActive(true);

            gameSceneUI.ActiveTutoPanel(TutoType.MulticellTuto, multicellTutoImage);
            currentGameTime = gameManager.gameDayTime;
        }

        character.canWeaponChange = true;

        for (int i = 0; i < character.weaponParent.childCount; ++i)
        {
            if (!character.weaponParent.GetChild(i).gameObject.activeSelf)
            {
                character.weaponParent.GetChild(i).gameObject.SetActive(true);
                character.weaponParent.GetChild(i).gameObject.SetActive(false);
            }
        }

        //yield return new WaitWhile(() => multicellInvenPanel.activeSelf);
        yield return CoroutineCaching.WaitWhile(() => multicellInvenPanel.activeSelf);

        canCharacterContact = true;
        nightFilter.SetActive(false);
        gameSceneUI.ChangeDayText(0, "아침이 밝았습니다.");

        soundManager.PlayBGM(1, true);

        gameSceneUI.ActiveTutoPanel(TutoType.BeachTuto);

        isSetPieceEnd = true;
        character.isCanControll = true;

        //yield return new WaitForSeconds(gameManager.gameDayTime);
        //yield return CoroutineCaching.WaitForSeconds(gameManager.gameDayTime);
        yield return CoroutineCaching.WaitUntil(() => currentGameTime <= 0);

        StartCoroutine(NightRoutine()); 
    }

    IEnumerator NightRoutine()
    {
        soundManager.PlayBGM(4, true);

        canCharacterContact = false;
        gameSceneUI.ChangeDayText(1, "밤이 되었습니다.");
        gameSceneUI.CursorChange(CursorType.Attack);

        isNight = true;
        nightFilter.SetActive(true);

        gameSceneUI.ActiveTutoPanel(TutoType.NightTuto, weaponChangeTutoImage);

        if (gameManager.specialStatus[SpecialStatus.AmmoPouch])
        {
            if (gameManager.totalBulletCount <= 0)
                gameManager.totalBulletCount++;
        }

        character.UpdateStat();
        character.weaponParent.gameObject.SetActive(true);
        character.transform.position = new Vector3(-1f, 0f, -41f);

        campFire.GetComponent<Campfire>().ToNightScene();

        if (gameManager.specialStatus[SpecialStatus.Rum])
        {
            character.speed *= 1.2f;
        }

        

        currentGameTime = gameManager.gameNightTime;

        character.isCanControll = true;


        //yield return new WaitForSeconds(gameManager.gameNightTime);
        //yield return CoroutineCaching.WaitForSeconds(gameManager.gameNightTime);
        yield return CoroutineCaching.WaitUntil(() => currentGameTime <= 0);

        if (gameManager.round != gameManager.maxRound)
        {
#if UNITY_EDITOR
#else
            gameManager.fishLowGradeCount = 0;
            gameManager.fishHighGradeCount = 0;
#endif

            if (character.IsTamingPet)
                character.TamedPed.GetComponent<Pet>().RunAway();

            campFire.GetComponent<Campfire>().ToDayScene();

            itemManager.pieceItemsList = itemManager.nightPieceList;

            StartCoroutine(DayRoutine());
        }

        else
        {
            character.isCanControll = false;
            character.canFlip = false;

            nightFilter.SetActive(false);
            StopAllCoroutines();
            gameManager.isClear = true;

            soundManager.StopBGM();
            StartCoroutine(gameSceneUI.GameClear(1));
        }
    }

    IEnumerator SpawnTree()
    {
        foreach (Transform trees in treeParent)
        {
            Destroy(trees.gameObject);
        }

        for (int i = 0; i < 20; i++)
        {
            SpawnOneBushOrTree(treePrefab, treeParent);
            yield return null;      // 약 0.002초
        }
    }

    IEnumerator SpawnBush()
    {
        foreach (Transform bushs in bushParent)
        {
            Destroy(bushs.gameObject);
        }


        for (int i = 0; i < 12; i++)
        {
            SpawnOneBushOrTree(bushPrefab, bushParent);
            yield return null;
        }
    }

    Vector3 SpawnTreeAndBushPos()
    {
        Vector3 spawnPos = TreeAndBushPos();
        int count = 0;

        while (true)
        {
            count++;

            if(count > 99999)
            {
                //Debug.LogError("Infinite Loop Skip");
                spawnPos = Vector3.zero;
                break;
            }

            if (Physics.OverlapSphere(spawnPos, 2f, interactionLayer).Length <= 0)
            {
                break;
            }

            spawnPos = TreeAndBushPos();
        }

        return spawnPos;
    }

    void SpawnOneBushOrTree(GameObject spawnObject, Transform parent)
    {
        Vector3 spawnPos = SpawnTreeAndBushPos();

        if (spawnPos == Vector3.zero)
            return;

        Instantiate(spawnObject, SpawnTreeAndBushPos(), spawnObject.transform.rotation, parent);
    }

    Vector3 TreeAndBushPos()
    {
        float groundX = treeBushSpawnArea.bounds.size.x;
        float groundZ = treeBushSpawnArea.bounds.size.z;

        groundX = Random.Range((groundX / 2f) * -1f + treeBushSpawnArea.bounds.center.x, (groundX / 2f) + treeBushSpawnArea.bounds.center.x);
        groundZ = Random.Range((groundZ / 2f) * -1f + treeBushSpawnArea.bounds.center.z, (groundZ / 2f) + treeBushSpawnArea.bounds.center.z);

        return new Vector3(groundX, 0, groundZ);
    }

#if UNITY_EDITOR
    void OnGUI () 
    {
        float width = Screen.width;
        float height = Screen.height;

        GUIStyle styleBox = new GUIStyle("box");
        styleBox.fontSize = (int)(30 * width / 1920);

        GUIStyle style = new GUIStyle("button");
        style.fontSize = (int)(50 * width / 1920);

        // 배경 박스 만들기
        GUI.Box(new Rect(0,0,width * 0.2f , height * 0.4f), "Menu", styleBox);
    
        // 버튼 하나 만들고 클릭시 true가되어 함수 호출
        if(GUI.Button(new Rect(width * 0.02f, height * 0.05f, width * 0.16f, height * 0.09f ), "스킵",style)) 
        {
            currentGameTime = 0;
        }
    
        /*// 두번 째 버튼 만들기
        if(GUI.Button(new Rect(width * 0.02f, height * 0.15f, width * 0.16f, height * 0.09f), "Level 2")) 
        {
            //Application.LoadLevel(2);
        }*/
    }
#endif
}
