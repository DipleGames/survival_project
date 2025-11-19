using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterOutline : MonoBehaviour
{
    [SerializeField] SpriteRenderer monsterRenderer;
    [SerializeField] Material outline_Off;
    [SerializeField] Material outline_On;

    // MaterialPropertyBlock mpb;

    bool isEnabled;

    private void Awake()
    {
        // mpb = new MaterialPropertyBlock();

        monsterRenderer.material = outline_Off;
    }

    private void OnEnable()
    {
        isEnabled = false;
    }

    public void SetOutLine(bool enabled)
    {
        isEnabled = enabled;

        if(isEnabled)
        {
            monsterRenderer.material = outline_On;
        }
        else
        {
            monsterRenderer.material = outline_Off;
        }
    }

}
