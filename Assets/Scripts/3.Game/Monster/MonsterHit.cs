using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Monster : IDamageable
{
    DamageUIManager damageUIManager;

    public void Attacked(float damage, GameObject hitObject)
    {
        OnDamaged(damage);
    }

    public void RendDamageUI(float damage, Vector3 rendPos, bool canCri, bool isCri)
    {
        damageUIManager.RendDamageUI(damage, rendPos, canCri, isCri);
    }

    public void GetStunned(float duration)
    {
        Stunned(duration);
    }

}
