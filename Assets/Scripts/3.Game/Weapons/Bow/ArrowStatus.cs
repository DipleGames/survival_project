using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowStatus : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float damage;

    public float Speed => speed;
    public float Damage => damage;
}
