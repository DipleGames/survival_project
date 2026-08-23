using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    public static FloatingText Instance; 
    
    [SerializeField] GameObject panel;
    [SerializeField] TMP_Text text;

    private Canvas canvas;
    RectTransform panelRect;
    Vector3 offset;

    private void Awake()
    {
        Instance = this;
        canvas = panel.GetComponentInParent<Canvas>();
        offset = Vector3.zero;
        Hide();
        panelRect = panel.GetComponent<RectTransform>();
    }

    private void Update()
    {
        RectTransform canvasRect = canvas.transform as RectTransform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            Input.mousePosition,
            canvas.worldCamera,
            out Vector2 localPos);

        panelRect.anchoredPosition = localPos + (Vector2)offset;
    }

    public void Show(string message)
    {
        text.text = message;
        panel.SetActive(true);
    }
    public void Show(string message, Vector3 offset)
    {
        this.offset = offset;
        Show(message);
    }

    public void Hide()
    {
        panel.SetActive(false);
        offset = Vector3.zero;
    }
}
