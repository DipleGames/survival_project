using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreatePanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI itemName;
    [SerializeField] Image itemImage;
    [SerializeField] Transform itemMaterialSlot;
    [SerializeField] GameObject createMaterialSlotPrefab;
    [SerializeField] TextMeshProUGUI createCountText;
    [SerializeField] Transform createItemListParent;
    [SerializeField] GameObject createItemSlotPrefab;
    [SerializeField] GameObject CreateSuccessPanel;

    GameManager gameManager;

    Item item;

    int createCount = 1;
    bool canCreate = false;

    Acquisition acquisition;

    int createType = 0;

    private void Awake()
    {
        gameManager = GameManager.Instance;
        createCountText.text = $"제작 개수:\t{createCount}";
    }

    private void OnEnable()
    {
        createType = 0;

        UpdateCreateItemList(createType);
        SetCreateItemPanelInfo();
    }

    public void SetCreateAcquisition(Acquisition acquisition)
    {
        this.acquisition = acquisition;
    }

    public void SelectCreateItemType(int num)
    {
        createType = num;

        UpdateCreateItemList(createType);
    }

    public void UpdateCreateItemList(int createType)
    {
        Item[] createItemList = gameManager.itemDatas.Where(x => x.AcquisitionList.Contains(acquisition)).ToArray();

        for (int i = 0; i < createItemListParent.childCount; ++i)
            createItemListParent.GetChild(i).gameObject.SetActive(false);

        for (int i = 0; i < createItemList.Length; i++)
        {
            if (createItemList[i].Type != (ItemType)createType)
                continue;

            GameObject itemList = null;

            if (createItemListParent.childCount <= i)
            {
                itemList = Instantiate(createItemSlotPrefab, createItemListParent);
                Button clickButton = itemList.transform.Find("Button").GetComponent<Button>();
                int num = i;    // Closure problem
                clickButton.onClick.AddListener(() => item = createItemList[num]);
                clickButton.onClick.AddListener(SetCreateItemPanelInfo);
            }
            else
            {
                itemList = createItemListParent.GetChild(i).gameObject;
                createItemListParent.GetChild(i).gameObject.SetActive(true);
            }

            itemList.transform.Find("ItemImage").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Item/{createItemList[i].ImageId}");
            itemList.transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = createItemList[i].ItemName;
        }

        item = createItemList[0];
    }

    public void SetCreateItemPanelInfo()
    {
        itemName.text = item.ItemName;
        itemImage.sprite = Resources.Load<Sprite>($"Item/{item.ImageId}");

        canCreate = true;

        int index = 0;

        for(int i = 0; i< itemMaterialSlot.childCount; ++i)
            itemMaterialSlot.GetChild(i).gameObject.SetActive(false);

        foreach (var material in item.NeedMaterials)
        {
            GameObject materialPrefab = null;

            if (itemMaterialSlot.childCount <= index)
                materialPrefab = Instantiate(createMaterialSlotPrefab, itemMaterialSlot);
            else
            {
                materialPrefab = itemMaterialSlot.GetChild(index).gameObject;
                materialPrefab.SetActive(true);
            }

            materialPrefab.transform.Find("MatImage").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Item/{GameManager.Instance.idByMaterialType[material.Key]}");
            TextMeshProUGUI countText = materialPrefab.transform.Find("Count").GetComponent<TextMeshProUGUI>();
            int haveItemAmount = !gameManager.haveItems.ContainsKey(gameManager.idByMaterialType[material.Key]) ? 0 : gameManager.haveItems[gameManager.idByMaterialType[material.Key]];
            countText.text = $"{material.Value * createCount} / {haveItemAmount}";
            countText.color = haveItemAmount < material.Value * createCount ? Color.red : Color.white;

            if (canCreate)
                canCreate = haveItemAmount >= material.Value * createCount;

            index++;
        }
    }

    public void UpDownCreateCount(int num)
    {
        createCount += num;

        if (createCount < 1)
        {
            createCount = 1;
            return;
        }

        SetCreateItemPanelInfo();
        createCountText.text = $"제작 개수:\t{createCount}";
    }

    public void CreateItem()
    {
        //int index = transform.GetSiblingIndex();

        if (!canCreate || createCount <= 0)
            return;

        foreach (var material in item.NeedMaterials)
        {
            if (!gameManager.haveItems.ContainsKey(gameManager.idByMaterialType[material.Key]))
                return;

            else if (gameManager.haveItems[gameManager.idByMaterialType[material.Key]] < material.Value)
                return;
        }

        foreach (var material in item.NeedMaterials)
        {
            gameManager.haveItems[gameManager.idByMaterialType[material.Key]] -= material.Value * createCount;
            Debug.Log($"{gameManager.Items[GameManager.Instance.idByMaterialType[material.Key]].ItemName}: {material.Value * createCount}개 소모");
        }

        //item.AddItem();
        GameManager.Instance.haveItems[item.ItemId] += createCount;

        SetCreateItemPanelInfo();

        CreateSuccessPanel.transform.Find("BackPanel/ItemImage").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Item/{item.ImageId}");
        CreateSuccessPanel.transform.Find("BackPanel/CreateCount").GetComponent<TextMeshProUGUI>().text = $"{item.ItemName}\n{createCount}개 제작";
        CreateSuccessPanel.SetActive(true);
    }

    public void CreateItem(Item item)
    {
        foreach (var material in item.NeedMaterials)
        {
            if (!gameManager.haveMaterials.ContainsKey(material.Key))
                return;

            else if (gameManager.haveMaterials[material.Key] < material.Value)
                return;
        }

        foreach (var material in item.NeedMaterials)
        {
            gameManager.haveMaterials[material.Key] -= material.Value;
            Debug.Log($"{gameManager.Items[GameManager.Instance.idByMaterialType[material.Key]].ItemName}: {material.Value}개 소모");
        }

        item.AddItem();
        Debug.Log($"{item.ItemName}을 제작했습니다.");
    }

    public void ChangeItemList(Acquisition aquisition)
    {
        List<Item> items = new List<Item>();

        foreach (var item in gameManager.Items)
        {
            List<Acquisition> aquisitions = item.Value.AcquisitionList;

            for (int i = 0; i < aquisitions.Count; i++)
            {
                if (aquisitions[i] == aquisition)
                {
                    items.Add(item.Value);
                }
            }
        }
    }
}
