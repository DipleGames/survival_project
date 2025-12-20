using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Debuff
{
    RECOVERY_HEALTH,
    ATTACK_SPEED,
    SPEED,
    POWER,
    COUNT,
}

public class Campfire : MonoBehaviour, IMouseInteraction
{
    [SerializeField] GameObject interactionUI;
    [SerializeField] GameObject buffIcon;
    [SerializeField] GameObject debuffIcon;
    [SerializeField] GameObject fireImage;
    [SerializeField] BuffDescription buffDescriptionPanel;
    [SerializeField] AudioClip buffSound;
    [SerializeField] AudioClip igniteSound;
    [SerializeField] Sprite dashTutoImage;
    [SerializeField] GameObject needWoodImage;
    [SerializeField] GameObject needFishImage;
    [SerializeField] GameObject createPanel;

    GameManager gameManager;
    Character character;
    GamesceneManager gamesceneManager;
    SoundManager soundManager;

    bool canInteraction = false;
    bool canCookFish = false;
    Vector3 fireInitScale;

    Dictionary<OldBuff, int> buffValues = new Dictionary<OldBuff, int>();
    OldBuff beforeBuff = OldBuff.COUNT;

    Dictionary<Debuff, int> debuffValues = new Dictionary<Debuff, int>();

    bool isWoodRefill = false;

    int[] mxHps = { 0, 15, 30, 50 };
    int[] reHps = { 0, 20, 30, 40 };
    int[] speeds = { 0, 5, 7, 10 };
    int[] avoids = { 0, 3, 5, 7 };
    int[] dmgs = { 0, 10, 15, 20 };
    int[] dfs = { 0, 0, 5, 10 };
    int[] dash = { 0, 0, 0, 1 };

    private void Start()
    {
        gameManager = GameManager.Instance;
        character = Character.Instance;
        gamesceneManager = GamesceneManager.Instance;
        soundManager = SoundManager.Instance;

        fireInitScale = fireImage.transform.localScale;

        for (int i = 0; i < (int)OldBuff.COUNT; i++)
        {
            buffValues.Add((OldBuff)i, 1);
            debuffValues.Add((Debuff)i, 0);
        }

        buffIcon.SetActive(false);
        debuffIcon.SetActive(false);
        needFishImage.SetActive(false);
        needWoodImage.SetActive(false);

        GetComponentInChildren<CheckCharacter>().needItemImage = needWoodImage;

        fireImage.transform.localScale = Vector3.zero;
    }

    private void Update()
    {
        ChangeFireImageScale();
    }

    void ChangeFireImageScale()
    {
        if (isWoodRefill && gamesceneManager.isNight)
        {
            fireImage.transform.localScale = fireInitScale * (gamesceneManager.currentGameTime / gameManager.gameNightTime);
        }
    }

    public void OnDebuff()
    {
        if (isWoodRefill || gameManager.specialStatus[SpecialStatus.Rum])
            return;

        Debuff debuffType = (Debuff)Random.Range(0, (int)Debuff.COUNT);

        debuffValues[debuffType] = 1;

        character.recoverHpRatio -= debuffValues[Debuff.RECOVERY_HEALTH] * 10;

        character.attackSpeed *= (10 + debuffValues[Debuff.ATTACK_SPEED]) * 0.1f;

        character.speed *= (10 - debuffValues[Debuff.SPEED] * 2) * 0.1f;

        character.percentDamage -= (debuffValues[Debuff.POWER]) * 20;

        debuffIcon.GetComponent<CampFireDebuff>().SetDebuff(debuffType);
        debuffIcon.SetActive(true);

        buffDescriptionPanel.SetDeBuffTextInfo(debuffType);
    }

    void OnBuff()
    {
        if (!isWoodRefill)
            return;

        for (int i = 0; i < buffValues.Count; i++)
        {
            if (i != (int)beforeBuff)
            {
                buffValues[(OldBuff)i] = 0;
            }
        }

        if (beforeBuff == OldBuff.SPEED && buffValues[OldBuff.SPEED] == 3)
        {
            StartCoroutine(GameSceneUI.Instance.IActiveTutoPanel(TutoType.DashTuto, dashTutoImage));
        }

        SettingBuff();
    }

    void SettingBuff()
    {
        character.maxHp = Mathf.RoundToInt(character.maxHp * (100 + mxHps[buffValues[OldBuff.MAXHEALTH]]) * 0.01f);
        //character.hp = character.maxHp;

        character.recoverHpRatio += reHps[buffValues[OldBuff.RECOVERY_HEALTH]];

        character.speed *= (100 + speeds[buffValues[OldBuff.SPEED]]) * 0.01f;
        character.avoid += avoids[buffValues[OldBuff.SPEED]];
        //gameManager.dashCount = buffValues[OldBuff.SPEED] == 3 ? 1 : 0;
#if UNITY_EDITOR
#else
        gameManager.dashCount = dash[buffValues[Buff.SPEED]];
        character.dashCount = gameManager.dashCount;
#endif

        character.percentDamage += dmgs[buffValues[OldBuff.POWER]];
        character.defence += dfs[buffValues[OldBuff.POWER]];
    }

    public void ToNightScene()
    {
        OnBuff();
        OnDebuff();
        canInteraction = false;
        interactionUI.SetActive(false);
    }

    public void ToDayScene()
    {
        OffBuffNDebuff();
        character.UpdateStat();

        isWoodRefill = false;
        GetComponentInChildren<CheckCharacter>().needItemImage = needWoodImage;

        character.InitailizeDashCool();
    }

    public void OffBuffNDebuff()
    {
        for (int i = 0; i < (int)OldBuff.COUNT; i++)
        {
            buffValues[(OldBuff)i] = 0;
            debuffValues[(Debuff)i] = 0;
        }

        beforeBuff = OldBuff.COUNT;

        SettingBuff();

        debuffIcon.SetActive(false);
        buffIcon.SetActive(false);
        buffDescriptionPanel.gameObject.SetActive(false);
    }

    public void InteractionLeftButtonFuc(GameObject hitObject)
    {
        if (!canInteraction)
            return;

        createPanel.GetComponent<CreatePanel>().SetCreateAcquisition(Acquisition.CampFire);
        createPanel.SetActive(true);

        OnFire();

        EatFish();
    }

    public void InteractionRightButtonFuc(GameObject hitObject)
    {
        
    }

    void OnFire()
    {
        if (isWoodRefill || gameManager.woodCount < 10)
            return;

        needWoodImage.SetActive(false);
        GetComponentInChildren<CheckCharacter>().needItemImage = needFishImage;

        gameManager.woodCount -= 10;
        fireImage.transform.localScale = fireInitScale;
        isWoodRefill = true;

        canInteraction = false;

        soundManager.PlaySFX(igniteSound);

        StartCoroutine(BuffCoolTime(0.7f));
    }

    void EatFish()
    {
        if (!canCookFish || !isWoodRefill || gameManager.fishLowGradeCount <= 0 && gameManager.fishHighGradeCount <= 0)
            return;

        //interactionUI.SetActive(false);
        canInteraction = false;

        OldBuff buffType = (OldBuff)Random.Range(0, (int)OldBuff.COUNT);

        if (gameManager.fishHighGradeCount > 0)
        {
            if (buffValues[buffType] < 2)
                buffValues[buffType] = 2;

            gameManager.fishHighGradeCount--;
        }

        else
        {
            if (buffValues[buffType] < 1)
                buffValues[buffType] = 1;

            gameManager.fishLowGradeCount--;
        }

        if (beforeBuff == buffType)
            buffValues[buffType] = Mathf.Clamp(buffValues[buffType] + 1, 1, 3);

        beforeBuff = buffType;

        buffIcon.GetComponent<FishBuffIcon>().SetBuffIcon(buffType, buffValues[buffType]);
        buffIcon.SetActive(true);

        switch (buffType)
        {
            case OldBuff.MAXHEALTH:
                buffDescriptionPanel.SetBuffTextInfo(buffType, mxHps[buffValues[OldBuff.MAXHEALTH]]);
                break;

            case OldBuff.POWER:
                buffDescriptionPanel.SetBuffTextInfo(buffType, dmgs[buffValues[OldBuff.POWER]], dfs[buffValues[OldBuff.POWER]]);
                break;

            case OldBuff.SPEED:
                buffDescriptionPanel.SetBuffTextInfo(buffType, speeds[buffValues[OldBuff.SPEED]], avoids[buffValues[OldBuff.SPEED]], dash[buffValues[OldBuff.SPEED]]);
                break;

            case OldBuff.RECOVERY_HEALTH:
                buffDescriptionPanel.SetBuffTextInfo(buffType, reHps[buffValues[OldBuff.RECOVERY_HEALTH]]);
                break;
        }

        soundManager.PlaySFX(buffSound);

        StartCoroutine(BuffCoolTime(1.5f));
    }

    IEnumerator BuffCoolTime(float time)
    {
        canCookFish = false;
        yield return CoroutineCaching.WaitForSeconds(time);

        if (!gamesceneManager.isNight)
        {
            //interactionUI.SetActive(true);
            canInteraction = true;
        }

        canCookFish = true;
    }

    public void CanInteraction(bool _canInteraction)
    {
        canInteraction = _canInteraction;
    }

    public IEnumerator EndInteraction(Animator anim, float waitTime)
    {
        yield return CoroutineCaching.WaitForSeconds(waitTime);
    }

    public bool ReturnCanInteraction()
    {
        return canInteraction;
    }
}
