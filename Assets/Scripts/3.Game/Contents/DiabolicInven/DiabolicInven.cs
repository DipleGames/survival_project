using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiabolicInven : Singleton<DiabolicInven>
{
    [SerializeField] Transform slotParent;
    [SerializeField] Transform itemImageParent;
    [SerializeField] GameObject itemImage;
    [SerializeField] AudioClip cantEquipSound;

    DiabolicInvenSlot[] slots;

    public int currentIndex;

    DragUI dragUI;
    GameManager gameManager;
    Character character;
    ItemManager itemManager;
    SoundManager soundManager;

    int width;
    int height;
    bool[] itemShape;

    int column;
    int row;

    public bool canSetImage;

    Dictionary<Status,int> itemStatus;
    Dictionary<SpecialStatus, bool> itemSpecialStatus;

    private void Start()
    {
        gameManager = GameManager.Instance;
        dragUI = DragUI.Instance;
        character = Character.Instance;
        itemManager = ItemManager.Instance;
        soundManager = SoundManager.Instance;

        slots = new DiabolicInvenSlot[slotParent.childCount];

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = slotParent.GetChild(i).GetComponent<DiabolicInvenSlot>();
        }
    }

    public void InstantItemImage(DiabolicItemInfo item, GameObject blockImage, int pieceSlotIndex, int itemQuantity)
    {
        OffSlotSetImage();

        if (!canSetImage)
        {
            soundManager.PlaySFX(cantEquipSound);
            return;
        }

        Vector3 setPos = slots[currentIndex].GetComponent<RectTransform>().localPosition;

        //setPos = new Vector3(setPos.x - 3, setPos.y- 39, setPos.z);

        soundManager.PlaySFX(item.EquipSound);

        Instantiate(itemImage, itemImageParent).GetComponent<DiabolicSlotItem>().
            ItemSetOnInventory(setPos, item, TransferIndexesNum(), currentIndex, pieceSlotIndex, itemQuantity);
        blockImage.SetActive(true);

        AddStatus(pieceSlotIndex);

        SetSlotIsEmpty(height, width, itemShape, row, column, false);

        if (!itemManager.currentEquipItems.ContainsKey(dragUI.DragItem))
            itemManager.currentEquipItems.Add(dragUI.DragItem, 1);

        else
            itemManager.currentEquipItems[dragUI.DragItem]++;

        canSetImage = false;
    }

    public void AddStatus(int pieceSlotIndex)
    {
        itemStatus = dragUI.DragItem.Stat();
        itemSpecialStatus = dragUI.DragItem.SpecialStat();

        for (int i = 0; i < gameManager.status.Count; i++)
        {
            if (itemStatus[(Status)i] == 0)
                continue;

            gameManager.status[(Status)i] += itemStatus[(Status)i] * (itemManager.itemQuantity[pieceSlotIndex]);
        }

        for (int i = 0; i < gameManager.specialStatus.Count; ++i)
        {
            if (itemSpecialStatus[(SpecialStatus)i])
                gameManager.specialStatus[(SpecialStatus)i] = true;
        }

        character.UpdateStat();
    }

    public void ChagneCanSetImage(bool _canSetImage)
    {
        canSetImage = _canSetImage;
    }

    void CheckCanSetImage()
    {
        width = dragUI.DragItem.ItemShape.Width;
        height = dragUI.DragItem.ItemShape.Height;
        itemShape = dragUI.DragItem.ItemShape.Shape;

        column = currentIndex % 4;
        row = currentIndex / 4;

        canSetImage = (column + width) <= 4 && (row + height) <= 4;

        if (!canSetImage)
            return;

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j <  width; j++)
            {
                if (itemShape[i * width + j] && !slots[(i + row) * 4 + j + column].GetComponent<DiabolicInvenSlot>().IsEmpty)
                {
                    canSetImage = false;
                    return;
                }
            }
        }
    }

    public void ChangeSlotsColor()
    {
        CheckCanSetImage();

        for (int i = row; i < Mathf.Clamp(row + height, row, 4); i++)
        {
            for (int j = column; j < Mathf.Clamp(column + width, column, 4); j++)
            {
                if (itemShape[(i - row) * width + (j - column)])
                {
                    slots[i * 4 + j].GetComponent<DiabolicInvenSlot>().ChangeCanSetImageColor(canSetImage);
                }
            }
        }
    }

    public void SetSlotIsEmpty(int _height, int _width, bool[] _itemShape,int _row, int _column, bool slotEmpty)
    {
        for (int i = 0; i < _height; i++)
        {
            for (int j = 0; j < _width; j++)
            {
                if (_itemShape[i * _width + j])
                {
                    slots[(i + _row) * 4 + j + _column].GetComponent<DiabolicInvenSlot>().ChagneEmptyState(slotEmpty);
                }
            }
        }
    }

    List<int> TransferIndexesNum()
    {
        List<int> indexes = new List<int>();

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (itemShape[i * width + j])
                {
                    indexes.Add((i + row) * 4 + j + column);
                }
            }
        }

        return indexes;
    }

    public void OffSlotSetImage()
    {
        for (int i = row; i < Mathf.Clamp(row + height, row, 4); i++)
        {
            for (int j = column; j < Mathf.Clamp(column + width, column, 4); j++)
            {
                if (itemShape[(i - row) * width + (j - column)])
                {
                    slots[i * 4 + j].GetComponent<DiabolicInvenSlot>().OffCanSetImage();
                }
            }
        }
    }
}
