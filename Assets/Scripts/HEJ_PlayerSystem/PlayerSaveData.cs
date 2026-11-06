using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class PlayerSaveData
{
    public List<string> equippedItemIDs = new();
    public float currentHp;
}