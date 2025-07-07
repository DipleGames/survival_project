using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzlePiece : MonoBehaviour
{
    [SerializeField] int index;
    public int Index => index;

    public void InitIndex(int _index)
    {
        index = _index; 
    }
}
