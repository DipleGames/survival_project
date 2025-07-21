using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemInventory : MonoBehaviour
{
    [SerializeField] GameObject descriptionPanel;
    [SerializeField] Transform slotParent;
    [SerializeField] GameObject slotPrefab;
    [SerializeField] GameObject invenPanel;
    [SerializeField] GameObject dragSlot;

    public GameObject DragSlot => dragSlot;

    int currentCategory;
    List<ItemInvenSlot> slots;

    GameManager gameManager;

    private void Awake()
    {
        slots = new List<ItemInvenSlot>();
        gameManager = GameManager.Instance;

        currentCategory = -1;

        for (int i = 0; i < 40; i++)
        {
            ItemInvenSlot slot = Instantiate(slotPrefab, slotParent).GetComponent<ItemInvenSlot>();
            slot.inventory = this;
            slots.Add(slot);
        }

        descriptionPanel.GetComponent<Text>().text = "";

        ChangeCategory(0);

        invenPanel.SetActive(false);    
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            invenPanel.SetActive(!invenPanel.activeSelf);

            if(invenPanel.activeSelf)
                ChangeCategory(currentCategory);
        }
    }

    public void ChangeCategory(int type)
    {
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
}
