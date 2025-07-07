using System.Collections.Generic;
using UnityEngine;

public class PuzzleController : MonoBehaviour
{
    [SerializeField] Puzzle_Beach puzzleBeach;
    [SerializeField] List<PuzzleSlot> puzzleSlots;

    private void Start()
    {
        if (puzzleBeach == null)
        {
            puzzleBeach = GetComponent<Puzzle_Beach>();
        }
        if (puzzleSlots == null || puzzleSlots.Count == 0)
        {
            puzzleSlots = new List<PuzzleSlot>(GetComponentsInChildren<PuzzleSlot>());
        }

    }
    private void OnEnable()
    {
        ItemSlotUI.OnItemDropped += HandleSlotChanged;        
    }

    private void OnDisable()
    {
        ItemSlotUI.OnItemDropped -= HandleSlotChanged;
    }

    private void HandleSlotChanged(Transform fromSlot, Transform toSlot)
    {
        // 두 슬롯의 퍼즐 인덱스를 갱신한다.
        if (fromSlot.TryGetComponent(out PuzzleSlot fromPuzzleSlot))
            UpdateSlotIndex(fromPuzzleSlot);

        if (toSlot.TryGetComponent(out PuzzleSlot toPuzzleSlot))
            UpdateSlotIndex(toPuzzleSlot);
    }

    private void UpdateSlotIndex(PuzzleSlot slot)
    {
        PuzzlePiece piece = slot.GetComponentInChildren<PuzzlePiece>();
        if (piece != null)
            slot.SetIndex(piece.Index);
        else
            slot.SetIndex(-1);
    }

    // 버튼에서 호출
    public void CheckAnswer()
    {
        List<int> submit = new List<int>();

        foreach (var slot in puzzleSlots)
        {
            submit.Add(slot.PuzzleIndex);
        }

        bool isCorrect = puzzleBeach.IsCorrect(submit);

        if (isCorrect)
        {
            Debug.Log("정답입니다!");
        }
        else
        {
            string failed = "오답입니다";
            foreach (int result in submit)
            {
                failed += $" {result}";
            }
            Debug.Log(failed);
        }
    }
}

