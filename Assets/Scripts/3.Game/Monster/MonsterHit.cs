using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterHit : MonoBehaviour, IDamageable
{
    DamageUIManager damageUIManager;
    Monster monster;

    private void Awake()
    {
        monster = transform.parent.gameObject.GetComponent<Monster>();
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

    public void GetStunned(float duration)
    {
        // Coroutine으로 처리해야 하는가?

        monster.canAttack = false;
        monster.canMove = false;

        float time = 0f;
        while(time < duration)
        {
            time += Time.deltaTime;
        }

        monster.canAttack = true;
        monster.canMove = true;
    }

    IEnumerator StunCorutine(float duration)
    {
        monster.canAttack = false;
        monster.canMove = false;

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            yield return null;
        }

        monster.canAttack = true;
        monster.canMove = true;
    }

}
