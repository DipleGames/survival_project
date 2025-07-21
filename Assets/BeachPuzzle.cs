using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeachPuzzle : InteractableObject
{
    [SerializeField] GameObject beachPuzzleUI;
    private void Awake()
    {
        if (beachPuzzleUI == null) Debug.Log("No PuzzleHintUI assigned!");
    }
    public override void InteractionLeftButtonFuc(GameObject hitObject)
    {
        if (isApproach)
        {
            beachPuzzleUI.SetActive(true);
        }
    }
}
