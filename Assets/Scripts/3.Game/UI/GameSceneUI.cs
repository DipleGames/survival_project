using System;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public enum CursorType
{
    Normal,
    Attack,
}

public class GameSceneUI : Singleton<GameSceneUI>
{
    Texture2D cursorAttack;
    Texture2D cursorNormal;

    [SerializeField] public GameObject monsterSpawn;
    public GameObject tamingGame;

    [Header("HP")]
    [SerializeField] Text hpText;
    [SerializeField] Slider hpBar;
    [SerializeField] Text recText;
    [SerializeField] Slider recoveryGaugeBar;

    [Header("Items")]
    [SerializeField] Text woodCount;
    [SerializeField] Text fish1Count;
    [SerializeField] Text fish2Count;
    [SerializeField] Text bulletCount;
    [SerializeField] GameObject itemPanel;

    [Header("Time")]
    [SerializeField] Text timeText;

    [Header("Round")]
    [SerializeField] Text roundText;

    [Header("Dash")]
    [SerializeField] GameObject dash;
    [SerializeField] Text dashKey;
    [SerializeField] Image dashImage;

    [Header("Stat")]
    [SerializeField] GameObject statWindow;
    [SerializeField] Text maxHp;
    [SerializeField] Text recoverHp;
    [SerializeField] Text def;
    [SerializeField] Text avoid;
    [SerializeField] Text damage;
    [SerializeField] Text closeDamage;
    [SerializeField] Text longDamage;
    [SerializeField] Text aSpd;
    [SerializeField] Text spd;
    [SerializeField] Text cri;

    [Header("Day Change")]
    [SerializeField] GameObject dayChangeGO;
    [SerializeField] Text dayChangeText;
    [SerializeField] Image dayChangeImage;
    [SerializeField] Sprite[] dayChangeSprites;

    [Header("Text")]
    [SerializeField] Text bossSceneText;

    GameManager gameManager;
    Character character;
    SoundManager soundManager;
    GamesceneManager gamesceneManager;

    [SerializeField] GameObject fishingGame;
    [SerializeField] GameObject weaponUI;
    [SerializeField] GameObject pieceCard;
    [SerializeField] public TilemapRenderer tileMap;
    [SerializeField] Material lightingMat;
    [SerializeField] GameObject spotlight;

    [Header("Clear")]
    [SerializeField] Image clearImage;
    [SerializeField] Sprite[] clearImages;
    [SerializeField] Text clearClickText;
    [SerializeField] TypingText gameClearText;
    [SerializeField] TypingText gameOverText;

    [Header("Tuto")]
    [SerializeField] public GameObject tutoTextPanel;
    [SerializeField] GameObject tutoClickText;
    [SerializeField] Text tutoText;
    [SerializeField] Image tutoImage;
    [SerializeField] GameObject tutoImagePanel;

    [Header("Tower")]
    [SerializeField] public GameObject towerWindow;

    Color initTimeColor;

    Coroutine currentCoroutine;

    protected override void Awake()
    {
        base.Awake();
        bossSceneText.gameObject.SetActive(false);
        dash.SetActive(false);
        statWindow.SetActive(false);
        tamingGame.SetActive(false);
        fishingGame.SetActive(false);
        weaponUI.SetActive(false);
        dayChangeGO.SetActive(false);
        itemPanel.SetActive(true);

        spotlight.SetActive(false);

        clearImage.gameObject.SetActive(false);
        clearClickText.gameObject.SetActive(false);
        gameClearText.gameObject.SetActive(false);
        gameOverText.gameObject.SetActive(false);

        tutoTextPanel.SetActive(false);
        tutoImagePanel.gameObject.SetActive(false);

        gameManager = GameManager.Instance;
    }

    private void Start()
    {
        character = Character.Instance;
        soundManager = SoundManager.Instance;
        gamesceneManager = GamesceneManager.Instance;

        initTimeColor = timeText.color;
    }

    public void ChangeTilemapMat(Vector3 pos)
    {
        tileMap.material = lightingMat;

        spotlight.transform.position = pos;

        spotlight.SetActive(true);
    }

    public void ActiveTutoPanel(TutoType tutoType, Sprite _tutoImage = null, TutoType nextTuto = TutoType.Null)
    {
        StartCoroutine(TutorialManager.Instance.IActiveTutoText(tutoTextPanel, tutoClickText, tutoText, tutoType, nextTuto));

        if(_tutoImage != null)
        {
            tutoImage.sprite = _tutoImage;
            tutoImagePanel.gameObject.SetActive(true);
        }

        else
        {
            tutoImagePanel.gameObject.SetActive(false);
        }
    }

    public IEnumerator IActiveTutoPanel(TutoType tutoType, Sprite _tutoImage = null)
    {
        StartCoroutine(TutorialManager.Instance.IActiveTutoText(tutoTextPanel, tutoClickText, tutoText, tutoType, TutoType.Null));

        yield return CoroutineCaching.WaitWhile(() => TutorialManager.Instance.IsTutoProgressing);

        if (_tutoImage != null)
        {
            tutoImage.sprite = _tutoImage;
            tutoImagePanel.gameObject.SetActive(true);
        }

        else
        {
            tutoImagePanel.gameObject.SetActive(false);
        }
    }

    IEnumerator BlinkBossSceneText()
    {
        float fadeTime = 0.5f;
        Color color = bossSceneText.color;

        while (fadeTime > 0f)
        {
            fadeTime = Mathf.Clamp(fadeTime - Time.deltaTime, 0f, 0.5f);
            color.a = fadeTime + 0.5f;
            bossSceneText.color = color;
            yield return null;
        }

        while (fadeTime < 0.5f)
        {
            fadeTime = Mathf.Clamp(fadeTime + Time.deltaTime, 0f, 0.5f);
            color.a = fadeTime + 0.5f;
            bossSceneText.color = color;
            yield return null;
        }

        while (fadeTime > 0f)
        {
            fadeTime = Mathf.Clamp(fadeTime - Time.deltaTime, 0f, 0.5f);
            color.a = fadeTime + 0.5f;
            bossSceneText.color = color;
            yield return null;
        }

        bossSceneText.gameObject.SetActive(false);
    }
    
    public void ChangeDayText(int index, string text)
    {
        dayChangeText.text = text;
        dayChangeImage.sprite = dayChangeSprites[index];

        dayChangeGO.SetActive(true);

        if(currentCoroutine != null) 
        {
            StopCoroutine(currentCoroutine);
        }

        currentCoroutine = StartCoroutine(FadeOut(dayChangeImage, dayChangeText, dayChangeGO));
    }

    private void Update()
    {
        HpUI();
        RecoveryGauegeUI();
        ItemsCountUI();
        RoundUI();
        TimeUI();
        DashUI();
        WeaponAndItemUI();
        SettingStatText();

        if(clearClickText.gameObject.activeSelf)
        {
            if (Input.GetMouseButtonUp(0))
            {
                SceneManager.LoadScene(0);

                if (soundManager.gameObject != null)
                    Destroy(soundManager.gameObject);

                if (gameManager.gameObject != null)
                    Destroy(gameManager.gameObject);

                if (character.gameObject != null)
                    Destroy(character.gameObject);
            }
        }
    }

    TypingText typingText;

    public IEnumerator GameClear(int num)
    {
        StopAllCoroutines();

        cursorNormal = GameManager.Instance.useCursorNormal;

        Vector2 cursorHotSpot = new Vector3(cursorNormal.width * 0.5f, cursorNormal.height * 0.5f);
        Cursor.SetCursor(cursorNormal, cursorHotSpot, CursorMode.ForceSoftware);
        //SoundManager.Instance.PlayBGM(4, false);

        if (num == 0)
        {
            typingText = gameOverText;
        }

        else if (num == 1)
        {
            typingText = gameClearText;
        }

        if(typingText == null)
            yield break;

        typingText.gameObject.SetActive(true);
        
        yield return CoroutineCaching.WaitWhile(() => !typingText.isOver);

        typingText.gameObject.SetActive(false);

        if (clearImages.Length > 0)
            clearImage.sprite = clearImages[num];

        clearImage.gameObject.SetActive(true);

        StartCoroutine(ClearImageFadeIn(clearImage, 2, clearClickText.gameObject));
    }

    public void CursorChange(CursorType cursorType)
    {
        cursorNormal = gameManager.useCursorNormal;
        cursorAttack = gameManager.useCursorAttack;

        Texture2D currentCursor;

        switch (cursorType)
        {
            case CursorType.Normal:
                currentCursor = cursorNormal;
                break;

            case CursorType.Attack:
                currentCursor = cursorAttack;
                break;

            default:
                currentCursor = cursorNormal;
                break;
        }

        Vector2 cursorHotSpot = new Vector3(currentCursor.width * 0.5f, currentCursor.height * 0.5f);
        Cursor.SetCursor(currentCursor, cursorHotSpot, CursorMode.ForceSoftware);
    }

    /*IEnumerator FadeIn()
    {
        yield return new WaitForSeconds(1f);

        float fadeTime = 0f;
        Color imageColor = clearImage.color;
        clearImage.gameObject.SetActive(true);

        do
        {
            fadeTime = Mathf.Clamp(fadeTime + Time.deltaTime, 0f, 2f);
            imageColor.a = fadeTime / 2f;
            clearImage.color = imageColor;
            yield return null;
        }

        while (fadeTime < 2f);

        yield return new WaitForSeconds(3.5f);

        clickText.SetActive(true);
    }*/

    IEnumerator FadeOut(Image image, Text text, GameObject go)
    {
        float fadeTime = 2f;
        Color imageColor = image.color;
        Color textColor = text.color;

        do
        {
            fadeTime = Mathf.Clamp(fadeTime - Time.deltaTime, 0f, 2f);

            imageColor.a = fadeTime / 2f;
            textColor.a = imageColor.a;

            image.color = imageColor;
            text.color = textColor;

            yield return null;
        }
        while (fadeTime > 0);

        yield return null;

        go.SetActive(false);
    }

    IEnumerator ClearImageFadeIn(Image image, float fadeTime, GameObject text)
    {
        Color imageColor = image.color;

        do
        {
            fadeTime = Mathf.Clamp(fadeTime - Time.deltaTime, 0f, fadeTime);

            imageColor.a = 1 - fadeTime / 2f;

            image.color = imageColor;

            yield return null;
        }
        while (fadeTime > 0);

        yield return null;

        text.SetActive(true);
    }

    void SettingStatText()
    {
        maxHp.text = character.maxHp.ToString();
        recoverHp.text = Mathf.CeilToInt(character.RecoveryValue * (100 + character.recoverHpRatio) * 0.01f).ToString();
        def.text = character.defence.ToString("0.#");
        avoid.text = character.avoid.ToString("0.#");
        damage.text = gameManager.status[Status.Damage].ToString("0.0#");
        closeDamage.text = gameManager.status[Status.CloseDamage].ToString("0.0#");
        longDamage.text = gameManager.status[Status.LongDamage].ToString("0.0#");
        aSpd.text = character.attackSpeed.ToString("0.#");
        spd.text = character.speed.ToString("0.##");
        cri.text = gameManager.status[Status.Critical].ToString("0.#");
    }

    void DashUI()
    {
        if (gameManager.dashCount <= 0)
        {
            if(dash.activeSelf)
                dash.SetActive(false);

            return;
        }

        if (!dash.activeSelf)
            dash.SetActive(true);

        dashImage.gameObject.SetActive(dashImage.fillAmount != 1);
        dashKey.text = KeySetting.keys[KeyAction.DASH].ToString();
        dashImage.fillAmount = 0;

        dashImage.fillAmount = (character.dashCoolTime / character.initDashCoolTime);
        
    }

    void HpUI()
    {
        hpText.text = $"{character.currentHp} / {character.maxHp}";

        hpBar.value = ((float)character.currentHp / character.maxHp);
    }

    void RecoveryGauegeUI()
    {
        recText.text = $"{character.currentRecoveryGauge} / {character.maxRecoveryGauge}";

        recoveryGaugeBar.value = Mathf.Clamp((float)character.currentRecoveryGauge / character.maxRecoveryGauge, 0f, 1f);
    }

    void ItemsCountUI()
    {
        woodCount.text = gameManager.haveItems[gameManager.idByMaterialType[MaterialType.Wood]].ToString();
        fish1Count.text = gameManager.haveItems[gameManager.idByMaterialType[MaterialType.Fish]].ToString();
        fish2Count.text = gameManager.haveItems[gameManager.idByMaterialType[MaterialType.HighFish]].ToString();
        bulletCount.text = gameManager.totalBulletCount.ToString();
    }

    void RoundUI()
    {
        roundText.text = gameManager.round != 0 ? gameManager.round.ToString() : "1";
    }

    void TimeUI()
    {
        if (gamesceneManager.currentGameTime >= 5)
        {
            if (timeText.color != initTimeColor)
                timeText.color = initTimeColor;

            timeText.text = gamesceneManager.currentGameTime > 0 ? ((int)gamesceneManager.currentGameTime).ToString() : "0.00";
        }

        else
        {
            if (timeText.color != Color.red)
                timeText.color = Color.red;

            timeText.text = (gamesceneManager.currentGameTime).ToString("F2");
        }
    }

    bool isNight = false;

    void WeaponAndItemUI()
    {
        if (isNight == gamesceneManager.isNight)
            return;

        isNight = gamesceneManager.isNight;

        if (isNight)
        {
            weaponUI.SetActive(true);
            itemPanel.SetActive(false);
        }

        else
        {
            weaponUI.SetActive(false);
            itemPanel.SetActive(true);
        }
    }

    public void OnOffStatWindow()
    {
        statWindow.gameObject.SetActive(!statWindow.gameObject.activeSelf);
    }

    public void ShowPieceCard(DiabolicItemInfo getItem)
    {
        StartCoroutine(ShowPieceCardUI(getItem));
    }

    IEnumerator ShowPieceCardUI(DiabolicItemInfo getItem)
    {
        pieceCard.GetComponent<PieceCard>().GetRandomItem(getItem);
        pieceCard.gameObject.SetActive(true);

        yield return CoroutineCaching.WaitForSeconds(2);
        pieceCard.gameObject.SetActive(false);
    }

    public void OnTowerWindow(TowerModel towerModel) // 타워 패널 키는 기능
    {
        TowerView towerView = towerWindow.GetComponent<TowerView>();

        towerView.InitTowerModel(towerModel);
        towerView.SetTowerView();
        towerWindow.SetActive(true);
    }

    public void OffTowerWindow() // 타워 패널 닫는 기능
    {
        towerWindow.SetActive(false);
    }
}
