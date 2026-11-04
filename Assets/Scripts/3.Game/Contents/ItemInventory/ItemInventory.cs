using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 인벤토리 UI를 관리하는 클래스
/// </summary>
public class ItemInventory : MonoBehaviour
{
    [SerializeField] GameObject descriptionPanel;
    [SerializeField] Transform slotParent;
    [SerializeField] GameObject slotPrefab;
    [SerializeField] GameObject invenPanel;
    [SerializeField] GameObject dragSlot;
    [SerializeField] private InventoryCategoryTabUI categoryTabUI; //카테고리 탭 UI 관리하는 스크립트 참조
    private Dictionary<int, int?[]> categorySlotItems = new Dictionary<int, int?[]>();
    private const int SlotCount = 36;
    public GameObject DragSlot => dragSlot;

    int currentCategory;
    List<ItemInvenSlot> slots;

    GameManager gameManager;

    private void Awake()
    {
        slots = new List<ItemInvenSlot>();
        gameManager = GameManager.Instance;

        currentCategory = 0;

        for (int i = 0; i < 36; i++)
        {
            ItemInvenSlot slot = Instantiate(slotPrefab, slotParent).GetComponent<ItemInvenSlot>();
            slot.inventory = this;
            slots.Add(slot);
        }

        descriptionPanel.GetComponent<Text>().text = "";

        invenPanel.SetActive(false);
        InitializeSlotData();
    }

    private void Update()
    {
/*        if (Input.GetKeyDown(KeyCode.U))
        {
            invenPanel.SetActive(!invenPanel.activeSelf);

            if(invenPanel.activeSelf) ChangeCategory(currentCategory);
        }
    }

    public void ChangeCategory(int type)
    {
        if (categoryTabUI != null)
        {
            categoryTabUI.SetSelectedCategory(type);
        }
        //int index = 0;
        //foreach (var item in gameManager.haveItems)
        //{
        //    if (gameManager.itemInfos[item.Key].itemType == type)
        //    {
        //        slots[index].SetSlot(gameManager.itemInfos[item.Key]);
        //        index++;
        //    }
        //}
        //for (int i = index; i < slots.Count; i++)
        //{
        //    slots[i].SetSlot(null);
        //}
        currentCategory = type;

        SyncSlotData(type); //현재 카테고리 배치 데이터와 실제 보유 아이템 동기화
        RefreshSlots(); //저장된 슬롯 배치 순서대로 UI 갱신
     

    }
    private void InitializeSlotData()
    {
        categorySlotItems.Clear();

        // 기존 아이템 배치 데이터 초기화
        foreach (var item in gameManager.haveItems)
        {
            int itemId = item.Key;
            int type = gameManager.itemInfos[itemId].itemType;

            if (!categorySlotItems.ContainsKey(type))
            {
                categorySlotItems[type] = new int?[SlotCount];
            }

            int?[] items = categorySlotItems[type];

            for (int i = 0; i < items.Length; i++)
            {
                if (!items[i].HasValue)
                {
                    items[i] = itemId;
                    break;
                }
            }
        }
    }

    public void MoveItem(ItemInvenSlot fromSlot, ItemInvenSlot toSlot) //인벤토리 내 아이템 움직이는 함수
    {
        int fromIndex = slots.IndexOf(fromSlot);
        int toIndex = slots.IndexOf(toSlot);

        if (fromIndex < 0 || toIndex < 0) return;

        if (fromIndex == toIndex) return;

        if (!categorySlotItems.TryGetValue(currentCategory, out int?[] items)) return;

        // 출발 슬롯에 아이템이 없다면 이동하지 않음
        if (!items[fromIndex].HasValue) return;

        // 두 슬롯의 아이템 위치 교환
        int? temp = items[fromIndex];

        items[fromIndex] = items[toIndex];
        items[toIndex] = temp;

        RefreshSlots();
    }

    public void SortInventory() // 현재 카테고리의 슬롯 데이터를 정렬하여 빈 슬롯을 뒤로 이동, ChangeCategory에서 진행하던 동기화 및 UI 갱신 작업을 여기서 수행
    {
        SyncSlotData(currentCategory);

        int?[] items = categorySlotItems[currentCategory];

        int writeIndex = 0;

        for (int readIndex = 0; readIndex < items.Length; readIndex++)
        {
            if (items[readIndex].HasValue)
            {
                items[writeIndex] = items[readIndex];
                writeIndex++;
            }
        }

        // 남은 슬롯 비우기
        for (int i = writeIndex; i < items.Length; i++)
        {
            items[i] = null;
        }

        RefreshSlots();
    }

    public void SetItemDescription(ItemInfo item)
    {
        descriptionPanel.GetComponent<Text>().text = $"{item.itemName}\n\n{item.effect.Replace("\\n", "\n")}\n\n{item.decription.Replace("\\n","\n")}";
    }
    public void MoveDragSlot(PointerEventData eventData)
    {
        RectTransform dragRect = dragSlot.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToWorldPointInRectangle(dragRect, eventData.position, eventData.pressEventCamera, out Vector3 worldPos);
        dragRect.position = worldPos;
    }

    private void SyncSlotData(int type)
    {
        if (!categorySlotItems.ContainsKey(type))
        {
            categorySlotItems[type] = new int?[SlotCount];
        }

        int?[] items = categorySlotItems[type];

        // 보유하지 않은 아이템 제거
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].HasValue &&
                (!gameManager.haveItems.ContainsKey(items[i].Value) || !gameManager.itemInfos.ContainsKey(items[i].Value) || gameManager.itemInfos[items[i].Value].itemType != type))
            {
                items[i] = null;
            }
        }

        // 새로 획득한 아이템을 빈 슬롯에 추가
        foreach (var item in gameManager.haveItems)
        {
            int itemId = item.Key;

            if (!gameManager.itemInfos.ContainsKey(itemId)) continue;

            if (gameManager.itemInfos[itemId].itemType != type) continue;

            if (System.Array.IndexOf(items, (int?)itemId) >= 0) continue;

            for (int i = 0; i < items.Length; i++)
            {
                if (!items[i].HasValue)
                {
                    items[i] = itemId;
                    break;
                }
            }
        }
    }

    private void RefreshSlots()
    {
        if (!categorySlotItems.TryGetValue(currentCategory, out int?[] items))
        {
            return;
        }

        for (int i = 0; i < slots.Count; i++)
        {
            if (i < items.Length && items[i].HasValue)
            {
                int itemId = items[i].Value;
                slots[i].SetSlot(gameManager.itemInfos[itemId]);
            }
            else
            {
                slots[i].SetSlot(null);
            }
        }
    }
}
