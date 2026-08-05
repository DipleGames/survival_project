using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;
using UnityEngine.UI;

public enum KeyAction { UP, DOWN ,LEFT, RIGHT, DASH, RECORVER, COUNT}

public static class KeySetting { public static Dictionary<KeyAction, KeyCode> keys = new Dictionary<KeyAction, KeyCode>(); }

public class TitleOption : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] Slider AllSound;
    [SerializeField] Slider bgmSound;
    [SerializeField] Slider sfxSound;
    [SerializeField] GameObject wUnMark;
    [SerializeField] GameObject bUnMark;
    [SerializeField] GameObject sUnMark;
    [SerializeField] GameObject normalToggle;
    [SerializeField] GameObject doubleToggle;
    [SerializeField] GameObject[] optionPanels;
    [SerializeField] Text[] keyTexts;

    SoundManager soundManager;
    GameManager gameManager;

    bool muteAllVolume;
    bool muteBgmVolume;
    bool muteSfxVolume;

    int[] defaultKeys;
    int key = -1;
    int checkKey = -1;

    int currentOptionIndex = 0;

    private void Start()
    {
        panel.SetActive(false);

        soundManager = SoundManager.Instance;
        gameManager = GameManager.Instance;

        normalToggle.SetActive(!Convert.ToBoolean(PlayerPrefs.GetInt("CursorSize", 0)));
        doubleToggle.SetActive(!normalToggle.activeSelf);

        AllSound.value = PlayerPrefs.GetFloat("Sound_All");
        bgmSound.value = PlayerPrefs.GetFloat("Sound_Bgm");
        sfxSound.value = PlayerPrefs.GetFloat("Sound_Sfx");

        muteAllVolume = Convert.ToBoolean(PlayerPrefs.GetInt("Mute_All", 0));
        muteBgmVolume = Convert.ToBoolean(PlayerPrefs.GetInt("Mute_Bgm", 0));
        muteSfxVolume = Convert.ToBoolean(PlayerPrefs.GetInt("Mute_Sfx", 0));

        wUnMark.SetActive(muteAllVolume);
        bUnMark.SetActive(muteBgmVolume);
        sUnMark.SetActive(muteSfxVolume);

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

        for (int i = 0; i < keyTexts.Length; i++)
        {
            keyTexts[i].text = KeySetting.keys[(KeyAction)i].ToString();    // keyText에 keys의 keyCode값 대입
        }

        currentOptionIndex = PlayerPrefs.GetInt("Option_Index", 0);

        for (int i = 0; i < optionPanels.Length; ++i)
        {
            optionPanels[i].SetActive(i == currentOptionIndex);
        }
    }

    private void Update()
    {
        wUnMark.SetActive(muteAllVolume);
        bUnMark.SetActive(muteBgmVolume);
        sUnMark.SetActive(muteSfxVolume);

        for (int i = 0; i < keyTexts.Length; i++)
        {
            keyTexts[i].text = KeySetting.keys[(KeyAction)i].ToString();
        }
    }

    public void CursorSizeSetting(int num)
    {
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

    // GUI, 키 입력등의 이벤트가 발생할 때 호출되는 함수.
    private void OnGUI()
    {
        Event keyEvent = Event.current;     // 지금 진행되고 있는 이벤트 확인

        if (keyEvent.isKey)                 // 키보드 입력이 있다면 
        {
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

            KeyCode keycode = keyEvent.keyCode;

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

            key = -1;
        }
    }

    public void ChangeKey(int num)
    {
        key = num;      // 버튼에 함수를 등록시키고 num값을 받아와 key값에 num값을 대입
        checkKey = key;
    }

    public void ChangeWholeVolume()
    {
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
        muteBgmVolume = !muteBgmVolume;
        soundManager.BgmOnOff(muteBgmVolume);

        if (muteBgmVolume == false)
            muteAllVolume = false;
    }

    public void ChangeSfxVolume()
    {
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
        muteSfxVolume = !muteSfxVolume;
        soundManager.SfxOnOff(muteSfxVolume);

        if (muteSfxVolume == false)
            muteAllVolume = false;
    }

    public void BackTitle()
    {
        PlayerPrefs.SetInt("Mute_All", Convert.ToInt32(muteAllVolume));
        PlayerPrefs.SetInt("Mute_Bgm", Convert.ToInt32(muteBgmVolume));
        PlayerPrefs.SetInt("Mute_Sfx", Convert.ToInt32(muteSfxVolume));
        panel.SetActive(false);
    }

    public void TextFx()
    {
        soundManager.PlaySFX("WeaponChange");
    }

    public void OptionIndex(int index)
    {
        PlayerPrefs.SetInt("Option_Index", index);

        for (int i = 0; i < optionPanels.Length; ++i)
        {
            optionPanels[i].SetActive(i == index);
        }
    }
}
