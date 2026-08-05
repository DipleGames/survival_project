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

        typingString = textMasage == "" ? typingText.text : textMasage;

        StartCoroutine(Typing(typingText, typingString, typingSpeed, waitTime));
    }

    private void Update()
    {
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
        TutorialManager.Instance.isTypingEnd = true;
    }
}
