using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PuzzleInventoryUI : MonoBehaviour
{
    [SerializeField] ItemType itemSort;
    [SerializeField] GameObject puzzlePieceObject;

    [SerializeField]List<Transform> slotpanels;

    private void Awake()
    {
        foreach(Transform slotpanel in transform)
        {
            slotpanels.Add(slotpanel);
        }
    }
    private void OnEnable()
    {
        UpdateInventoryUI();
    }
    public void UpdateInventoryUI()
    {
        int index = 0;
        GameManager gameManager = GameManager.Instance;
        foreach (var item in gameManager.haveItems)
        {
            if (gameManager.itemInfos[item.Key].itemType == (int)itemSort)
            {
                GameObject puzzlePiece = Instantiate(puzzlePieceObject, slotpanels[index]);
                if(puzzlePiece.TryGetComponent(out PuzzlePiece piece))
                {
                    piece.InitShellColor((ShellColor)gameManager.itemInfos[item.Key].itemId - 2030017);
                }

                index++;   
            }
        }
    }

}