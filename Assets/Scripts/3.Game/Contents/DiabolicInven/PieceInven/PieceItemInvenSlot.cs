using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PieceItemInvenSlot : MonoBehaviour
{
    [SerializeField] Image itemImage;
    [SerializeField] Image GradeColorImage;
    [SerializeField] Image blockImage;
    
    Text itemDiscriptionText;
    PieceItemInven pieceInven;
    DragUI dragUI;
    ItemManager itemManager;

    int indexNum;

    Dictionary<Status,int> status = new Dictionary<Status,int>();

    private void Awake()
    {
        pieceInven = PieceItemInven.Instance;
        dragUI = DragUI.Instance;
        itemManager = ItemManager.Instance;

        indexNum = transform.GetSiblingIndex();

        blockImage.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        UpdateSlotData();
        ChangeSlotColor();
    }

    void ChangeSlotColor()
    {
        if (pieceInven.itemQuantity[indexNum] == 0)
        {
            GradeColorImage.gameObject.SetActive(false);
        }

        else
        {
            GradeColorImage.gameObject.SetActive(true);
            GradeColorImage.color = PieceCard.GradeColors[(pieceInven.items[indexNum].MaxCount - 1) - (pieceInven.itemQuantity[indexNum] - 1)];
        }
    }

    public void UpdateSlotData()
    {
        if (pieceInven == null)
        {
            pieceInven = PieceItemInven.Instance;
        }

        if (pieceInven.items[indexNum] == null)
        {
            itemImage.gameObject.SetActive(false);
        }

        else
        {
            itemImage.sprite = pieceInven.items[indexNum].ItemSprite;
            itemImage.gameObject.SetActive(true);
        }
    }

    public void UpdateDiscription()
    {
        if (dragUI == null)
            dragUI = DragUI.Instance;

        if (dragUI.isDragging)
            return;

        if (pieceInven.items[indexNum] == null)
        {
            return;
        }

        if(itemDiscriptionText == null)
            itemDiscriptionText = pieceInven.DiscriptionPanel.GetComponent<Text>();

        itemDiscriptionText.text = pieceInven.items[indexNum].ItemName + "\n";

        status = pieceInven.items[indexNum].Stat();

        for (int i = 0; i < status.Count; i++)
        {
            if (status[(Status)i] == 0)
                continue;

            itemDiscriptionText.text += "\n" + GameManager.Instance.statNames[i] + ": +" + status[(Status)i] * itemManager.itemQuantity[indexNum];
        }

        itemDiscriptionText.text += "\n" + pieceInven.items[indexNum].SpecialStatInfo;
        itemDiscriptionText.text += "\n\n" + pieceInven.items[indexNum].Description;
    }

    public void DragBeginItem()
    {
        if(dragUI == null)
            dragUI = DragUI.Instance;

        if (pieceInven.items[indexNum] == null || dragUI.isDragging || blockImage.gameObject.activeSelf)
            return;

        dragUI.SettingDragUI(pieceInven.items[indexNum]);
        dragUI.isDragging = true;
    }

    public void DraggingItem()
    {
        if (!dragUI.isDragging)
            return;

        dragUI.MoveDragUI();
    }

    public void DragEndItem()
    {
        if (!dragUI.isDragging)
            return;

        DiabolicInven.Instance.InstantItemImage(dragUI.DragItem, blockImage.gameObject, indexNum, pieceInven.itemQuantity[indexNum]);

        dragUI.OffDragUI();
        dragUI.isDragging = false;
    }

    public void OffBlockImage()
    {
        blockImage.gameObject.SetActive(false);
    }
}
