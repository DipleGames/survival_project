using UnityEngine;
using System.Collections;

public class Tree : GatherableObj
{
    [Header("Tree 전용 설정")]
    [SerializeField] Sprite normalTreeSprite;
    [SerializeField] Sprite choppedTreeSprite;


    SpriteRenderer spriteRenderer;
    Animator animator;

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected override bool CanStartInteraction()
    {
        return currentToolType == ToolType.Axe || currentToolType == ToolType.Lighter;
    }

    protected override void OnInteractionFinished()
    {
        switch (currentToolType)
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
        spriteRenderer.sprite = choppedTreeSprite;

        switch (currentToolType)
        {
            case ToolType.Axe:
                Debug.Log("Tree is chopped down");
                break;
            case ToolType.Lighter:
                Debug.Log("Tree is burnt down");
                break;
        }
    }

    protected override void ResetGatherState()
    {
        base.ResetGatherState();
        spriteRenderer.sprite = normalTreeSprite;
    }
}