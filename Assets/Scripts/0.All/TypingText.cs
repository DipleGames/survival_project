using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TypingText : MonoBehaviour
{
    [SerializeField] Text typingText;  
    [SerializeField] string textMasage;  
    [SerializeField] float typingSpeed; 
    [SerializeField] float waitTime = 1f;
    [SerializeField] bool isTypingFullMessage = false;

    [HideInInspector] public bool isOver = false;

    string typingString;

    private void OnEnable()
    {
        isOver = false;

        // 텍스트 메시지가 비어있으면 typingText의 텍스트를 사용하고, 그렇지 않으면 textMasage를 사용
        typingString = textMasage == "" ? typingText.text : textMasage;

        StartCoroutine(Typing(typingText, typingString, typingSpeed, waitTime));
    }

    private void Update()
    {
        // isTypingFullMessage가 Inspector에서 true로 설정된 경우에만, 마우스 왼쪽클릭시 isOver이 true로 설정
        if (isTypingFullMessage && Input.GetMouseButtonDown(0))
        {
            isOver = true;
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator Typing(Text typingText, string message, float speed, float waitTime)
    {
        for (int i = 0; i < message.Length; i++)
        {
            // isOver이 다른곳에서 true로 설정된다면 즉시 메세지를 전부 출력하고 루프 종료
            if (isOver)
            {
                typingText.text = message;
                break;
            }

            typingText.text = message.Substring(0, i + 1);

            yield return CoroutineCaching.WaitForSecondsRealTime(speed);
        }

        yield return CoroutineCaching.WaitForSecondsRealTime(waitTime);

        isOver = true;
        TutorialManager.Instance.isTypingEnd = true; // 타이핑이 끝났음을 TutorialManager에 알림
    }
}
