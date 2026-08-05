using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterHit : MonoBehaviour, IDamageable
{
    DamageUIManager damageUIManager;

    private void Awake()
    {
        damageUIManager = DamageUIManager.Instance;
    }

    public void Attacked(float damage, GameObject hitObject)
    {
        transform.parent.GetComponent<Monster>().OnDamaged(damage);
    }

    public void RendDamageUI(float damage, Vector3 rendPos, bool canCri, bool isCri)
    {
        damageUIManager.RendDamageUI(damage, rendPos, canCri, isCri);
    }
}
