using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BuffDescriptionDisplayer : MonoBehaviour
{
    [Header("Cache")]
    [SerializeField] TMP_Text descriptionText;

    private void Awake()
    {
        if (descriptionText == null) descriptionText = transform.GetComponentInChildren<TMP_Text>();
    }
    public void Show(string description)
    {
        descriptionText.gameObject.SetActive(true);
        descriptionText.text = description;
    }

    public void Hide()
    {
        descriptionText.gameObject.SetActive(false);
    }
}
