using System.Collections.Generic;
using UnityEngine;

public class ItemManager : Singleton<ItemManager>
{
    [SerializeField] Transform slotParent;
    public DiabolicItemInfo[] startPieceList;
    public DiabolicItemInfo[] nightPieceList;
    [HideInInspector] public DiabolicItemInfo[] pieceItemsList;
    [SerializeField] DiabolicItemInfo[] allPieceList;
    [SerializeField] DiabolicItemInfo[] testList;
    [SerializeField] AudioClip getPieceSound;

    [HideInInspector] public DiabolicItemInfo[] items;
    [HideInInspector] public int[] itemQuantity;
    [HideInInspector] public Dictionary<DiabolicItemInfo, int> getItems = new Dictionary<DiabolicItemInfo, int>();
    [HideInInspector] public Dictionary<DiabolicItemInfo, int> currentEquipItems = new Dictionary<DiabolicItemInfo, int>();

    SoundManager soundManager;

    protected override void Awake()
    {
        base.Awake();

        soundManager = SoundManager.Instance;

        pieceItemsList.Initialize();

        items = new DiabolicItemInfo[slotParent.childCount];
        itemQuantity = new int[slotParent.childCount];

#if UNITY_EDITOR
        /*for (int i = 0; i < testList.Length; ++i)
        {
            AddItem(testList[i]);
        }*/
#endif
    }

    public void AddItem(DiabolicItemInfo selectedItem)
    {
        int emptyIndex = -1;

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null && emptyIndex == -1)
                emptyIndex = i;

            if (items[i] == selectedItem)
            {
                itemQuantity[i]++;
                getItems[items[i]] = itemQuantity[i];
                return;
            }
        }

        items[emptyIndex] = selectedItem;
        itemQuantity[emptyIndex]++;

        getItems.Add(items[emptyIndex], itemQuantity[emptyIndex]);

        soundManager.PlaySFX(getPieceSound);
    }

    public void AddItem(int id)
    {
        for (int i = 0; i < allPieceList.Length; ++i)
        {
            if (allPieceList[i].ItemNum == id)
            {
                AddItem(allPieceList[i]);
                break;
            }
        }
    }

    public DiabolicItemInfo GetPieceInfo(int Id)
    {
        foreach (var piece in allPieceList) 
        {
            if (piece.ItemNum == Id)
            {
                return piece;
            }
        }

        Debug.LogWarning("해당 Id를 가지는 Piece가 없습니다.");
        return null;
    }
}
