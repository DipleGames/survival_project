using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterModel : MonoBehaviour
{
    [SerializeField] private int _monsterHP;
    [SerializeField] private int _monsterSize;

    public int MonsterHP => _monsterHP;
    public int MonsterSize => _monsterSize;
}
