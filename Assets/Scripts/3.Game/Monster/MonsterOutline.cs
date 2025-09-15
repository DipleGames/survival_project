using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterOutline : MonoBehaviour
{
    [SerializeField] Renderer monsterRenderer;
    [SerializeField] Material monsterOutline;

    MaterialPropertyBlock mpb;

    int outlineWidth;

    bool isEnabled = false;

    private void Awake()
    {
        mpb = new MaterialPropertyBlock();

        outlineWidth = Shader.PropertyToID("_Thickness");
    }

    public void SetOutLine(bool enabled)
    {
        isEnabled = enabled;

        mpb.SetFloat(outlineWidth, isEnabled ? 5f : 0f);
    }

}
