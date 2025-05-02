using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemDictionay : MonoBehaviour
{
    [SerializeField] GameObject InventoryView;
    [SerializeField] Transform itemSlotParent;
    [SerializeField] GameObject itemSlotPrefab;
    [SerializeField] GameObject itemMatSlotPrefab;
    [SerializeField] Image itemImage;
    [SerializeField] TextMeshProUGUI itemNameText;
    [SerializeField] TextMeshProUGUI itemEffectText;
    [SerializeField] TextMeshProUGUI itemAcquisition;
    [SerializeField] Transform materialSlot;
    [SerializeField] Transform dictionaryIndexParent;
    [SerializeField] GameObject dictionaryIndexSlot;
    [SerializeField] Scrollbar scrollbar;
    [SerializeField] GameObject DiscriptionContents;

    int currentCategory;

    List<GameObject> itemSlot = new List<GameObject>();

    List<Item> items;
    List<Item> itemsByCategory;


    private void Start()
    {
        items = GameManager.Instance.itemDatas;
        itemsByCategory = new List<Item>();

        currentCategory = -1;

        for (int i = 0; i < 10; i++)
        {
            itemSlot.Add(Instantiate(itemSlotPrefab, itemSlotParent));
        }

        /*int index = 0;

        for (int i = 0; i < items.Count; ++i)
        {
            if ((int)items[i].Type == currentCategory)
            {
                itemSlot[index].SetActive(true);
                itemSlot[index].transform.Find("ItemImage/Sprite").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Item/{items[i].ImageId}");
                itemSlot[index].transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = items[i].ItemName;
                index++;
            }

            if (index == 10)
                break;
            Transform matSlotParent = itemSlot.transform.Find("MatSlots");

            foreach (var mat in items[i].NeedMaterials)
            {
                GameObject matSlot = Instantiate(itemMatSlotPrefab, matSlotParent);
                matSlot.transform.Find("MatImage").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Item/{GameManager.Instance.idByMaterialType[mat.Key]}");
                matSlot.transform.Find("MatCount").GetComponent<Text>().text = $"x{mat.Value}";
            }
        }

        for (int i = index; i < 10; i++)
        {
            itemSlot[i].SetActive(false);
        }*/

        ChangeCategory(0);
        ChangeItemInfoDescriptionText(0);

        InventoryView.SetActive(false);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.I))
            InventoryView.SetActive(!InventoryView.activeSelf);
    }

    public void ChangeCategory(int category)
    {
        if (currentCategory == category) return;

        itemsByCategory.Clear();

        int index = 0;
        int dicIndex = 1;

        for (int i = 0; i < dictionaryIndexParent.childCount; ++i)
        {
            dictionaryIndexParent.GetChild(i).gameObject.SetActive(false);
        }

        for (int i = 0; i < items.Count; ++i)
        {
            if ((int)items[i].Type == category)
            {
                itemsByCategory.Add(items[i]);
                if (dicIndex == 1)
                {
                    itemSlot[index].SetActive(true);
                    itemSlot[index].transform.Find("ItemImage/Sprite").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Item/{items[i].ImageId}");
                    itemSlot[index].transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = items[i].ItemName;
                }
                index++;
            }

            if (index >= 10 && index % 10 == 0)
            {
                if (dictionaryIndexParent.childCount < dicIndex)
                {
                    GameObject slot = Instantiate(dictionaryIndexSlot, dictionaryIndexParent);
                    slot.transform.GetComponentInChildren<TextMeshProUGUI>().text = dicIndex.ToString();
                    slot.transform.GetComponentInChildren<Button>().onClick.AddListener(() => ChangeDictionaryIndex(slot.transform.GetSiblingIndex()));
                }

                else
                    dictionaryIndexParent.GetChild(dicIndex - 1).gameObject.SetActive(true);

                dicIndex++;
            }
        }

        for (int i = index; i < 10; i++)
        {
            itemSlot[i].SetActive(false);
        }

        currentCategory = category;
    }

    public void ChangeDictionaryIndex(int dicIndex)
    {
        scrollbar.value = 1;

        for (int i = 0; i < 10; i++)
        {
            if (itemsByCategory.Count > i + dicIndex * 10)
            {
                itemSlot[i].SetActive(true);
                itemSlot[i].transform.Find("ItemImage/Sprite").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Item/{itemsByCategory[i + dicIndex * 10].ImageId}");
                itemSlot[i].transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = itemsByCategory[i + dicIndex * 10].ItemName;
            }

            else
                itemSlot[i].SetActive(false);
        }
    }

    Item overlapedItem;

    public void ChangeItemInfoDescriptionText(int index)
    {
        DiscriptionContents.SetActive(true);

        itemImage.sprite = Resources.Load<Sprite>($"Item/{itemsByCategory[index].ItemId}");
        itemNameText.text = itemsByCategory[index].ItemName;
        itemEffectText.text = itemsByCategory[index].ItemEffect;
        itemAcquisition.text = "획득 장소: ";
        foreach (var aquisition in itemsByCategory[index].takeTimeByAcquisition)
        {
            itemAcquisition.text += $"{GameManager.Instance.aquisitionName[aquisition.Key]} ";
        }

        int count = 0;

        Transform matStringParent = transform.Find("Scroll View/DescriptionPanel/Contents/MatSlots");

        for (int i = 0; i < matStringParent.childCount; i++)
            matStringParent.GetChild(i).gameObject.SetActive(false);

        foreach (var mat in itemsByCategory[index].NeedMaterials)
        {
            GameObject matSlot = null;

            if (materialSlot.childCount <= count)
                matSlot = Instantiate(itemMatSlotPrefab, materialSlot);
            else
            {
                matSlot = materialSlot.GetChild(count).gameObject;
                matSlot.SetActive(true);
            }

            matSlot.transform.Find("MatImage").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Item/{GameManager.Instance.idByMaterialType[mat.Key]}");
            matSlot.transform.Find("MatName").GetComponent<Text>().text = $"{GameManager.Instance.itemInfos[GameManager.Instance.idByMaterialType[mat.Key]].itemName}";
            matSlot.transform.Find("MatCount").GetComponent<Text>().text = $"x {mat.Value}";

            GameObject matString = null;
            TextMeshProUGUI matName = null;
            if (matStringParent.childCount <= count)
            {
                matString = new GameObject($"Material{count}");
                matString.transform.parent = matStringParent;
                matString.transform.localScale = Vector3.one;
                matString.transform.localRotation = Quaternion.identity;

                matName = matString.AddComponent<TextMeshProUGUI>();
                matName.alignment = TextAlignmentOptions.Center;
                matName.fontSize = 40f;
            }

            else
            {
                matString = matStringParent.GetChild(count).gameObject;
                matName = matString.GetComponent<TextMeshProUGUI>();
                matString.SetActive(true);
            }

            matName.text = $"[{GameManager.Instance.itemInfos[GameManager.Instance.idByMaterialType[mat.Key]].itemName}]";

            Vector2 textSize = matName.GetComponent<RectTransform>().sizeDelta;
            textSize.x = matName.preferredWidth;
            textSize.y = 54;
            matName.GetComponent<RectTransform>().sizeDelta = textSize;

            EventTrigger eventTrigger = matString.AddComponent<EventTrigger>();

            EventTrigger.Entry enterEntry = new EventTrigger.Entry();
            enterEntry.eventID = EventTriggerType.PointerEnter;
            enterEntry.callback.AddListener((data) => { MouseOverOnMaterialString(GameManager.Instance.idByMaterialType[mat.Key]); });

            EventTrigger.Entry exitEntry = new EventTrigger.Entry();
            exitEntry.eventID = EventTriggerType.PointerExit;
            exitEntry.callback.AddListener((data) => { MouseExitOnMaterialString(GameManager.Instance.idByMaterialType[mat.Key]); });

            eventTrigger.triggers.Add(enterEntry);
            eventTrigger.triggers.Add(exitEntry);

            for (int i = itemsByCategory[index].NeedMaterials.Count; i < materialSlot.childCount; ++i)
            {
                materialSlot.GetChild(i).gameObject.SetActive(false);
            }

            count++;
        }

        DiscriptionContents.SetActive(true);
    }

    GameObject materialPanelPrefab;
    GameObject materialPanel;

    void MouseOverOnMaterialString(int matId)
    {

        Item item = GameManager.Instance.itemDatas.Where(x => x.ItemId == matId).First();

        if (materialPanel == null)
        {
            materialPanelPrefab = Resources.Load<GameObject>("Panels/MatDescriptionPanel");
            materialPanel = Instantiate(materialPanelPrefab, transform);
        }

        materialPanel.transform.Find("ItemImage").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Item/{item.ImageId}");
        materialPanel.transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = item.ItemName;
        materialPanel.transform.Find("ItemEffect").GetComponent<TextMeshProUGUI>().text = item.ItemEffect;
        materialPanel.transform.Find("ItemAquisition").GetComponent<TextMeshProUGUI>().text = "획득 장소: ";

        foreach (var aquisition in item.takeTimeByAcquisition)
        {
            materialPanel.transform.Find("ItemAquisition").GetComponent<TextMeshProUGUI>().text += $"{GameManager.Instance.aquisitionName[aquisition.Key]} ";
        }

        /*RectTransform rt = materialPanel.GetComponent<RectTransform>();
        Vector2 panelPos;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, Input.mousePosition, Camera.main, out panelPos);

        rt.localPosition = panelPos;*/

        materialPanel.SetActive(true);
    }

    void MouseExitOnMaterialString(int matId)
    {
        materialPanel.SetActive(false);
    }
}
