using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SkullInteraction : InteractableObj
{
    [SerializeField] private GameObject player;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private SkullView _skullView;

    private Color _outlineColor;
    
    private void Start()
    {
        player = GameObject.FindWithTag("Character");
        _outlineColor = _spriteRenderer.material.GetColor("_SolidOutline");
        _outlineColor.a = 0f;
        _spriteRenderer.material.SetColor("_SolidOutline", _outlineColor);
        _skullView = GetComponent<SkullView>();
    }

    public override void InteractionLeftButtonFuc(GameObject hitObject)
    {
        if (!canInteract)
            return;

        _skullView.SwitchSkullUI();
    }

    public override void InteractionRightButtonFuc(GameObject hitObject)
    {
        
    }

    public override void BeginInteraction()
    {
        
    }
    public override void EndInteraction()
    {
        
    }

    public override void OnHoverEnter()
    {
        
    }
    public override void OnHoverExit()
    {
        
    }

    private void OnMouseOver()
    {
        canInteract = Vector3.Distance(player.transform.position, transform.position) < 2f;
        Debug.Log("OnMouseOver 실행");
        if (canInteract)
        {
            if (_outlineColor.a == 1)
                return;

            _outlineColor.a = 1;
            _spriteRenderer.material.SetColor("_SolidOutline", _outlineColor);
        }
        else
        {
            if (_outlineColor.a == 0)
                return;

            _outlineColor.a = 0;
            _spriteRenderer.material.SetColor("_SolidOutline", _outlineColor);
        }
    }

    private void OnMouseExit()
    {
        canInteract = false;

        if (_outlineColor.a == 0)
            return;

        _outlineColor.a = 0;
        _spriteRenderer.material.SetColor("_SolidOutline", _outlineColor);
    }

}
