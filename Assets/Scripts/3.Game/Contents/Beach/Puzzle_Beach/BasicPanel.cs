using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.UI;

public class BasicPanel : MonoBehaviour
{
    [SerializeField]Button closeButton;

    private void OnValidate()
    {
        closeButton = transform.Find("CloseButton").GetComponent<Button>();
        closeButton.onClick.AddListener(ClosePanel);
    }

    public virtual void OpenPanel()
    {
        gameObject.SetActive(true);
    }
    public virtual void ClosePanel()
    {
        gameObject.SetActive(false);
    }
    public virtual void TogglePanel()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
