using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PuzzleSlot : ItemSlotUI
{
    [SerializeField] int puzzleIndex = -1;
    public int PuzzleIndex => puzzleIndex;

    public void SetIndex(int newIndex)
    {
        puzzleIndex = newIndex;
    }
}
