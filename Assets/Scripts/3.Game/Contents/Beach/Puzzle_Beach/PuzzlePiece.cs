using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ShellColor
{
    RED,
    YELLOW,
    GREEN,
    BLUE,
    PURPLE,
}
public class PuzzlePiece : MonoBehaviour
{
    [SerializeField] ShellColor index;

    Image image;
    public ShellColor Index => index;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void InitShellColor(ShellColor color)
    {
        index = color;
        image.sprite = Resources.Load<Sprite>($"Item/{2030017+index}");
    }
}
