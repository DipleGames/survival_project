using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public enum Status
{
    Maxhp,
    Damage,
    CloseDamage,
    LongDamage,
    Recover,
    Defence,
    AttackSpeed,
    MoveSpeed,
    Critical,
    Avoid,
}

public enum SpecialStatus
{
    DoubleAxe,
    Rum,
    AmmoPouch,
    RustyHarpoon,
    BaitWarm,
    Soulmate,
    Grape,
    Tabatiere,
    Invincible,
    SilverBullet,
    BloodMadness,
    RottenCheese,
    TurTle,
    Mirror,
    Count,
}

public class GameManager : Singleton<GameManager>
{
    [SerializeField] public Texture2D[] cursorNormal;
    [SerializeField] public Texture2D[] cursorAttack;
    [SerializeField] public Transform bulletStorage;
    [SerializeField] public Transform damageStorage;
    [SerializeField] public Transform monsterBulletStorage;

    [Header("GameData")]
    [SerializeField] GameObject characterPrefab;
    [SerializeField] public float gameDayTime;
    [SerializeField] public float gameNightTime;
    [SerializeField] public int round;
    [SerializeField] public int maxRound;
    [SerializeField] public int woodCount;
    public int fishHighGradeCount = 0;
    public int fishLowGradeCount = 0;

    [Header("StatData")]
    [SerializeField] int maxHp;
    [SerializeField] int damage;
    [SerializeField] int closeDamage;
    [SerializeField] int longDamage;
    [SerializeField] int recoverHp;
    [SerializeField] int defence;
    [SerializeField] int attackSpeed;
    [SerializeField] int speed;
    [SerializeField] int critical;
    [SerializeField] int avoid;
    [SerializeField] public int dashCount;

    public int percentDamage;
    public int percentDefence;
    public int bloodDamage;

    [HideInInspector] public string currentScene;

    [HideInInspector] public bool isPause;
    [HideInInspector] public bool isClear;

    [HideInInspector] public Texture2D useCursorNormal;
    [HideInInspector] public Texture2D useCursorAttack;

    [HideInInspector] public int cursorSize;

    [HideInInspector] public bool isTuto = false;

    public Vector3 characterSpawnPos = new Vector3(0, 0, -56f);

    public Dictionary<Status, int> status = new Dictionary<Status, int>();
    public Dictionary<SpecialStatus, bool> specialStatus = new Dictionary<SpecialStatus, bool>();

    public string[] statNames = { "최대 체력", "공격력", "근거리 공격력", "원거리 공격력", "회복 수치", "방어력", "공격 속도", "이동 속도", "크리티컬", "회피율" };

    public int totalBulletCount;

    [HideInInspector] public bool isCursorVisible = true;

    public int pieceCardGetRate = 10;

    public Dictionary<MaterialType, int> haveMaterials = new Dictionary<MaterialType, int>();

    public Dictionary<int, ItemInfo> itemInfos = new Dictionary<int, ItemInfo>();
    public Dictionary<int, int> haveItems = new Dictionary<int, int>(); // 아이템id - 갯수

    public Dictionary<MaterialType, int> idByMaterialType = new Dictionary<MaterialType, int>()
    {
        {MaterialType.Wood, 2030000}, {MaterialType.Bamboo, 2030001}, {MaterialType.BlackWood, 2030002},
        {MaterialType.Branch, 2030003}, {MaterialType.Fruit, 2030004}, {MaterialType.BetterFruit, 2030005},
        {MaterialType.BestFruit, 2030006}, {MaterialType.Fish, 2030007}, {MaterialType.HighFish, 2030008},
        {MaterialType.SilkFish, 2030009}, {MaterialType.Patch, 2030010}, {MaterialType.Vodka, 2030011},
        {MaterialType.Rope, 2030012}, {MaterialType.Bottle, 2030013}, {MaterialType.Flour, 2030014},
        {MaterialType.Soil, 2030015}, {MaterialType.Salt, 1030002}, {MaterialType.SeeSalt, 1030003},
        {MaterialType.Mud, 1030001}, {MaterialType.Stick, 1030000}, {MaterialType.WaterBottle, 2020002},
        {MaterialType.Shell, 2030016}, {MaterialType.BlueCoral, 2030022}, {MaterialType.Sand, 2030023},
        {MaterialType.RedDye, 2030024}, {MaterialType.YellowDye, 2030025}, {MaterialType.GreenDye, 2030026},
        {MaterialType.BlueDye, 2030027}, {MaterialType.PurpleDye, 2030028}
    };

    public static Dictionary<int, MaterialType> materialTypeById = new Dictionary<int, MaterialType>();

    public Dictionary<Acquisition, string> aquisitionName = new Dictionary<Acquisition, string>()
    {
        {Acquisition.CraftTable, "제작대"}, {Acquisition.Logging, "벌목"}, {Acquisition.Fishing, "낚시"},
        {Acquisition.Bush, "채집" }, {Acquisition.FishPot, "통발"}, {Acquisition.CampFire, "모닥불"},
        {Acquisition.Item, "아이템 사용" }
    };

    public List<Item> itemDatas = new List<Item>();

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);

        InitSetting();
    }

    private void Start()
    {
        cursorSize = PlayerPrefs.GetInt("CursorSize", 0);
        useCursorNormal = cursorNormal[cursorSize];
        useCursorAttack = cursorAttack[cursorSize];

        Vector2 cursorHotSpot = new Vector3(useCursorNormal.width * 0.5f, useCursorNormal.height * 0.5f);
        Cursor.SetCursor(useCursorNormal, cursorHotSpot, CursorMode.ForceSoftware);

        Time.timeScale = 1;
        isPause = false;
        isClear = false;

        LoadItemData();

#if UNITY_EDITOR
        foreach (var material in idByMaterialType)
        {
            materialTypeById.Add(material.Value, material.Key);
        }

        int num = 10;

        foreach (var itemInfo in itemInfos)
        {
            if (haveItems.ContainsKey(itemInfo.Value.itemId))
                haveItems[itemInfo.Value.itemId] += num;

            else
                haveItems.Add(itemInfo.Value.itemId, num);

            num++;
        }
#endif
    }

    void InitSetting()
    {
#if UNITY_EDITOR
        /*gameDayTime = 60;
        gameNightTime = 60;
        woodCount = 20;
        round = 14;
        maxRound = 15;
        maxHp = 100;
        recoverHp = 0;
        defence = 3;
        attackSpeed = 1;
        speed = 3;
        critical = 5;
        avoid = 3;

        dashCount = 0;

        fishLowGradeCount = 30;
        fishHighGradeCount = 30;

        percentDamage = 0;
        percentDefence = 0;
        bloodDamage = 0;

        totalBulletCount = 1;*/
#else
        gameDayTime = 150;
        gameNightTime = 60;
        woodCount = 0;
        round = 0;
        maxRound = 15;
        maxHp = 100;
        recoverHp = 0;
        defence = 0;
        attackSpeed = 1;
        speed = 3;
        critical = 5;
        avoid = 3;

        dashCount = 0;

        pieceCardGetRate = 10;

        fishLowGradeCount = 0;
        fishHighGradeCount = 0;

        percentDamage = 0;
        percentDefence = 0;
        bloodDamage = 0;

        totalBulletCount = 1;
#endif

        status.Add(Status.Maxhp, maxHp);
        status.Add(Status.Damage, damage);
        status.Add(Status.CloseDamage, closeDamage);
        status.Add(Status.LongDamage, longDamage);
        status.Add(Status.Recover, recoverHp);
        status.Add(Status.Defence, defence);
        status.Add(Status.AttackSpeed, attackSpeed);
        status.Add(Status.MoveSpeed, speed);
        status.Add(Status.Critical, critical);
        status.Add(Status.Avoid, avoid);

        for (int i = 0; i < (int)SpecialStatus.Count; ++i)
        {
            specialStatus.Add((SpecialStatus)i, false);
        }

#if UNITY_EDITOR
        specialStatus[SpecialStatus.Rum] = true;
        specialStatus[SpecialStatus.AmmoPouch] = true;
#endif

        //haveMaterials.Add(MaterialType.Wood, woodCount);
    }

    public void ToNextScene(string sceneName)
    {
        Character character = Instantiate(characterPrefab, Vector3.zero, characterPrefab.transform.rotation).GetComponent<Character>();
        character.GetComponent<NavMeshAgent>().enabled = false;
        character.transform.position = characterSpawnPos;
        
        currentScene = sceneName;

        //SceneManager.LoadScene(sceneName);
    }

    public void GamePause(bool _isPause)
    {
        isPause = _isPause;

        Time.timeScale = _isPause ? 0 : 1;
    }

    void LoadItemData()
    {
        ItemInfo[] itemInfos = ItemData.Instance.itemInfos;

        for (int i = 0; i < itemInfos.Length; ++i)
        {
            this.itemInfos.Add(itemInfos[i].itemId, itemInfos[i]);

            //f (itemInfos[i].itemId / 1000000 == 01 && !string.IsNullOrEmpty(itemInfos[i].needMaterialTypes))
            itemDatas.Add(new Item(itemInfos[i].itemId, itemInfos[i].itemName, itemInfos[i].itemType, itemInfos[i].needMaterialTypes, itemInfos[i].needMaterialCounts, itemInfos[i].takeTimeByAcquisition, itemInfos[i].acquisitions, itemInfos[i].isConsumable, itemInfos[i].effect, itemInfos[i].decription));
        }
    }
}