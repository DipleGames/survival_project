using UnityEngine;

public class TitleOption : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] OptionSettingsUI optionSettings;
    [SerializeField] GameObject[] optionPanels;

    SoundManager soundManager;

    private void Start()
    {
        panel.SetActive(false);
        soundManager = SoundManager.Instance;
        optionSettings.Initialize(panel);
        OptionIndex(PlayerPrefs.GetInt("Option_Index", 0));
    }

    public void BackTitle()
    {
        optionSettings.SaveMuteSettings();
        optionSettings.CancelKeyChange();
        panel.SetActive(false);
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
