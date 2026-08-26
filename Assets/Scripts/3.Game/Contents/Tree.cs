using UnityEngine;
using System.Collections;
using UnityEngine.LowLevel;

public class Tree : GatherableObj
{
    [Header("Tree")]
    [SerializeField] int requiredHitCount = 3;
    [SerializeField] int currentHit;
    [SerializeField] string[] dropItems;
    [SerializeField] Sprite choppedTreeSprite;

    [Header("Temp")]
    [SerializeField] ToolType toolOfPlayer;

    Animator animator;

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponentInChildren<Animator>();
    }

    protected override bool CanStartInteraction()
    {
        // Check Player's tool
        if (toolOfPlayer == ToolType.Axe || toolOfPlayer == ToolType.Lighter)
        {
            return true;
        }

        return false;
    }
    protected override void OnInteractionFinished()
    {
        switch(toolOfPlayer)
        {
            case ToolType.Axe:
                currentHit++;
                if (currentHit < requiredHitCount)
                {
                    isInteracting = false;
                    return;
                }
                foreach (var item in dropItems)
                {
                    GetItem(item);
                }
                GetComponent<SpriteRenderer>().sprite = choppedTreeSprite;
                ChangeGatherState();
                break;
            case ToolType.Lighter:
                animator.SetTrigger("Burn");
                StartCoroutine(WaitForBurn());
                foreach (var item in dropItems)
                {
                    GetItem(item);
                }
                ChangeGatherState();
                break;
        }

    }

    IEnumerator WaitForBurn()
    {
        yield return CoroutineCaching.WaitForSeconds(3f);
        animator.SetTrigger("BurnEnd");
    }

    protected override void ChangeGatherState()
    {
        base.ChangeGatherState();
        switch(toolOfPlayer)
        {
            case ToolType.Axe:
                Debug.Log("Tree is chopped down");
                break;
            case ToolType.Lighter:
                Debug.Log("Tree is burnt down");
                break;
        }
    }
}