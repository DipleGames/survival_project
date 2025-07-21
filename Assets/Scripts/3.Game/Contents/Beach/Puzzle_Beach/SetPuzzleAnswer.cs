using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPuzzleAnswer : Singleton<SetPuzzleAnswer>
{
    /// <summary>
    /// 해변1->해변2 퍼즐 클리어 여부
    /// </summary>
    public bool isCleared = false;
    [SerializeField] int puzzleSize = 5;

    [SerializeField]List<int> answer;
    public IReadOnlyList<int> Answer => answer.AsReadOnly();
    

    void Start()
    {
        if (isCleared) Destroy(gameObject);

        answer = InitAnswer();
    }

    // 정답 설정 로직
    private List<int> InitAnswer()
    {
        List<int> result = new List<int>();

        for (int i = 0; i < puzzleSize; i++)
        {
            result.Add(i);
        }

        for (int i = result.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (result[j], result[i]) = (result[i], result[j]);
        }

        return result;
    }
}
