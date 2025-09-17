using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PuzzleInventoryUI : MonoBehaviour
{
    [SerializeField] ItemType type;
    [SerializeField] GameObject puzzlePieceObject;

    List<Transform> slotpanels = new List<Transform>();

    private void Awake()
    {
        foreach(Transform slotpanel in transform)
        {
            slotpanels.Add(slotpanel);
        }
    }
    private void Start()
    {
        UpdatePuzzleInventoryUI();
    }
    private void UpdatePuzzleInventoryUI()
    {
        int index = 0;
        GameManager gameManager = GameManager.Instance;

        foreach (var item in gameManager.haveItems)
        {
            if (gameManager.Items[item.Key].Type == type)
            {
                GameObject puzzlePiece = Instantiate(puzzlePieceObject, slotpanels[index]);
                if(puzzlePiece.TryGetComponent(out PuzzlePiece piece))
                {
                    piece.InitShellColor((ShellColor)gameManager.Items[item.Key].ItemId);
                }

                index++;   
            }
        }
    }

}