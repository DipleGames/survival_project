using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class BuffIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Cache")]
    [SerializeField] Image icon;
    [SerializeField] TMP_Text stacks;
    [SerializeField] BuffDescriptionDisplayer descriptionPanel;
    [SerializeField] ImageFader fader;

    Sprite _iconCache;
    string description;

    Buff buff;

    private void Awake()
    {
        if (fader == null) TryGetComponent(out fader);
    }
    public void Bind(Buff _buff)
    {
        buff = _buff;

        if (!string.IsNullOrEmpty(_buff.IconPath))
        {
            string iconPath = _buff.IconPath;
            _iconCache = Resources.Load<Sprite>(iconPath);
            if(_iconCache == null) { Debug.Log("No Resource on this path : " + iconPath); }
            icon.sprite = _iconCache;
        }

        stacks.text = buff.MaxStacks <= 1 ? "" : buff.MaxStacks.ToString();

        description = buff.Description;
    }

    void Update()
    {
        if (buff == null) return;
        if (buff.Duration <= 0f) return; // 무한 버프 등

        float progress = Mathf.Clamp01(buff.ElapsedTime / buff.Duration);
        if (fader != null) { fader.SetRatio(progress); }
    }


    public void OnPointerEnter(PointerEventData e)
    {
        if (buff == null) return;

        descriptionPanel.gameObject.SetActive(true);
        descriptionPanel.Show(description);
    }
    public void OnPointerExit(PointerEventData e) 
    {
        descriptionPanel.gameObject.SetActive(false); 
    }
}
