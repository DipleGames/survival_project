using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puzzle_Beach : MonoBehaviour
{
    [SerializeField] int size;

    [Tooltip("현재 정답확인용")]
    [SerializeField] List<int> answer;

    private void Start()
    {
        answer = InitAnswer();
    }

    // 정답 설정 로직
    private List<int> InitAnswer()
    {
        List<int> result = new List<int>();
        for (int i = 0; i < size; i++)
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
    
    public bool IsCorrect(List<int> submit)
    {
        if (submit.Count != answer.Count) return false;

        for (int i = 0; i < submit.Count; i++)
        {
            if (submit[i] != answer[i]) return false;
        }
        return true;
    }
}
