
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemDictionay : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private GameObject InventoryView;
    [SerializeField] private Transform itemSlotParent;
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private GameObject itemMatSlotPrefab;

    [Header("Item Description")]
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemEffectText;
    [SerializeField] private TextMeshProUGUI itemAcquisition;
    [SerializeField] private Transform materialSlot;
    [SerializeField] private GameObject DiscriptionContents;

    [Header("Page")]
    [SerializeField] private Transform dictionaryIndexParent;
    [SerializeField] private GameObject dictionaryIndexSlot;
    [SerializeField] private Scrollbar scrollbar;

    private const int PageSize = 10;

    private readonly List<GameObject> itemSlot = new();
    private readonly List<Item> itemsByCategory = new();

    private List<Item> items;
    private ItemManager itemManager;

    private int currentCategory = -1;
    private int currentPage;

    private GameObject materialPanel;
    private Transform matStringParent;

    /// <summary>
    /// 아이템 데이터를 초기화하고 도감 슬롯을 생성한다.
    /// </summary>
    private void Start()
    {
        itemManager = ItemManager.Instance;

        if (itemManager == null || !itemManager.Initialize())
        {
            Debug.LogError("[ItemDictionay] ItemManager 초기화 실패", this);
            enabled = false;
            return;
        }

        items = itemManager.itemDatas;
        matStringParent = transform.Find("Scroll View/DescriptionPanel/Contents/MatSlots");

        for (int i = 0; i < PageSize; i++)
        {
            GameObject slot = Instantiate(itemSlotPrefab, itemSlotParent);
            itemSlot.Add(slot);
        }

        ChangeCategory(0);

        if (InventoryView != null)
            InventoryView.SetActive(false);
    }

    /// <summary>
    /// 선택한 카테고리의 아이템 목록을 구성한다.
    /// </summary>
    public void ChangeCategory(int category)
    {
        if (items == null)
            return;

        if (currentCategory == category)
            return;

        currentCategory = category;
        currentPage = 0;

        itemsByCategory.Clear();

        foreach (Item item in items)
        {
            if ((int)item.Type == category)
                itemsByCategory.Add(item);
        }

        RefreshPageButtons();
        ChangeDictionaryIndex(0);
    }

    /// <summary>
    /// 도감 페이지 버튼을 생성하고 갱신한다.
    /// </summary>
    private void RefreshPageButtons()
    {
        if (dictionaryIndexParent == null || dictionaryIndexSlot == null)
            return;

        int pageCount = Mathf.CeilToInt(itemsByCategory.Count / (float)PageSize);

        for (int i = 0; i < dictionaryIndexParent.childCount; i++)
            dictionaryIndexParent.GetChild(i).gameObject.SetActive(false);

        for (int i = 0; i < pageCount; i++)
        {
            GameObject pageObject;

            if (i < dictionaryIndexParent.childCount)
                pageObject = dictionaryIndexParent.GetChild(i).gameObject;
            else
                pageObject = Instantiate(dictionaryIndexSlot, dictionaryIndexParent);

            pageObject.SetActive(true);

            TextMeshProUGUI pageText = pageObject.GetComponentInChildren<TextMeshProUGUI>();

            if (pageText != null)
                pageText.text = (i + 1).ToString();

            Button button = pageObject.GetComponentInChildren<Button>();

            if (button != null)
            {
                int pageIndex = i;

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => ChangeDictionaryIndex(pageIndex));
            }
        }
    }

    /// <summary>
    /// 선택한 페이지에 해당하는 아이템 슬롯을 표시한다.
    /// </summary>
    public void ChangeDictionaryIndex(int pageIndex)
    {
        if (itemsByCategory == null)
            return;

        int pageCount = Mathf.CeilToInt(itemsByCategory.Count / (float)PageSize);

        if (pageIndex < 0 || pageIndex >= pageCount)
        {
            if (pageCount == 0)
            {
                ClearItemSlots();

                if (DiscriptionContents != null)
                    DiscriptionContents.SetActive(false);
            }

            return;
        }

        currentPage = pageIndex;

        if (scrollbar != null)
            scrollbar.value = 1f;

        for (int i = 0; i < itemSlot.Count; i++)
        {
            int itemIndex = currentPage * PageSize + i;

            if (itemIndex >= itemsByCategory.Count)
            {
                itemSlot[i].SetActive(false);
                continue;
            }

            Item item = itemsByCategory[itemIndex];

            GameObject slot = itemSlot[i];
            slot.SetActive(true);

            Transform spriteTransform = slot.transform.Find("ItemImage/Sprite");
            Transform nameTransform = slot.transform.Find("ItemName");

            if (spriteTransform != null)
            {
                Image image = spriteTransform.GetComponent<Image>();

                if (image != null)
                    image.sprite = Resources.Load<Sprite>($"Item/{item.ImageId}");
            }

            if (nameTransform != null)
            {
                TextMeshProUGUI nameText = nameTransform.GetComponent<TextMeshProUGUI>();

                if (nameText != null)
                    nameText.text = item.ItemName;
            }
        }

        // 현재 페이지의 첫 번째 아이템 설명 표시
        ChangeItemInfoDescriptionText(0);
    }

    /// <summary>
    /// 모든 아이템 슬롯을 비활성화한다.
    /// </summary>
    private void ClearItemSlots()
    {
        foreach (GameObject slot in itemSlot)
        {
            if (slot != null)
                slot.SetActive(false);
        }
    }

    /// <summary>
    /// 선택한 아이템의 설명과 필요 재료를 표시한다.
    /// index는 현재 페이지의 슬롯 번호(0~9)이다.
    /// </summary>
    public void ChangeItemInfoDescriptionText(int index)
    {
        int itemIndex = currentPage * PageSize + index;

        if (index < 0 || index >= PageSize ||
            itemIndex < 0 || itemIndex >= itemsByCategory.Count)
        {
            if (DiscriptionContents != null)
                DiscriptionContents.SetActive(false);

            return;
        }

        Item item = itemsByCategory[itemIndex];

        if (DiscriptionContents != null)
            DiscriptionContents.SetActive(true);

        if (itemImage != null)
            itemImage.sprite = Resources.Load<Sprite>($"Item/{item.ImageId}");

        if (itemNameText != null)
            itemNameText.text = item.ItemName;

        if (itemEffectText != null)
            itemEffectText.text = item.ItemEffect;

        if (itemAcquisition != null)
        {
            itemAcquisition.text = "획득 장소: ";

            if (item.takeTimeByAcquisition != null)
            {
                foreach (var acquisition in item.takeTimeByAcquisition)
                {
                    if (itemManager.aquisitionName.TryGetValue(acquisition.Key, out string name))
                        itemAcquisition.text += $"{name} ";
                }
            }
        }

        RefreshMaterialSlots(item);
    }

    /// <summary>
    /// 선택한 아이템에 필요한 재료 UI를 갱신한다.
    /// </summary>
    private void RefreshMaterialSlots(Item item)
    {
        if (materialSlot == null)
            return;

        // 기존 재료 슬롯 비활성화
        for (int i = 0; i < materialSlot.childCount; i++)
            materialSlot.GetChild(i).gameObject.SetActive(false);

        // 기존 재료 설명 텍스트 비활성화
        if (matStringParent != null)
        {
            for (int i = 0; i < matStringParent.childCount; i++)
                matStringParent.GetChild(i).gameObject.SetActive(false);
        }

        if (item.NeedMaterials == null)
            return;

        int count = 0;

        foreach (var mat in item.NeedMaterials)
        {
            if (!itemManager.idByMaterialType.TryGetValue(mat.Key, out int materialId))
                continue;

            ItemInfo materialInfo = itemManager.GetItemInfo(materialId);

            if (materialInfo == null)
                continue;

            GameObject slot;

            if (count < materialSlot.childCount)
                slot = materialSlot.GetChild(count).gameObject;
            else
                slot = Instantiate(itemMatSlotPrefab, materialSlot);

            slot.SetActive(true);

            Transform imageTransform = slot.transform.Find("MatImage");
            Transform nameTransform = slot.transform.Find("MatName");
            Transform countTransform = slot.transform.Find("MatCount");

            if (imageTransform != null)
                imageTransform.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Item/{materialId}");

            if (nameTransform != null)
                nameTransform.GetComponent<Text>().text = materialInfo.itemName;

            if (countTransform != null)
                countTransform.GetComponent<Text>().text = $"x {mat.Value}";

            RefreshMaterialText(count, materialId, materialInfo.itemName);

            count++;
        }
    }

    /// <summary>
    /// 필요 재료 이름을 생성하고 마우스 호버 이벤트를 연결한다.
    /// </summary>
    private void RefreshMaterialText(int index, int materialId, string materialName)
    {
        if (matStringParent == null)
            return;

        GameObject textObject;
        TextMeshProUGUI text;

        if (index < matStringParent.childCount)
        {
            textObject = matStringParent.GetChild(index).gameObject;
            text = textObject.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            textObject = new GameObject($"Material{index}", typeof(RectTransform));
            textObject.transform.SetParent(matStringParent, false);

            text = textObject.AddComponent<TextMeshProUGUI>();
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 40f;
        }

        if (text == null)
            return;

        textObject.SetActive(true);
        text.text = $"[{materialName}]";
        text.raycastTarget = true;

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(text.preferredWidth, 54f);

        EventTrigger trigger = textObject.GetComponent<EventTrigger>();

        if (trigger == null)
            trigger = textObject.AddComponent<EventTrigger>();

        // 기존 이벤트를 지우고 현재 재료에 맞게 다시 등록
        trigger.triggers.Clear();

        EventTrigger.Entry enterEntry = new();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener(data => MouseOverOnMaterialString(materialId));

        EventTrigger.Entry exitEntry = new();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener(data => MouseExitOnMaterialString(materialId));

        trigger.triggers.Add(enterEntry);
        trigger.triggers.Add(exitEntry);
    }

    /// <summary>
    /// 재료 이름에 마우스를 올리면 상세 설명 패널을 표시한다.
    /// </summary>
    private void MouseOverOnMaterialString(int materialId)
    {
        if (itemManager == null)
            return;

        Item item = itemManager.itemDatas.FirstOrDefault(x => x.ItemId == materialId);

        if (item == null)
            return;

        if (materialPanel == null)
        {
            GameObject prefab = Resources.Load<GameObject>("Panels/MatDescriptionPanel");

            if (prefab == null)
            {
                Debug.LogWarning("[ItemDictionay] MatDescriptionPanel 프리팹이 없습니다.", this);
                return;
            }

            materialPanel = Instantiate(prefab, transform);
        }

        Transform image = materialPanel.transform.Find("ItemImage");
        Transform name = materialPanel.transform.Find("ItemName");
        Transform effect = materialPanel.transform.Find("ItemEffect");
        Transform acquisition = materialPanel.transform.Find("ItemAquisition");

        if (image != null)
            image.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Item/{item.ImageId}");

        if (name != null)
            name.GetComponent<TextMeshProUGUI>().text = item.ItemName;

        if (effect != null)
            effect.GetComponent<TextMeshProUGUI>().text = item.ItemEffect;

        if (acquisition != null)
        {
            TextMeshProUGUI text = acquisition.GetComponent<TextMeshProUGUI>();
            text.text = "획득 장소: ";

            if (item.takeTimeByAcquisition != null)
            {
                foreach (var entry in item.takeTimeByAcquisition)
                {
                    if (itemManager.aquisitionName.TryGetValue(entry.Key, out string acquisitionName))
                        text.text += $"{acquisitionName} ";
                }
            }
        }

        materialPanel.SetActive(true);
    }

    /// <summary>
    /// 재료 설명 패널을 숨긴다.
    /// </summary>
    private void MouseExitOnMaterialString(int materialId)
    {
        if (materialPanel != null)
            materialPanel.SetActive(false);
    }
}
