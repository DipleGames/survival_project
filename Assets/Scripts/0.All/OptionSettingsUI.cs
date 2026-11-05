using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum KeyAction
{
    UP,
    DOWN,
    LEFT,
    RIGHT,
    DASH,
    RECORVER,
    INVENTORY,
    COUNT
}

public static class KeySetting
{
    public static Dictionary<KeyAction, KeyCode> keys = new();
}

public class OptionSettingsUI : MonoBehaviour
{
    [Header("Sound")]
    [SerializeField] private Slider AllSound;
    [SerializeField] private Slider bgmSound;
    [SerializeField] private Slider sfxSound;
    [SerializeField] private GameObject wUnMark;
    [SerializeField] private GameObject bUnMark;
    [SerializeField] private GameObject sUnMark;

    [Header("Cursor")]
    [SerializeField] private GameObject normalToggle;
    [SerializeField] private GameObject doubleToggle;
    [SerializeField] private Texture2D[] normalCursorTextures;

    [Header("Key Bindings")]
    [SerializeField] private Text[] keyTexts = new Text[0];

    private static readonly string[] KeyPrefNames =
    {
        "Key_Up", "Key_Down", "Key_Left", "Key_Right",
        "Key_Dash", "Key_Recover", "Key_Inventory"
    };

    private static readonly KeyCode[] DefaultKeyCodes =
    {
        KeyCode.W, KeyCode.S, KeyCode.A, KeyCode.D,
        KeyCode.Space, KeyCode.Q, KeyCode.I
    };

    private GameObject panel;
    private SoundManager soundManager;

    private bool muteAllVolume;
    private bool muteBgmVolume;
    private bool muteSfxVolume;
    private bool initialized;

    private int waitingKeyIndex = -1;

    public bool IsWaitingForKey => waitingKeyIndex >= 0;

    /// <summary>
    /// 옵션 패널과 저장된 사운드, 커서, 키 설정을 초기화한다.
    /// </summary>
    public void Initialize(GameObject optionPanel)
    {
        if (initialized)
            return;

        panel = optionPanel;
        soundManager = SoundManager.Instance;

        if (AllSound != null)
            AllSound.SetValueWithoutNotify(PlayerPrefs.GetFloat("Sound_All"));

        if (bgmSound != null)
            bgmSound.SetValueWithoutNotify(PlayerPrefs.GetFloat("Sound_Bgm"));

        if (sfxSound != null)
            sfxSound.SetValueWithoutNotify(PlayerPrefs.GetFloat("Sound_Sfx"));

        muteAllVolume = PlayerPrefs.GetInt("Mute_All", 0) != 0;
        muteBgmVolume = PlayerPrefs.GetInt("Mute_Bgm", 0) != 0;
        muteSfxVolume = PlayerPrefs.GetInt("Mute_Sfx", 0) != 0;

        int cursorSize = Mathf.Clamp(PlayerPrefs.GetInt("CursorSize", 0), 0, 1);

        UpdateCursorToggles(cursorSize);
        ApplyCursorTexture(cursorSize);

        for (int i = 0; i < (int)KeyAction.COUNT; i++)
        {
            KeyCode keyCode = (KeyCode)PlayerPrefs.GetInt(KeyPrefNames[i], (int)DefaultKeyCodes[i]);

            KeySetting.keys[(KeyAction)i] = keyCode;
            PlayerPrefs.SetInt(KeyPrefNames[i], (int)keyCode);
        }

        initialized = true;

        RefreshMuteMarks();
        RefreshKeyTexts();
    }

    private void Update()
    {
        if (initialized && (panel == null || !panel.activeInHierarchy))
            CancelKeyChange();
    }

    /// <summary>
    /// 현재 음소거 상태를 UI에 반영한다.
    /// </summary>
    private void RefreshMuteMarks()
    {
        if (wUnMark != null)
            wUnMark.SetActive(muteAllVolume);

        if (bUnMark != null)
            bUnMark.SetActive(muteBgmVolume);

        if (sUnMark != null)
            sUnMark.SetActive(muteSfxVolume);
    }

    /// <summary>
    /// 음소거 설정을 저장한다.
    /// </summary>
    public void SaveMuteSettings()
    {
        if (!initialized)
            return;

        PlayerPrefs.SetInt("Mute_All", muteAllVolume ? 1 : 0);
        PlayerPrefs.SetInt("Mute_Bgm", muteBgmVolume ? 1 : 0);
        PlayerPrefs.SetInt("Mute_Sfx", muteSfxVolume ? 1 : 0);

        PlayerPrefs.Save();
    }

    /// <summary>
    /// 커서 크기 선택값을 저장하고 커서를 변경한다.
    /// </summary>
    public void CursorSizeSetting(int num)
    {
        if (!initialized)
            return;

        int cursorSize = Mathf.Clamp(num, 0, 1);

        PlayerPrefs.SetInt("CursorSize", cursorSize);

        UpdateCursorToggles(cursorSize);
        ApplyCursorTexture(cursorSize);

        PlayerPrefs.Save();
    }

    /// <summary>
    /// 선택한 커서 크기에 맞게 토글 UI를 변경한다.
    /// </summary>
    private void UpdateCursorToggles(int cursorSize)
    {
        if (normalToggle != null)
            normalToggle.SetActive(cursorSize == 0);

        if (doubleToggle != null)
            doubleToggle.SetActive(cursorSize == 1);
    }

    /// <summary>
    /// 선택한 크기의 일반 커서 이미지를 적용한다.
    /// </summary>
    private void ApplyCursorTexture(int cursorSize)
    {
        if (normalCursorTextures == null || cursorSize >= normalCursorTextures.Length)
            return;

        Texture2D texture = normalCursorTextures[cursorSize];

        if (texture == null)
            return;

        Vector2 hotspot = new Vector2(texture.width * 0.5f, texture.height * 0.5f);

        Cursor.SetCursor(texture, hotspot, CursorMode.ForceSoftware);
    }

    /// <summary>
    /// 전체 볼륨 슬라이더 변경을 적용한다.
    /// </summary>
    public void ChangeWholeVolume()
    {
        if (!initialized || panel == null || !panel.activeSelf || AllSound == null)
            return;

        muteAllVolume = AllSound.value <= 0f;
        muteBgmVolume = muteAllVolume;
        muteSfxVolume = muteAllVolume;

        SoundManager sound = GetSoundManager();

        if (sound != null)
            sound.WholeVolume(AllSound.value, muteBgmVolume, muteSfxVolume);

        RefreshMuteMarks();
    }

    /// <summary>
    /// 전체 음소거를 켜거나 끈다.
    /// </summary>
    public void OnOffWholeVolume()
    {
        if (!initialized)
            return;

        if (AllSound != null && AllSound.value != 0f)
            muteAllVolume = !muteAllVolume;

        if (bgmSound != null && bgmSound.value != 0f)
            muteBgmVolume = muteAllVolume;

        if (sfxSound != null && sfxSound.value != 0f)
            muteSfxVolume = muteAllVolume;

        SoundManager sound = GetSoundManager();

        if (sound != null)
            sound.WholeVolumeOnOff(muteAllVolume);

        RefreshMuteMarks();
    }

    /// <summary>
    /// BGM 볼륨 슬라이더 변경을 적용한다.
    /// </summary>
    public void ChangeBgmVolume()
    {
        if (!initialized || panel == null || !panel.activeSelf || bgmSound == null)
            return;

        muteBgmVolume = bgmSound.value <= 0f;

        SoundManager sound = GetSoundManager();

        if (sound != null)
            sound.BgmVolume(bgmSound.value, muteBgmVolume);

        RefreshMuteMarks();
    }

    /// <summary>
    /// BGM 음소거를 켜거나 끈다.
    /// </summary>
    public void OnOffBgmVolume()
    {
        if (!initialized)
            return;

        muteBgmVolume = !muteBgmVolume;

        if (!muteBgmVolume)
            muteAllVolume = false;

        SoundManager sound = GetSoundManager();

        if (sound != null)
            sound.BgmOnOff(muteBgmVolume);

        RefreshMuteMarks();
    }

    /// <summary>
    /// 효과음 볼륨 슬라이더 변경을 적용한다.
    /// </summary>
    public void ChangeSfxVolume()
    {
        if (!initialized || panel == null || !panel.activeSelf || sfxSound == null)
            return;

        muteSfxVolume = sfxSound.value <= 0f;

        SoundManager sound = GetSoundManager();

        if (sound != null)
            sound.SfxVolume(sfxSound.value, muteSfxVolume);

        RefreshMuteMarks();
    }

    /// <summary>
    /// 효과음 음소거를 켜거나 끈다.
    /// </summary>
    public void OnOffSfxVolume()
    {
        if (!initialized)
            return;

        muteSfxVolume = !muteSfxVolume;

        if (!muteSfxVolume)
            muteAllVolume = false;

        SoundManager sound = GetSoundManager();

        if (sound != null)
            sound.SfxOnOff(muteSfxVolume);

        RefreshMuteMarks();
    }

    /// <summary>
    /// SoundManager 참조를 반환한다.
    /// </summary>
    private SoundManager GetSoundManager()
    {
        if (soundManager == null)
            soundManager = SoundManager.Instance;

        return soundManager;
    }

    /// <summary>
    /// 현재 등록된 키 설정을 UI에 표시한다.
    /// </summary>
    private void RefreshKeyTexts()
    {
        if (keyTexts == null)
            return;

        for (int i = 0; i < keyTexts.Length && i < (int)KeyAction.COUNT; i++)
        {
            if (keyTexts[i] != null && KeySetting.keys.TryGetValue((KeyAction)i, out KeyCode code))
                keyTexts[i].text = code.ToString();
        }
    }

    /// <summary>
    /// 진행 중인 키 변경을 취소한다.
    /// </summary>
    public void CancelKeyChange()
    {
        waitingKeyIndex = -1;
    }

    /// <summary>
    /// 키 변경 버튼이 눌리면 새로운 키 입력을 기다린다.
    /// </summary>
    public void ChangeKey(int num)
    {
        if (!initialized || panel == null || !panel.activeInHierarchy ||
            num < 0 || num >= (int)KeyAction.COUNT)
            return;

        waitingKeyIndex = num;
    }

    /// <summary>
    /// 새로운 키를 입력받아 저장한다.
    /// 기존에 같은 키가 등록되어 있다면 해당 키 설정을 해제한다.
    /// ESC는 키 변경 취소에 사용한다.
    /// </summary>
    private void OnGUI()
    {
        if (!initialized || panel == null || !panel.activeInHierarchy || !IsWaitingForKey)
            return;

        Event keyEvent = Event.current;

        if (keyEvent.type != EventType.KeyDown || keyEvent.keyCode == KeyCode.None)
            return;

        if (keyEvent.keyCode == KeyCode.Escape)
        {
            CancelKeyChange();
            keyEvent.Use();
            return;
        }

        KeyCode newKey = keyEvent.keyCode;

        // 기존에 같은 키가 설정되어 있다면 해제
        for (int i = 0; i < (int)KeyAction.COUNT; i++)
        {
            if (i == waitingKeyIndex)
                continue;

            if (KeySetting.keys.TryGetValue((KeyAction)i, out KeyCode assignedKey) &&
                assignedKey == newKey)
            {
                KeySetting.keys[(KeyAction)i] = KeyCode.None;
                PlayerPrefs.SetInt(KeyPrefNames[i], (int)KeyCode.None);
            }
        }

        // 새로운 키 등록
        KeySetting.keys[(KeyAction)waitingKeyIndex] = newKey;
        PlayerPrefs.SetInt(KeyPrefNames[waitingKeyIndex], (int)newKey);

        PlayerPrefs.Save();

        CancelKeyChange();
        RefreshKeyTexts();

        keyEvent.Use();
    }

    /// <summary>
    /// 옵션 UI 효과음을 재생한다.
    /// </summary>
    public void TextFx()
    {
        SoundManager sound = GetSoundManager();

        if (sound != null)
            sound.PlaySFX("WeaponChange");
    }

    private void OnDisable()
    {
        CancelKeyChange();
    }

    private void OnDestroy()
    {
        SaveMuteSettings();
    }
}