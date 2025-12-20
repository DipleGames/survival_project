using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Fishing : Singleton<Fishing>
{
    [SerializeField] float barMoveSpeed;
    [SerializeField] Slider catchBar;
    [SerializeField] RectTransform catchPoint;
    [SerializeField] Text catchText;
    [SerializeField] Text maxFishingCount;
    [SerializeField] Text currentFishingCount;
    [SerializeField] Text[] catchItemsText;
    [SerializeField] Image catchFishImage;
    [SerializeField] Sprite[] fishTypeImage;
    [SerializeField] GameObject pieceCard;
    [SerializeField] DiabolicItemInfo[] fishingPieceList;
    [SerializeField] GameObject clickUI;
    [SerializeField] AudioClip successSound;
    [SerializeField] AudioClip failSound;

    bool isCatch = false;
    [HideInInspector] public bool isCatchingStart = false;
    
    int maxFishCount;
    int currentFishCount;

    GameManager gameManager;
    GamesceneManager gamesceneManager;
    FishingAnim fishingAnim;
    ItemManager itemManager;
    SoundManager soundManager;


    float catchPointStart;
    float catchPointEnd;

    float catchPointWidth;
    float initCatchPointWidth;

    List<DiabolicItemInfo> itemList = new List<DiabolicItemInfo>();

    bool isPress = false;

    protected override void Awake()
    {
        base.Awake();

        gameManager = GameManager.Instance;
        gamesceneManager = GamesceneManager.Instance;
        fishingAnim = FishingAnim.Instance;
        itemManager = ItemManager.Instance;
        soundManager = SoundManager.Instance;

        pieceCard.SetActive(false);

        initCatchPointWidth = catchBar.maxValue + catchPoint.offsetMax.x - catchPoint.offsetMin.x;

        clickUI.SetActive(false);
    }

    private void OnEnable()
    {
#if UNITY_EDITOR
        maxFishCount = 55;
#else
        maxFishCount = 10;
#endif
        currentFishCount = maxFishCount;

        maxFishingCount.text = maxFishCount.ToString();
        currentFishingCount.text = currentFishCount.ToString();

        catchPointWidth = gameManager.specialStatus[SpecialStatus.RustyHarpoon] ? initCatchPointWidth * 2 : initCatchPointWidth;

        Initialize();
    }

    private void OnDisable()
    {
        pieceCard.SetActive(false);
        catchFishImage.gameObject.SetActive(false);

        Character.Instance.isCanControll = true;

        Initialize();
    }

    void Update()
    {
        if(gameManager.isPause)
            return;

        if (gamesceneManager.isNight)
        {
            Character.Instance.isCanControll = true;
            gameObject.SetActive(false);

            Initialize();
        }

        if (!isCatch && isCatchingStart)
        {
            catchBar.gameObject.SetActive(true);
            isCatchingStart = false;

            GetRandomCatchPoint();
        }

        MoveBar();
    }

    void GetRandomCatchPoint()
    {
        catchPointStart = Random.Range(0, catchBar.maxValue - catchPointWidth);
        catchPointEnd = Mathf.Clamp(catchPointStart + catchPointWidth, catchPointWidth, catchBar.maxValue);

        catchPoint.offsetMin = new Vector2(catchPointStart, catchPoint.offsetMin.y);
        catchPoint.offsetMax = new Vector2(catchPointEnd - catchBar.maxValue, catchPoint.offsetMax.y);
    }

    bool isMin = true;

    void MoveBar()
    {
        if (!catchBar.IsActive())
            return;

        if (!isCatch && !isCatchingStart)
        {
            if (Input.GetMouseButtonUp(0))
            {
                CatchFish();
            }

            else if (Input.GetMouseButtonDown(0))
            {
                if (isPress)
                {
                    return;
                }

                clickUI.SetActive(false);
                isPress = true;
            }

            else if (Input.GetMouseButton(0))
            {
                if(!isPress)
                {
                    return;
                }

                if (catchBar.value == catchBar.minValue)
                    isMin = true;

                else if (catchBar.value == catchBar.maxValue)
                    isMin = false;
 
                catchBar.value += (isMin ? Time.deltaTime : -Time.deltaTime) * 100 * barMoveSpeed;
            }
        }
    }

    void CatchFish()
    {
        isCatch = true;
        fishingAnim.isCatch = true;

#if UNITY_EDITOR
        catchBar.value = catchPointStart;
#endif

        if (catchBar.value < catchPointStart || catchBar.value > catchPointEnd)
        {
            fishingAnim.CatchSuccess = false;
            catchText.text = $"<color=#A52D39>놓쳤다...</color>";
            soundManager.PlaySFX(failSound);
        }

        else if (catchBar.value >= catchPointStart && catchBar.value <= catchPointEnd)
        {
            GetItem();
            fishingAnim.CatchSuccess = true;
            catchText.text = $"<color=#B09F5E>낚아챘다!</color>";
            soundManager.PlaySFX(successSound);
        }

        catchText.gameObject.SetActive(true);
    }

    void GetItem()
    {
        int rand = Random.Range(0, 100);

#if UNITY_EDITOR
        int high = gameManager.specialStatus[SpecialStatus.BaitWarm] ? 0 : 0;
#else
        int high = gameManager.specialStatus[SpecialStatus.BaitWarm] ? 60 : 80;
#endif

        if (rand < high)
        {
            gameManager.fishLowGradeCount++;
            GetExtraFishBySubstatus(true);

            catchFishImage.sprite = fishTypeImage[0];
        }

        else
        {
            gameManager.fishHighGradeCount++;
            GetExtraFishBySubstatus(false);

            catchFishImage.sprite = fishTypeImage[1];

            rand = Random.Range(0, 100);

#if UNITY_EDITOR
            if (rand >= 0)
            {
                GetRandomPiece();
            }
#else
            if (rand >= 100 - gameManager.pieceCardGetRate)
            {
                GetRandomPiece();
            }
#endif
        }
    }


    void GetRandomPiece()
    {
        itemList.Clear();

        for (int i = 0; i < fishingPieceList.Length; ++i)
        {
            if (!itemManager.getItems.ContainsKey(fishingPieceList[i]))
            {
                itemList.Add(fishingPieceList[i]);
            }

            else
            {
                if (itemManager.getItems[fishingPieceList[i]] < fishingPieceList[i].MaxCount)
                {
                    itemList.Add(fishingPieceList[i]);
                }
            }
        }

        if (itemList.Count <= 0)
        {
            return;
        }

        int totalWeightValue = 0;

        for (int i = 0; i<itemList.Count; ++i)
        {
            totalWeightValue += itemList[i].WeightValue;
        }

        int rand = Random.Range(0, totalWeightValue);

        float total = 0;

        for (int i = 0; i < itemList.Count; i++)
        {
            total += itemList[i].WeightValue;

            if (rand < total)
            {
                rand = i;
                break;
            }
        }

        pieceCard.GetComponent<PieceCard>().GetRandomItem(itemList[rand]);
        pieceCard.gameObject.SetActive(true);
        ItemManager.Instance.AddItem(itemList[rand]);
    }
    private void GetExtraFishBySubstatus(bool isLowGradeFish)
    {
        float addChance = gameManager.substatus[SubStatus.Fishing_AdditionalChance];
        int addAmount = (int)gameManager.substatus[SubStatus.Fishing_AdditionalAmount];
        int addAmountByChance = (int)gameManager.substatus[SubStatus.Fishing_AdditionalAmount_ByChance];

        int extraCount = 0;

        if (addChance > 99.99f)
        {
            extraCount += addAmount;
        }

        float rand = Random.Range(0f, 100f);
        if (addChance > rand)
        {
            extraCount += addAmountByChance;
        }

        if (extraCount == 0)
            return;

        if (isLowGradeFish)
            gameManager.fishLowGradeCount += extraCount;
        else
            gameManager.fishHighGradeCount += extraCount;
    }


    public IEnumerator CatchingStart()
    {
        clickUI.SetActive(true);
        isPress = false;

        yield return CoroutineCaching.WaitForSeconds(0.7f);

        clickUI.SetActive(false);

        if (isPress)
            yield break;

        isCatch = true;
        fishingAnim.isCatch = true;

        fishingAnim.CatchSuccess = false;
        catchText.text = $"<color=#A52D39>놓쳤다...</color>";
        soundManager.PlaySFX(failSound);

        catchText.gameObject.SetActive(true);
    }

    public IEnumerator FishingEnd()
    {
        currentFishCount--;
        currentFishingCount.text = currentFishCount.ToString();

        yield return CoroutineCaching.WaitForSeconds(2f);

        pieceCard.SetActive(false);

        if (currentFishCount <= 0)
        {
            Character.Instance.isCanControll = true;
            gameObject.SetActive(false);
        }

        Initialize();
    }

    public void Initialize()
    {
        catchBar.gameObject.SetActive(false);
        catchText.gameObject.SetActive(false);
        isCatch = false;
        isCatchingStart = false;

        if (currentFishCount > 0)
        {
            catchBar.value = 0;

            fishingAnim.isCatch = false;
        }
    }
}
