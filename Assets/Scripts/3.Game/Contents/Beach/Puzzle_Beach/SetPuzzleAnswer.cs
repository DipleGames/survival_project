using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SetPuzzleAnswer : Singleton<SetPuzzleAnswer>
{
    [Tooltip("Is puzzle Cleared")]
    public bool isCleared = false;
    [SerializeField] int puzzleSize = 5;
    public int PuzzleSize { get { return puzzleSize; } }

    int shellID = 0;
    public int ShellID { get { return shellID; } }

    [SerializeField]List<int> answer;
    public IReadOnlyList<int> Answer => answer.AsReadOnly();
    

    protected override void Awake()
    {
        base.Awake();
        if (isCleared) Destroy(gameObject);

        answer = InitAnswer();
    }

    // 정답 설정 로직
    private List<int> InitAnswer()
    {
        GameManager gameManager = GameManager.Instance;
        foreach (var item in gameManager.Items)
        {
            if(item.Value.CompareTypeExact(ItemType.BeachPuzzlePiece))
            {
                shellID = item.Key;
                break;
            }
        }

        List<int> result = new List<int>()
        {
            shellID,shellID+3,shellID+2,shellID+1,shellID+4
        };

        return result;
    }
}
