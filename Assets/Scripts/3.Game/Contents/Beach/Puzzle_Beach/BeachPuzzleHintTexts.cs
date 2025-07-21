using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BeachPuzzleHintTexts : MonoBehaviour
{
    [SerializeField] List<string> red;
    [SerializeField] List<string> yellow;
    [SerializeField] List<string> green;
    [SerializeField] List<string> blue;
    [SerializeField] List<string> purple;

    TextMeshProUGUI[] texts;
    List<List<string>> colors;

    private void Awake()
    {
        texts = GetComponentsInChildren<TextMeshProUGUI>();

        colors = new List<List<string>> { red, yellow, green, blue, purple };
    }

    private void Start()
    {
        SetTexts();
    }

    void SetTexts()
    {
        IReadOnlyList<int> _answer = SetPuzzleAnswer.Instance.Answer;
        
        texts[0].text =
            $"{GetRandomExpression(_answer[0])}하고 {GetRandomExpression(_answer[1])}느낌의 색이 앞장선다.";
        texts[1].text =
            $"{GetRandomExpression(_answer[2])}느낌의 색이 중심을 지킨다.";
        texts[2].text =
            $"{GetRandomExpression(_answer[3])}느낌의 색과 {GetRandomExpression(_answer[4])}느낌의 색은 서로 맞닿아야 한다.";
        ///
        /// 하고   느낌의 색이 앞장선다.
        /// 느낌의 색이 중심을 지킨다.
        /// 느낌의 색과   느낌의 색은 서로 맞닿아야 한다.
        ///
    }

    string GetRandomExpression(int i)
    {
        return colors[i][Random.Range(0, colors[i].Count)];
    }
}
