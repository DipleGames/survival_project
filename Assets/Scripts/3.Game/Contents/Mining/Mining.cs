using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Mining : MonoBehaviour
{
    [SerializeField] GameObject currentObj;
    [SerializeField] List<GameObject> ObjectList;
    [SerializeField] List<int> objectProbability;
    [SerializeField] Text stoneTimerText;
    
    int currentType = 0;
    int probability = 0;
    int stoneMaxValue = 10;
    int stoneInputValue = 0;
    float stoneTimer = 0.3f;
    bool stone = false;
    float time = 0;
    
    public bool play = true;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        for (int i = 0; i < objectProbability.Count; i++)
        {
            probability += objectProbability[i];
        }
        RandomObjectSet();
    }

    public void OpenUI()
    {
        gameObject.SetActive(true);
        RandomObjectSet();
    }
    void Update()
    {
        if (play)
        {
            KeyInputJudgement();
        }
    }

    //키 입력
    void KeyInputJudgement()
    {
        //박쥐 오브젝트일때
        if (currentType < 2)
        {
            //왼쪽 방향키 입력
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (currentType == 0)
                {
                    StartCoroutine(BatFlyAnimation(0));
                }
                else
                {
                    StartCoroutine(BatAttackAnimation());
                }
                play = false;
            }
            //오른쪽 방향키 입력
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (currentType == 1)
                {
                    StartCoroutine(BatFlyAnimation(1));
                }
                else
                {
                    StartCoroutine(BatAttackAnimation());
                }
                play = false;
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                stoneInputValue++;
                stoneTimer = 0.03f;
                StopCoroutine(StoneAnimation());
                StartCoroutine(StoneAnimation());
            }
        }
    }
    void RandomObjectSet()
    {
        currentObj.SetActive(false);
        int type = Random.Range(0, probability);
        int minJudgement = 0;
        int maxJudgement = objectProbability[0];
        for (int i = 0; i < objectProbability.Count;i++)
        {
            if(minJudgement <= type && maxJudgement > type)
            {
                currentType = i;
                currentObj = ObjectList[i];
                break;
            }
            minJudgement += objectProbability[i];
            maxJudgement += objectProbability[i];
        }

        currentObj.GetComponent<RectTransform>().localPosition = Vector3.zero;
        currentObj.SetActive(true);
        if (currentType > 1)
        {
            stone = true;
            time = 10;
            stoneInputValue = 0;
            StartCoroutine(StoneObjectTimer());
        }
        else
        {
            time = 0;
        }

    }
    //박쥐가 날아가는 애니메이션
    IEnumerator BatFlyAnimation(int type)
    {
        Vector2 startPosition = new Vector2(0, 0);
        Vector2 peakPosition;
        Vector2 endPosition;
        if(type == 0 )
        {
            peakPosition = new Vector2(-3f, 4.5f);
            endPosition = new Vector2(-4, 0);
        }
        else
        {
            peakPosition = new Vector2(3f, 4.5f);
            endPosition = new Vector2(4, 0);
        }
        while (time < 1f)
        { 
            float t = time;
            float xPosition = Mathf.Pow(1 - t, 2) * startPosition.x + 2 * (1 - t) * t * peakPosition.x + Mathf.Pow(t, 2) * endPosition.x;
            float yPosition = Mathf.Pow(1 - t, 2) * startPosition.y + 2 * (1 - t) * t * peakPosition.y + Mathf.Pow(t, 2) * endPosition.y;
            currentObj.GetComponent<RectTransform>().localPosition = new Vector2(xPosition, yPosition) * 100;
            time += Time.deltaTime;
            yield return null;
        }
        play = true;
        RandomObjectSet();
        yield break;
    }
    //박쥐가 공격하는 애니메이션
    IEnumerator BatAttackAnimation()
    {
        while (time < 0.5f)
        {
            float t = time * 2;
            currentObj.GetComponent<RectTransform>().localScale = Vector3.one * (1.5f + t);
            time += Time.deltaTime;
            yield return null;
        }
        currentObj.GetComponent<RectTransform>().localScale = Vector3.one;
        play = true;
        RandomObjectSet();
        yield break;
    }

    //바위 오브젝트일때 타이머
    IEnumerator StoneObjectTimer()
    {
        stoneTimerText.gameObject.SetActive(true);
        bool stop = false;
        while (time > 0)
        {
            time -= Time.deltaTime;
            stoneTimerText.text = time.ToString("0.00");
            if(stoneMaxValue < stoneInputValue)
            {
                stop = true;
                break;
            }
            yield return null;
        }
        if (time < 0 || stop)
        {
            stoneTimerText.gameObject.SetActive(false);
            RandomObjectSet();
            stone = false;
            yield break;
        }
        yield return null;
    }

    IEnumerator StoneAnimation()
    {
        while (stoneTimer > 0)
        {
            stoneTimer -= Time.deltaTime;
            float angle = stoneTimer * 10;
            currentObj.transform.localRotation = Quaternion.Euler(0,0,angle);
            yield return null;
        }
        currentObj.transform.localRotation = Quaternion.Euler(0, 0, 0);
        yield return null;
    }
}
