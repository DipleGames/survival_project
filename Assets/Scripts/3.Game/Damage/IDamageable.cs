using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    void Attacked(float damage, GameObject hitObject);
    void RendDamageUI(float damage, Vector3 rendPos, bool canCri, bool isCri);
}
