using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum TutoType
{
    Null,
    StartTuto,
    StartItemTuto,
    BeachTuto,
    MoveTuto,
    BushTuto,
    LogTuto,
    TamingTuto,
    CampFireTuto,
    FishingTuto,
    NightTuto,
    MulticellTuto,
    FishingEnterTuto,
    BeachGetTuto,
    TamingGameTuto,
    DashTuto,
    Count,
}

/*public enum TutoImageType
{
    Null,
    BeachGathering,
    BushGathering,
    WoodLogging,
    WeaponChange,
    TamingGame,
    FishingGame,
    CampFire,
    MulticellInven,
    Count,
}*/

public class TutorialManager : Singleton<TutorialManager>
{
#if UNITY_EDITOR
    [SerializeField] bool isTuto;
#endif

    Dictionary<TutoType, int> tutorialClear;

    Dictionary<TutoType, string[]> tutoText;

    bool isTutoProgressing = false;

    [HideInInspector] public bool IsTutoProgressing => isTutoProgressing;
    [HideInInspector] public bool isTypingEnd = false;
    [HideInInspector] public bool tutorialCheck = false;

    private void Start()
    {
        tutorialClear = new Dictionary<TutoType, int>();
        tutoText = new Dictionary<TutoType, string[]>();

#if UNITY_EDITOR
        for (int i = 0; i < (int)TutoType.Count; i++)
        {
            if (!isTuto)
                tutorialClear.Add((TutoType)i, 1);

            else
                tutorialClear.Add((TutoType)i, 0);
        }
#else

        for (int i = 0; i < (int)TutoType.Count; i++)
        {
            tutorialClear.Add((TutoType)i, PlayerPrefs.GetInt(((TutoType)i).ToString(), 0));
        }
#endif

        TutoTextContent();
    }

    int count = 0;

    public IEnumerator IActiveTutoText(GameObject tutoPanel, GameObject closeText, Text characterText, TutoType tutoType, TutoType nextTuto)
    {
        if (tutorialClear[tutoType] == 1 || tutoType == TutoType.Null)
            yield break;

        tutorialCheck = true;

        yield return CoroutineCaching.WaitWhile(() => isTutoProgressing);

        isTutoProgressing = true;
        isTypingEnd = false;

        GameManager.Instance.GamePause(true);

        characterText.text = tutoText[tutoType][count];

        if (tutoType != TutoType.Null)
        {
            tutoPanel.SetActive(true);
            closeText.SetActive(false);
        }

        yield return CoroutineCaching.WaitForSecondsRealTime(0.5f);

        count++;
        closeText.SetActive(true);

        yield return CoroutineCaching.WaitUntil(() => isTypingEnd && Input.GetMouseButtonDown(0));

        if (tutoText[tutoType].Count() > count)
        {
            tutoPanel.SetActive(false);
            closeText.SetActive(true);
            isTutoProgressing = false;
            StartCoroutine(IActiveTutoText(tutoPanel, closeText, characterText, tutoType, nextTuto));
            yield break;
        }

        tutoPanel.SetActive(false);

        PlayerPrefs.SetInt((tutoType).ToString(), 1);
        tutorialClear[tutoType] = 1;
        count = 0;

        yield return null;

        if (nextTuto != TutoType.Null)
        {
            isTutoProgressing = false;
            StartCoroutine(IActiveTutoText(tutoPanel, closeText, characterText, nextTuto, TutoType.Null));
        }

        else
        {
            isTutoProgressing = false;
            GameManager.Instance.GamePause(false);
        }

        tutorialCheck = false;
    }

    void TutoTextContent()
    {
        tutoText.Add(TutoType.StartTuto, new string[] { 
            "이 섬, 화물선이 지나다니는 항로에 있는 섬인 거로 기억하는데...", 
            $"{GameManager.Instance.maxRound}일만 버텨보자. 배가 지나갈지도 몰라." });
        tutoText.Add(TutoType.StartItemTuto, new string[] { "그 녀석들이 던져주고 간 물품은 챙겨야지.", "하지만 가지고 있을 손이 부족하니 일단 하나만 챙기자." });
        tutoText.Add(TutoType.BeachTuto, new string[] { "일단 해안가 주변에 떨어진 나무들을 좀 주워서\n불이라도 때워야지." });
        tutoText.Add(TutoType.MoveTuto, new string[] { "섬 안쪽으로 좀 더 들어가 보자.\n거처를 마련해야 할 것 같아." });
        tutoText.Add(TutoType.BushTuto, new string[] { "이 수풀의 열매들 두고두고 먹을 수 있겠는데?\n좀 따가야겠어.", "그리고 이 정도 크기면 안에 무언가 더 있을 것 같아" });
        tutoText.Add(TutoType.LogTuto, new string[] { "이 나무도 땔감으로 쓸 수 있겠는데?" , "그리고 혹시 나무를 쓰러트려 두면 도움이 되지 않을까?"});
        tutoText.Add(TutoType.TamingTuto, new string[] { "이 케이지는 뭐지? 무언갈 잡을 수 있으려나.", "혹시 모르니 나중에 미끼를 둬봐야겠다." });
        tutoText.Add(TutoType.CampFireTuto, new string[] { "여기 누군가가 있었나? 불을 지핀 흔적이 있어.\n일단 밤이 오기 전에 아까 캤던 땔감들로 불을 지펴야겠다.", "물고기를 잡는다면 여기서 구워 먹어도 되겠는데?" });
        tutoText.Add(TutoType.FishingTuto, new string[] { "그리고 동쪽에 절벽이 있는 것 같은데?\n저기서 낚시를 할 수 있을 것 같아." });
        tutoText.Add(TutoType.NightTuto, new string[] { "왜 이렇게 음산하지… 꼭 유령이라도 나올 것만 같은데...\n혹시 모르니 준비해야겠어." });
        tutoText.Add(TutoType.MulticellTuto, new string[] { "주워둔 물품들을 유용하게 사용할 수 있겠는데? 주머니에서 꺼내 착용해보자." });
        tutoText.Add(TutoType.BeachGetTuto, new string[] { "이런 나무들을 주우면 되겠는데?", "왠지 바다에서 떠밀려 온 물품도 주울 수 있을 것 같아." });
        tutoText.Add(TutoType.FishingEnterTuto, new string[] { "이 절벽 바위 위에서 낚시하면 좋을 것 같아.", "물고기도 좋지만 떠밀려 온 물품도 낚이면 좋겠는데..." });
        tutoText.Add(TutoType.TamingGameTuto, new string[] { "저 매를 길들일 수 있으면 든든할 것 같은데?\n한 번 잡아보자." });
        tutoText.Add(TutoType.DashTuto, new string[] { "몸이 왠지 엄청 가벼운걸.\n저 정도 거리는 한 번에 달려갈 수 있을 것 같아." });
    }
}
