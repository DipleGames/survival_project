using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tombstone : InteractableObject
{
    [SerializeField] GameObject PuzzleHintUI;
    private void Awake()
    {
        if (PuzzleHintUI == null) Debug.Log("No PuzzleHintUI assigned!");
    }
    public override void InteractionLeftButtonFuc(GameObject hitObject)
    {
        if(isApproach)
        {
            PuzzleHintUI.SetActive(true);
        }
    }
}
