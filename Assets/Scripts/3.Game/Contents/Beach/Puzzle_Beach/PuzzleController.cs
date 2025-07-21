using System.Collections.Generic;
using UnityEngine;

public class PuzzleController : MonoBehaviour
{
    [SerializeField] List<PuzzleSlot> puzzleSlots;
    BasicPanel puzzleUI;

    private void Awake()
    {
        if (puzzleUI == null)
        {
            puzzleUI = transform.parent.GetComponent<BasicPanel>();
        }
    }
    private void Start()
    {
        if (puzzleSlots == null || puzzleSlots.Count == 0)
        {
            puzzleSlots = new List<PuzzleSlot>(GetComponentsInChildren<PuzzleSlot>());
        }
        if (puzzleUI == null) { Debug.LogError("puzzleUI not assigned"); }

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
            slot.SetIndex((int)piece.Index);
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

        if (IsCorrect(submit))
        {
            Debug.Log("(대충 해변 2가 활성화된다는 내용)");
            puzzleUI.ClosePanel();
        }
        else
        {
            Debug.Log("오답입니다");
        }
    }

    bool IsCorrect(List<int> submit)
    {
        if (submit.Count != SetPuzzleAnswer.Instance.Answer.Count) return false;

        for (int i = 0; i < submit.Count; i++)
        {
            if (submit[i] != SetPuzzleAnswer.Instance.Answer[i]) return false;
        }
        return true;
    }
}

