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

    Dictionary<Buff, int> buffValues = new Dictionary<Buff, int>();
    Buff beforeBuff = Buff.COUNT;

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

        for (int i = 0; i < (int)Buff.COUNT; i++)
        {
            buffValues.Add((Buff)i, 1);
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
                buffValues[(Buff)i] = 0;
            }
        }

        if (beforeBuff == Buff.SPEED && buffValues[Buff.SPEED] == 3)
        {
            StartCoroutine(GameSceneUI.Instance.IActiveTutoPanel(TutoType.DashTuto, dashTutoImage));
        }

        SettingBuff();
    }

    void SettingBuff()
    {
        character.maxHp = Mathf.RoundToInt(character.maxHp * (100 + mxHps[buffValues[Buff.MAXHEALTH]]) * 0.01f);
        //character.hp = character.maxHp;

        character.recoverHpRatio += reHps[buffValues[Buff.RECOVERY_HEALTH]];

        character.speed *= (100 + speeds[buffValues[Buff.SPEED]]) * 0.01f;
        character.avoid += avoids[buffValues[Buff.SPEED]];
        //gameManager.dashCount = buffValues[Buff.SPEED] == 3 ? 1 : 0;
#if UNITY_EDITOR
#else
        gameManager.dashCount = dash[buffValues[Buff.SPEED]];
        character.dashCount = gameManager.dashCount;
#endif

        character.percentDamage += dmgs[buffValues[Buff.POWER]];
        character.defence += dfs[buffValues[Buff.POWER]];
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
        for (int i = 0; i < (int)Buff.COUNT; i++)
        {
            buffValues[(Buff)i] = 0;
            debuffValues[(Debuff)i] = 0;
        }

        beforeBuff = Buff.COUNT;

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

        Buff buffType = (Buff)Random.Range(0, (int)Buff.COUNT);

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
            case Buff.MAXHEALTH:
                buffDescriptionPanel.SetBuffTextInfo(buffType, mxHps[buffValues[Buff.MAXHEALTH]]);
                break;

            case Buff.POWER:
                buffDescriptionPanel.SetBuffTextInfo(buffType, dmgs[buffValues[Buff.POWER]], dfs[buffValues[Buff.POWER]]);
                break;

            case Buff.SPEED:
                buffDescriptionPanel.SetBuffTextInfo(buffType, speeds[buffValues[Buff.SPEED]], avoids[buffValues[Buff.SPEED]], dash[buffValues[Buff.SPEED]]);
                break;

            case Buff.RECOVERY_HEALTH:
                buffDescriptionPanel.SetBuffTextInfo(buffType, reHps[buffValues[Buff.RECOVERY_HEALTH]]);
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
