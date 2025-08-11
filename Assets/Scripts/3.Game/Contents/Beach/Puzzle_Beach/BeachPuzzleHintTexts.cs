using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static UnityEditor.Recorder.OutputPath;

public class BeachPuzzleHintTexts : MonoBehaviour
{
    [SerializeField] List<string> red;
    [SerializeField] List<string> yellow;
    [SerializeField] List<string> green;
    [SerializeField] List<string> blue;
    [SerializeField] List<string> purple;

    readonly string[] templates = new string[]
    {
        "{0}색과 {1}색은 서로 맞닿아야 한다.",
        "{0}색은 {1}색 바로 앞에 와야 한다.",
        "{0}색은 {1}색 보다 앞에 있어야 한다.",
        "{0}색은 {1}색과 붙어있으면 안된다."
    };

    TextMeshProUGUI[] texts;
    List<List<string>> colors;
    (int, int)[] colorPairs;
    int[] randomIndexes;

    private void Awake()
    {
        texts = GetComponentsInChildren<TextMeshProUGUI>();
        colors = new List<List<string>> { red, yellow, green, blue, purple };
        colorPairs = new (int, int)[texts.Length];
        randomIndexes = new int[templates.Length];
        for (int i = 0; i < randomIndexes.Length; i++) randomIndexes[i] = i;

        SetParagraphs(); // 힌트 양식 선택 및 색 표현 랜덤 지정
    }

    private void OnEnable()
    {
        SetRandomcolorExpression(); // 색 표현 랜덤 갱신
    }

    private void SetParagraphs()
    {
        Shuffle(randomIndexes);

        for (int i = 0; i < texts.Length; i++)
        {
            (int front, int back) = SetPairsByTemplates(randomIndexes[i]);
            colorPairs[i] = (front, back);

            string frontColorExpression = GetRandomColorExpression(front);
            string backColorExpression = GetRandomColorExpression(back);

            texts[i].text = string.Format(templates[randomIndexes[i]], frontColorExpression, backColorExpression);
        }
    }

    private void SetRandomcolorExpression()
    {
        for (int i = 0; i < texts.Length; i++)
        {
            (int front, int back) = colorPairs[i];

            string frontColorExpression = GetRandomColorExpression(front);
            string backColorExpression = GetRandomColorExpression(back);

            texts[i].text = string.Format(templates[randomIndexes[i]], frontColorExpression, backColorExpression);

        }
    }
    (int front,int back) SetPairsByTemplates(int templatesNum)
    {
        int puzzleSize = SetPuzzleAnswer.Instance.PuzzleSize;
        int i = Random.Range(0, puzzleSize);
        int j = 0;

        switch(templatesNum)
        {
            case 0:
                if (i <= 0) j = 1;
                else if (i >= puzzleSize - 1) j = puzzleSize - 2;
                else
                {
                    int value = Random.value > 0.5f ? 1 : -1;
                    j = i + value;
                }
                break;
            case 1:
                while (i >= puzzleSize -1)
                {
                    i = Random.Range(0, puzzleSize);
                }
                j = i + 1;
                break;
            case 2:
                while(i >= puzzleSize - 1)
                {
                    i = Random.Range(0, puzzleSize);
                }
                j = Random.Range(i+1, puzzleSize);
                break;
            case 3:
                j = Random.Range(0, puzzleSize);

                while ( Mathf.Abs(i-j) <= 1)
                {
                    j = Random.Range(0, puzzleSize);
                }
                break;
        }
        return (i, j);
    }
    string GetRandomColorExpression(int colorIndex)
    {
        return colors[colorIndex][Random.Range(0, colors[colorIndex].Count)];
    }
    void Shuffle<T>(T[] list)
    {
        for (int i = list.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
