using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStatConfig", menuName = "GameData/Player Stat")]
public class PlayerStatConfig : ScriptableObject
{
    public List<Stat> initialStats = new();
}