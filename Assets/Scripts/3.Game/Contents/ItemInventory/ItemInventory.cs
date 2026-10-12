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
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
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
        int index = 0;

        foreach (var item in gameManager.haveItems)
        {
            if (gameManager.itemInfos[item.Key].itemType == type)
            {
                slots[index].SetSlot(gameManager.itemInfos[item.Key]);
                index++;
            }
        }

        for (int i = index; i < slots.Count; i++)
        {
            slots[i].SetSlot(null);
        }

        currentCategory = type;
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
}
