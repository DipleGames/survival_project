using System.Collections.Generic;
using UnityEngine;

public class ItemManager : Singleton<ItemManager>
{
    [Header("Item Data")]
    public Dictionary<int, ItemInfo> itemInfos = new();
    public Dictionary<int, int> haveItems = new();
    public List<Item> itemDatas = new();

    [Header("Material Data")]
    public Dictionary<MaterialType, int> haveMaterials = new();

    public Dictionary<MaterialType, int> idByMaterialType = new()
    {
        { MaterialType.Wood, 2030000 }, { MaterialType.Bamboo, 2030001 },
        { MaterialType.BlackWood, 2030002 }, { MaterialType.Branch, 2030003 },
        { MaterialType.Fruit, 2030004 }, { MaterialType.BetterFruit, 2030005 },
        { MaterialType.BestFruit, 2030006 }, { MaterialType.Fish, 2030007 },
        { MaterialType.HighFish, 2030008 }, { MaterialType.SilkFish, 2030009 },
        { MaterialType.Patch, 2030010 }, { MaterialType.Vodka, 2030011 },
        { MaterialType.Rope, 2030012 }, { MaterialType.Bottle, 2030013 },
        { MaterialType.Flour, 2030014 }, { MaterialType.Soil, 2030015 },
        { MaterialType.Salt, 1030002 }, { MaterialType.SeeSalt, 1030003 },
        { MaterialType.Mud, 1030001 }, { MaterialType.Stick, 1030000 },
        { MaterialType.WaterBottle, 2020002 }
    };

    public static Dictionary<int, MaterialType> materialTypeById = new();

    public Dictionary<Acquisition, string> aquisitionName = new()
    {
        { Acquisition.CraftTable, "제작대" }, { Acquisition.Logging, "벌목" },
        { Acquisition.Fishing, "낚시" }, { Acquisition.Bush, "채집" },
        { Acquisition.FishPot, "통발" }, { Acquisition.CampFire, "모닥불" },
        { Acquisition.Item, "아이템 사용" }
    };

    [Header("Debug")]
    [SerializeField] private bool giveTestItemsOnStart;

    private bool _initialized;
    public bool IsInitialized => _initialized;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        Initialize();
        DebugHaveItems();
    }

    /// <summary>
    /// ItemData가 준비된 이후 최초 한 번만 아이템 정보를 불러온다.
    /// ItemInventory가 Start에서 호출해도 중복 초기화되지 않는다.
    /// </summary>
    public bool Initialize()
    {
        if (_initialized)
            return true;

        if (ItemData.Instance == null || ItemData.Instance.itemInfos == null)
        {
            Debug.LogError("[ItemManager] ItemData를 찾을 수 없어 초기화할 수 없습니다.", this);
            return false;
        }

        InitializeMaterialMapping();
        LoadItemData();
        _initialized = true;

#if UNITY_EDITOR
        if (giveTestItemsOnStart)
            GiveTestItems();
#endif

        return true;
    }

    /// <summary>
    /// MaterialType에서 아이템 ID로 가는 매핑의 역방향 Dictionary를 구성한다.
    /// </summary>
    private void InitializeMaterialMapping()
    {
        materialTypeById.Clear();

        foreach (var pair in idByMaterialType)
            materialTypeById[pair.Value] = pair.Key;
    }

    /// <summary>
    /// ItemData에 정의된 아이템 정보를 Dictionary와 List로 구성한다.
    /// </summary>
    public void LoadItemData()
    {
        if (ItemData.Instance == null || ItemData.Instance.itemInfos == null)
        {
            Debug.LogError("[ItemManager] ItemData.itemInfos가 없습니다.", this);
            return;
        }

        itemInfos.Clear();
        itemDatas.Clear();

        foreach (ItemInfo info in ItemData.Instance.itemInfos)
        {
            if (info == null)
                continue;

            if (itemInfos.ContainsKey(info.itemId))
            {
                Debug.LogWarning($"[ItemManager] 중복 아이템 ID: {info.itemId}", this);
                continue;
            }

            itemInfos.Add(info.itemId, info);
            itemDatas.Add(new Item(info.itemId, info.itemName, info.itemType,
                info.needMaterialTypes, info.needMaterialCounts, info.takeTimeByAcquisition,
                info.acquisitions, info.isConsumable, info.effect, info.decription));
        }
    }

    /// <summary>
    /// 아이템 ID로 원본 아이템 정보를 조회한다.
    /// </summary>
    public ItemInfo GetItemInfo(int itemId)
    {
        return itemInfos.TryGetValue(itemId, out ItemInfo info) ? info : null;
    }

    /// <summary>
    /// 보유 수량을 조회한다. 보유하지 않은 아이템은 0을 반환한다.
    /// </summary>
    public int GetItemCount(int itemId)
    {
        return haveItems.TryGetValue(itemId, out int count) ? count : 0;
    }

    /// <summary>
    /// 지정한 아이템을 보유 목록에 추가한다.
    /// </summary>
    public void AddItem(int itemId, int count)
    {
        if (count <= 0)
            return;

        if (!itemInfos.ContainsKey(itemId))
        {
            Debug.LogWarning($"[ItemManager] 등록되지 않은 아이템 ID: {itemId}", this);
            return;
        }

        haveItems[itemId] = GetItemCount(itemId) + count;
    }

    /// <summary>
    /// 보유 수량이 충분하면 차감하고 true를 반환한다.
    /// </summary>
    public bool TryRemoveItem(int itemId, int count)
    {
        if (count <= 0 || GetItemCount(itemId) < count)
            return false;

        int remaining = GetItemCount(itemId) - count;

        if (remaining == 0)
            haveItems.Remove(itemId);
        else
            haveItems[itemId] = remaining;

        return true;
    }

    [ContextMenu("보유 아이템 확인")]
    public void DebugHaveItems()
    {
        Debug.Log($"현재 보유 아이템 종류: {haveItems.Count}개");

        foreach (var item in haveItems)
        {
            ItemInfo info = GetItemInfo(item.Key);
            string itemName = info != null ? info.itemName : "알 수 없는 아이템";

            Debug.Log($"아이템: {itemName} / ID: {item.Key} / 수량: {item.Value}");
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// 에디터 테스트용 아이템 지급. 최초 초기화 시 한 번만 실행된다.
    /// </summary>
    private void GiveTestItems()
    {
        int count = 10;

        foreach (var pair in itemInfos)
            AddItem(pair.Key, count++);
    }
#endif

    // Load
    public bool RestoreItems(List<ItemStackSaveData> savedItems)
    {
        if (!Initialize())
            return false;

        if (savedItems == null)
        {
            Debug.LogError("저장된 아이템 목록이 없습니다.");
            return false;
        }

        // 기존 수량과 합쳐지지 않도록 초기화
        haveItems.Clear();

        foreach (var item in savedItems)
        {
            if (item == null)
                continue;

            AddItem(item.itemId, item.count);
        }

        return true;
    }
}
