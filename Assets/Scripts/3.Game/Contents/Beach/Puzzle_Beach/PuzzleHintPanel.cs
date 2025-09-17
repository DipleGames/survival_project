using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleHintPanel : BasicPanel
{
    [SerializeField] BasicPanel shellmakehintpanel;
    public override void ClosePanel()
    {
        if (shellmakehintpanel != null) { shellmakehintpanel.ClosePanel(); }
        base.ClosePanel();
    }
}
