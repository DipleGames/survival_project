using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


#region OptionSetting
// 타이틀/게임의 옵션 창이 각각 참조하는 공통 UI 컴포넌트입니다.
// 각 씬의 활성 상태인 옵션 관리자 오브젝트에 붙이고 UI 필드를 연결하세요.
public enum KeyAction { UP, DOWN ,LEFT, RIGHT, DASH, RECORVER, COUNT}

public static class KeySetting { public static Dictionary<KeyAction, KeyCode> keys = new Dictionary<KeyAction, KeyCode>(); }

public class OptionSettingsUI : MonoBehaviour
{
    [SerializeField] Slider AllSound;
    [SerializeField] Slider bgmSound;
    [SerializeField] Slider sfxSound;
    [SerializeField] GameObject wUnMark;
    [SerializeField] GameObject bUnMark;
    [SerializeField] GameObject sUnMark;
    [SerializeField] GameObject normalToggle;
    [SerializeField] GameObject doubleToggle;

    GameObject panel;
    [SerializeField] GameManager gameManager;
    SoundManager soundManager;
    bool muteAllVolume;
    bool muteBgmVolume;
    bool muteSfxVolume;
    bool initialized;

    // UP, DOWN, LEFT, RIGHT, DASH, RECORVER 순서. 키 UI가 없으면 비워도 됩니다.
    [SerializeField] Text[] keyTexts = new Text[0];
    int[] defaultKeys;
    int key = -1;
    int checkKey = -1;
    public bool IsWaitingForKey { get { return key >= 0; } }

    // 호출자가 패널을 먼저 닫은 후 명시적으로 초기화합니다.
    // 컴포넌트 사이의 Start 실행 순서에 의존하지 않습니다.
    public void Initialize(GameObject optionPanel)
    {
        if (initialized)
            return;

        panel = optionPanel;
        gameManager = GameManager.Instance;
        soundManager = SoundManager.Instance;

        // 초기값 로딩은 슬라이더 이벤트를 발생시키지 않습니다.
        AllSound.SetValueWithoutNotify(PlayerPrefs.GetFloat("Sound_All"));
        bgmSound.SetValueWithoutNotify(PlayerPrefs.GetFloat("Sound_Bgm"));
        sfxSound.SetValueWithoutNotify(PlayerPrefs.GetFloat("Sound_Sfx"));

        muteAllVolume = Convert.ToBoolean(PlayerPrefs.GetInt("Mute_All", 0));
        muteBgmVolume = Convert.ToBoolean(PlayerPrefs.GetInt("Mute_Bgm", 0));
        muteSfxVolume = Convert.ToBoolean(PlayerPrefs.GetInt("Mute_Sfx", 0));

        normalToggle.SetActive(!Convert.ToBoolean(PlayerPrefs.GetInt("CursorSize", 0)));
        doubleToggle.SetActive(!normalToggle.activeSelf);
        defaultKeys = new int[] { PlayerPrefs.GetInt("Key_Up", (int)KeyCode.W),
                                  PlayerPrefs.GetInt("Key_Down", (int)KeyCode.S),
                                  PlayerPrefs.GetInt("Key_Left", (int)KeyCode.A),
                                  PlayerPrefs.GetInt("Key_Right", (int)KeyCode.D),
                                  PlayerPrefs.GetInt("Key_Dash", (int)KeyCode.Space),
                                  PlayerPrefs.GetInt("Key_Recover",(int)KeyCode.Q)};

        PlayerPrefs.SetInt("Key_Up", defaultKeys[0]);
        PlayerPrefs.SetInt("Key_Down", defaultKeys[1]);
        PlayerPrefs.SetInt("Key_Left", defaultKeys[2]);
        PlayerPrefs.SetInt("Key_Right", defaultKeys[3]);
        PlayerPrefs.SetInt("Key_Dash", defaultKeys[4]);
        PlayerPrefs.SetInt("Key_Recover", defaultKeys[5]);

        for (int i = 0; i < (int)KeyAction.COUNT; i++)
        {
            if (!KeySetting.keys.ContainsKey((KeyAction)i))                 // Dictionary keys에 값이 없다면
            {
                KeySetting.keys.Add((KeyAction)i, (KeyCode)defaultKeys[i]);          // keys값에 default값 대입
            }
        }

        initialized = true;
        RefreshMuteMarks();
        RefreshKeyTexts();
    }

    private void Update()
    {
        if (!initialized)
            return;

        if (!panel.activeInHierarchy)
            CancelKeyChange();

        RefreshMuteMarks();
        RefreshKeyTexts();
    }

    private void RefreshMuteMarks()
    {
        wUnMark.SetActive(muteAllVolume);
        bUnMark.SetActive(muteBgmVolume);
        sUnMark.SetActive(muteSfxVolume);
    }

    public void SaveMuteSettings()
    {
        if (!initialized)
            return;

        PlayerPrefs.SetInt("Mute_All", Convert.ToInt32(muteAllVolume));
        PlayerPrefs.SetInt("Mute_Bgm", Convert.ToInt32(muteBgmVolume));
        PlayerPrefs.SetInt("Mute_Sfx", Convert.ToInt32(muteSfxVolume));
    }

    public void CursorSizeSetting(int num)
    {
        if (!initialized) return;
        PlayerPrefs.SetInt("CursorSize", num);
        normalToggle.SetActive(!Convert.ToBoolean(PlayerPrefs.GetInt("CursorSize", 0)));
        doubleToggle.SetActive(!normalToggle.activeSelf);
        gameManager.cursorSize = num;
        gameManager.useCursorNormal = gameManager.cursorNormal[num];
        gameManager.useCursorAttack = gameManager.cursorAttack[num];
        Texture2D useCursorNormal = gameManager.useCursorNormal;
        Vector2 cursorHotSpot = new Vector3(useCursorNormal.width * 0.5f, useCursorNormal.height * 0.5f);
        Cursor.SetCursor(useCursorNormal, cursorHotSpot, CursorMode.ForceSoftware);
    }

    public void ChangeWholeVolume()
    {
        if (!initialized) return;
        if (panel.activeSelf)
        {
            if (AllSound.value == 0)
            {
                muteAllVolume = true;
                muteBgmVolume = muteAllVolume;
                muteSfxVolume = muteAllVolume;
            }

            else
            {
                muteAllVolume = false;
                muteBgmVolume = muteAllVolume;
                muteSfxVolume = muteAllVolume;
            }

            soundManager.WholeVolume(AllSound.value, muteBgmVolume, muteSfxVolume);
        }
    }

    public void OnOffWholeVolume()
    {
        if (!initialized) return;
        if (AllSound.value != 0)
            muteAllVolume = !muteAllVolume;

        if (bgmSound.value != 0)
            muteBgmVolume = muteAllVolume;

        if (sfxSound.value != 0)
            muteSfxVolume = muteAllVolume;

        soundManager.WholeVolumeOnOff(muteAllVolume);
    }

    public void ChangeBgmVolume()
    {
        if (!initialized) return;
        if (panel.activeSelf)
        {
            if (bgmSound.value == 0)
                muteBgmVolume = true;

            else
                muteBgmVolume = false;

            soundManager.BgmVolume(bgmSound.value, muteBgmVolume);
        }
    }

    public void OnOffBgmVolume()
    {
        if (!initialized) return;
        muteBgmVolume = !muteBgmVolume;
        soundManager.BgmOnOff(muteBgmVolume);

        if (muteBgmVolume == false)
            muteAllVolume = false;
    }

    public void ChangeSfxVolume()
    {
        if (!initialized) return;
        if (panel.activeSelf)
        {
            if (sfxSound.value == 0)
                muteSfxVolume = true;

            else
                muteSfxVolume = false;

            soundManager.SfxVolume(sfxSound.value, muteSfxVolume);
        }
    }

    public void OnOffSfxVolume()
    {
        if (!initialized) return;
        muteSfxVolume = !muteSfxVolume;
        soundManager.SfxOnOff(muteSfxVolume);

        if (muteSfxVolume == false)
            muteAllVolume = false;
    }

    private void RefreshKeyTexts()
    {
        if (keyTexts == null)
            return;

        for (int i = 0; i < keyTexts.Length && i < (int)KeyAction.COUNT; i++)
        {
            if (keyTexts[i] != null)
                keyTexts[i].text = KeySetting.keys[(KeyAction)i].ToString();
        }
    }

    public void CancelKeyChange()
    {
        key = -1;
        checkKey = -1;
    }

    private void OnDisable()
    {
        CancelKeyChange();
    }
    private void OnGUI()
    {
        if (!initialized || !panel.activeInHierarchy || key < 0 || key >= (int)KeyAction.COUNT)
            return;

        Event keyEvent = Event.current;     // 지금 진행되고 있는 이벤트 확인

        if (keyEvent.type == EventType.KeyDown && keyEvent.keyCode != KeyCode.None)                 // 키보드 입력이 있다면 
        {
            // ESC는 인게임 옵션 닫기와 충돌하지 않도록 키 변경 취소용으로 예약합니다.
            if (keyEvent.keyCode == KeyCode.Escape)
            {
                CancelKeyChange();
                keyEvent.Use();
                return;
            }

            KeySetting.keys[(KeyAction)key] = keyEvent.keyCode;     // 지금 입력된 키를 받아와 KeySetting 클래스의 Dictionary형 변수 keys의 key번째에 등록.

            switch((KeyAction)key)
            {
                case KeyAction.UP:
                    PlayerPrefs.SetInt("Key_Up", (int)keyEvent.keyCode);
                    break;

                case KeyAction.DOWN:
                    PlayerPrefs.SetInt("Key_Down", (int)keyEvent.keyCode);
                    break;

                case KeyAction.LEFT:
                    PlayerPrefs.SetInt("Key_Left", (int)keyEvent.keyCode);
                    break;

                case KeyAction.RIGHT:
                    PlayerPrefs.SetInt("Key_Right", (int)keyEvent.keyCode);
                    break;

                case KeyAction.DASH:
                    PlayerPrefs.SetInt("Key_Dash", (int)keyEvent.keyCode);
                    break;

                case KeyAction.RECORVER:
                    PlayerPrefs.SetInt("Key_Recover", (int)keyEvent.keyCode);
                    break;
            }

            // 똑같은 키가 존재하면 존재하던 키를 none으로 교체
            for (int i = 0; i < defaultKeys.Length; i++)
            {
                if (i != checkKey)
                {
                    if (KeySetting.keys[(KeyAction)i] == KeySetting.keys[(KeyAction)checkKey])
                    {
                        switch (i)
                        {
                            case 0:
                                PlayerPrefs.SetInt("Key_Up", 0);
                                KeySetting.keys.Remove((KeyAction)i);
                                KeySetting.keys.Add((KeyAction)i, KeyCode.None);
                                break;

                            case 1:
                                PlayerPrefs.SetInt("Key_Down", 0);
                                KeySetting.keys.Remove((KeyAction)i);
                                KeySetting.keys.Add((KeyAction)i, KeyCode.None);
                                break;

                            case 2:
                                PlayerPrefs.SetInt("Key_Left", 0);
                                KeySetting.keys.Remove((KeyAction)i);
                                KeySetting.keys.Add((KeyAction)i, KeyCode.None);
                                break;

                            case 3:
                                PlayerPrefs.SetInt("Key_Right", 0);
                                KeySetting.keys.Remove((KeyAction)i);
                                KeySetting.keys.Add((KeyAction)i, KeyCode.None);
                                break;

                            case 4:
                                PlayerPrefs.SetInt("Key_Dash", 0);
                                KeySetting.keys.Remove((KeyAction)i);
                                KeySetting.keys.Add((KeyAction)i, KeyCode.None);
                                break;

                            case 5:
                                PlayerPrefs.SetInt("Key_Recover", 0);
                                KeySetting.keys.Remove((KeyAction)i);
                                KeySetting.keys.Add((KeyAction)i, KeyCode.None);
                                break;
                        }
                    }
                }
            }

            CancelKeyChange();
            RefreshKeyTexts();
            keyEvent.Use();
        }
    }

    public void ChangeKey(int num)
    {
        if (!initialized || !panel.activeInHierarchy || num < 0 || num >= (int)KeyAction.COUNT)
            return;

        key = num;      // 버튼에 함수를 등록시키고 num값을 받아와 key값에 num값을 대입
        checkKey = key;
    }
    public void TextFx()
    {
        soundManager.PlaySFX("WeaponChange");
    }

    #endregion
    private void OnDestroy()
    {
        SaveMuteSettings();
    }
}
