using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 아이템 인벤토리의 슬롯 생성, 카테고리 전환, 정렬 및 드래그 UI를 관리한다.
/// </summary>
public class ItemInventory : MonoBehaviour
{
    public enum InventoryTab { Equipment, Consumable, Miscellaneous }

    private static readonly int[] EquipmentCategories = { (int)ItemType.Tool };
    private static readonly int[] ConsumableCategories = { (int)ItemType.Consumable };
    private static readonly int[] MiscellaneousCategories =
    {
        (int)ItemType.Material, (int)ItemType.Piece, (int)ItemType.Facility
    };
    [SerializeField] private GameObject descriptionPanel;
    [SerializeField] private Transform slotParent;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private GameObject invenPanel;
    [SerializeField] private GameObject dragSlot;
    [SerializeField] private InventoryCategoryTabUI categoryTabUI;

    private const int SlotCount = 36;

    private readonly Dictionary<int, int?[]> categorySlotItems = new();
    private List<ItemInvenSlot> slots;
    private ItemManager itemManager;
    private readonly List<int> visibleCategories = new();
    private InventoryTab currentTab;
    private bool initialized;

    public GameObject DragSlot => dragSlot;

    /// <summary>
    /// 인벤토리 슬롯 UI만 생성한다. 아이템 데이터는 아직 읽지 않는다.
    /// </summary>
    private void Awake()
    {
        slots = new List<ItemInvenSlot>(SlotCount);

        if (slotPrefab == null || slotParent == null)
        {
            Debug.LogError("[ItemInventory] Slot Prefab 또는 Slot Parent가 할당되지 않았습니다.", this);
            enabled = false;
            return;
        }

        if (!EnsureSlotCount(SlotCount))
            return;

        SetDescriptionText("");

        if (invenPanel != null)
            invenPanel.SetActive(false);
    }

    private bool EnsureSlotCount(int count)
    {
        while (slots.Count < count)
        {
            GameObject obj = Instantiate(slotPrefab, slotParent);
            ItemInvenSlot slot = obj.GetComponent<ItemInvenSlot>();

            if (slot == null)
            {
                Debug.LogError("[ItemInventory] 슬롯 프리팹에 ItemInvenSlot 컴포넌트가 없습니다.", obj);
                Destroy(obj);
                enabled = false;
                return false;
            }

            slot.inventory = this;
            slots.Add(slot);
        }

        return true;
    }

    /// <summary>
    /// ItemManager 초기화를 보장한 후 슬롯 데이터를 구성하고 UI를 갱신한다.
    /// </summary>
    private void Start()
    {
        if (!enabled)
            return;

        itemManager = ItemManager.Instance;

        if (itemManager == null)
        {
            Debug.LogError("[ItemInventory] ItemManager가 씬에 없습니다.", this);
            enabled = false;
            return;
        }

        if (!itemManager.Initialize())
        {
            Debug.LogError("[ItemInventory] 아이템 데이터를 초기화하지 못했습니다.", this);
            enabled = false;
            return;
        }

        initialized = true;
        ChangeCategory((int)InventoryTab.Equipment);
    }


    /// <summary>
    /// 선택된 카테고리의 아이템들을 동기화하고 슬롯 UI를 다시 그린다.
    /// </summary>
    public void ChangeCategory(int tab)
    {
        if (!initialized || tab < (int)InventoryTab.Equipment || tab > (int)InventoryTab.Miscellaneous)
            return;

        currentTab = (InventoryTab)tab;
        if (categoryTabUI != null)
            categoryTabUI.SetSelectedCategory(tab);

        SetDescriptionText("");
        RefreshInventory();
    }

    private void SyncAllSlotData()
    {
        SyncSlotData((int)ItemType.Tool);
        SyncSlotData((int)ItemType.Consumable);
        foreach (int category in MiscellaneousCategories)
            SyncSlotData(category);
    }

    private static int[] GetTabCategories(InventoryTab tab)
    {
        return tab switch
        {
            InventoryTab.Equipment => EquipmentCategories,
            InventoryTab.Consumable => ConsumableCategories,
            InventoryTab.Miscellaneous => MiscellaneousCategories,
            _ => System.Array.Empty<int>()
        };
    }

    internal static bool IsInTab(ItemType itemType, InventoryTab tab)
    {
        return System.Array.IndexOf(GetTabCategories(tab), GetSlotCategory((int)itemType)) >= 0;
    }

    private static int GetSlotCategory(int itemType)
    {
        // 기존 저장 데이터에서 무기는 도구 슬롯 배치에 포함된다.
        return itemType == (int)ItemType.Weapon ? (int)ItemType.Tool : itemType;
    }

    public List<InventorySlotSaveData> CallInventoryData()
    {
        SyncAllSlotData();
        var data = new List<InventorySlotSaveData>();
        foreach (var category in categorySlotItems)
        {
            int?[] items = category.Value;

            for (int i = 0; i < items.Length; i++)
            {
                data.Add(new InventorySlotSaveData
                {
                    category = category.Key,
                    slotIndex = i,
                    itemId = items[i] ?? -1
                });
            }
        }
        return data;
    }


    /// <summary>
    /// 드래그한 두 슬롯의 아이템 배치를 교환한다.
    /// </summary>
    public void MoveItem(ItemInvenSlot fromSlot, ItemInvenSlot toSlot)
    {
        if (!initialized)
            return;

        int fromIndex = slots.IndexOf(fromSlot);
        int toIndex = slots.IndexOf(toSlot);

        if (fromIndex < 0 || toIndex < 0 || fromIndex == toIndex)
            return;

        int group = fromIndex / SlotCount;
        if (group >= visibleCategories.Count || group != toIndex / SlotCount)
            return;

        int?[] items = categorySlotItems[visibleCategories[group]];
        fromIndex %= SlotCount;
        toIndex %= SlotCount;

        if (!items[fromIndex].HasValue)
            return;

        (items[fromIndex], items[toIndex]) = (items[toIndex], items[fromIndex]);
        RefreshSlots();
    }

    /// <summary>
    /// 현재 카테고리의 빈 슬롯을 뒤로 보내도록 정렬한다.
    /// </summary>
    public void SortInventory()
    {
        if (!initialized)
            return;

        foreach (int category in visibleCategories)
        {
            SyncSlotData(category);
            CompactSlots(categorySlotItems[category]);
        }

        RefreshSlots();
    }

    private static void CompactSlots(int?[] items)
    {
        int writeIndex = 0;

        for (int readIndex = 0; readIndex < items.Length; readIndex++)
        {
            if (items[readIndex].HasValue)
                items[writeIndex++] = items[readIndex];
        }

        for (int i = writeIndex; i < items.Length; i++)
            items[i] = null;
    }

    /// <summary>
    /// 선택한 아이템의 설명을 표시한다.
    /// </summary>
    public void SetItemDescription(ItemInfo item)
    {
        if (item == null)
        {
            SetDescriptionText("");
            return;
        }

        string effect = item.effect?.Replace("\\n", "\n") ?? "";
        string description = item.decription?.Replace("\\n", "\n") ?? "";
        string equipmentLabel = (ItemType)item.itemType switch
        {
            ItemType.Tool => "\n생활 장비",
            ItemType.Weapon => "\n전투 장비",
            _ => ""
        };
        SetDescriptionText($"{item.itemName}{equipmentLabel}\n\n{effect}\n\n{description}");
    }

    /// <summary>
    /// Text와 TMP_Text 중 존재하는 컴포넌트로 설명 UI를 갱신한다.
    /// </summary>
    private void SetDescriptionText(string value)
    {
        if (descriptionPanel == null)
            return;

        Text legacyText = descriptionPanel.GetComponentInChildren<Text>(true);
        TMP_Text tmpText = descriptionPanel.GetComponentInChildren<TMP_Text>(true);

        if (legacyText != null)
            legacyText.text = value;
        else if (tmpText != null)
            tmpText.text = value;
    }

    /// <summary>
    /// 아이템 드래그 UI를 마우스 위치로 이동한다.
    /// </summary>
    public void MoveDragSlot(PointerEventData eventData)
    {
        if (dragSlot == null)
            return;

        RectTransform dragRect = dragSlot.GetComponent<RectTransform>();

        if (dragRect == null)
            return;

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
            dragRect, eventData.position, eventData.pressEventCamera, out Vector3 worldPos))
        {
            dragRect.position = worldPos;
        }
    }

    /// <summary>
    /// 보유 목록을 기준으로 제거된 아이템을 비우고 새 아이템을 추가한다.
    /// </summary>
    private void SyncSlotData(int type)
    {
        if (!categorySlotItems.TryGetValue(type, out int?[] items))
        {
            items = new int?[SlotCount];
            categorySlotItems[type] = items;
        }

        for (int i = 0; i < items.Length; i++)
        {
            if (!items[i].HasValue)
                continue;

            int itemId = items[i].Value;

            if (itemManager.GetItemCount(itemId) <= 0 ||
                !itemManager.itemInfos.TryGetValue(itemId, out ItemInfo info) ||
                info == null || GetSlotCategory(info.itemType) != type)
            {
                items[i] = null;
            }
        }

        foreach (var item in itemManager.haveItems)
        {
            int itemId = item.Key;

            if (item.Value <= 0 || !itemManager.itemInfos.TryGetValue(itemId, out ItemInfo info) ||
                info == null || GetSlotCategory(info.itemType) != type)
                continue;

            if (System.Array.IndexOf(items, (int?)itemId) >= 0)
                continue;

            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].HasValue)
                    continue;

                items[i] = itemId;
                break;
            }
        }
    }

    /// <summary>
    /// 저장된 배치에 따라 현재 카테고리의 UI 슬롯을 갱신한다.
    /// 빈 카테고리에서는 모든 슬롯을 비운다.
    /// </summary>
    private void RefreshSlots()
    {
        int visibleSlotCount = visibleCategories.Count * SlotCount;
        if (!EnsureSlotCount(visibleSlotCount))
            return;

        for (int i = 0; i < slots.Count; i++)
        {
            bool visible = i < visibleSlotCount;
            slots[i].gameObject.SetActive(visible);
            if (!visible)
                continue;

            int? itemId = categorySlotItems[visibleCategories[i / SlotCount]][i % SlotCount];
            slots[i].SetSlot(itemId.HasValue ? itemManager.GetItemInfo(itemId.Value) : null);
        }
    }

    /// <summary>
    /// 외부에서 아이템 수량이나 저장 데이터를 복원한 뒤 UI를 갱신할 때 호출한다.
    /// </summary>
    public void RefreshInventory()
    {
        if (!initialized)
            return;

        SyncAllSlotData();
        visibleCategories.Clear();

        int[] categories = GetTabCategories(currentTab);
        foreach (int category in categories)
        {
            if (System.Array.Exists(categorySlotItems[category], item => item.HasValue))
                visibleCategories.Add(category);
        }

        if (visibleCategories.Count == 0)
            visibleCategories.Add(categories[0]);

        RefreshSlots();
    }

    //Load
    public bool RestoreSlots(List<InventorySlotSaveData> savedSlots)
    {
        if (!initialized)
        {
            Debug.LogError("인벤토리 초기화가 완료되지 않았습니다.");
            return false;
        }

        if (savedSlots == null)
        {
            Debug.LogError("저장된 슬롯 목록이 없습니다.");
            return false;
        }

        // 이전 배치 제거
        categorySlotItems.Clear();

        foreach (var saved in savedSlots)
        {
            if (saved == null)
                continue;

            // 잘못된 슬롯 번호 제외
            if (saved.slotIndex < 0 || saved.slotIndex >= SlotCount)
                continue;

            int category = GetSlotCategory(saved.category);

            // 카테고리별 슬롯 배열 생성
            if (!categorySlotItems.TryGetValue(
                category, out int?[] items))
            {
                items = new int?[SlotCount];
                categorySlotItems.Add(category, items);
            }

            // 새 배열은 null로 초기화되므로 빈 슬롯은 건너뜀
            if (saved.itemId == -1)
                continue;

            ItemInfo info = itemManager.GetItemInfo(saved.itemId);

            // 존재하고, 보유하고 있으며, 카테고리가 맞는 아이템만 배치
            if (info == null ||
                itemManager.GetItemCount(saved.itemId) <= 0 ||
                GetSlotCategory(info.itemType) != category)
            {
                continue;
            }

            // 중복 배치 및 같은 슬롯 덮어쓰기 방지
            if (items[saved.slotIndex].HasValue ||
                System.Array.IndexOf(items, (int?)saved.itemId) >= 0)
            {
                continue;
            }

            items[saved.slotIndex] = saved.itemId;
        }

        SetDescriptionText("");
        RefreshInventory();
        return true;
    }
}
