using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ShellColor
{
    RED = 2030017,
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
    private void Start()
    {
        if((int)ShellColor.RED != SetPuzzleAnswer.Instance.ShellID) 
        {
            Debug.LogError("ShellColor doesn't match");
        }
    }
    public void InitShellColor(ShellColor color)
    {
        index = color;
        image.sprite = Resources.Load<Sprite>($"Item/{(int)index}");
    }
}
