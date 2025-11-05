using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms;

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

    public void OnDamaged(float damage)
    {
        hp -= damage;

        if (hp <= 0)
        {
            soundManager.PlaySFX(damagedSound);
            OnDead();

            if (monsterOutline != null)
                monsterOutline.SetOutLine(false);
        }

        else
        {
            if (runningCoroutine != null)
                StopCoroutine(runningCoroutine);

            runningCoroutine = MonsterColorBlink();
            StartCoroutine(runningCoroutine);
        }
    }

    void AreaDamage(float distance, bool isCri)
    {
        impactRadius = 0.5f + distance * 0.1f;
        detactLayer = 1 << LayerMask.NameToLayer("MonsterAttaked");
        var detected = Physics.OverlapSphere(transform.position, impactRadius, detactLayer);

        Debug.Log("impactRadius: " + impactRadius);
        Debug.Log("detected List: " + detected.Length);

        if (detected != null)
        {
            Debug.Log("detectedOne: " + detected.GetValue(0));

            foreach (BoxCollider monster in detected)
            {
                Transform target = monster.transform.parent;
                Debug.Log("target: " + target.gameObject.name);

                target.GetComponent<Monster>().Attacked(damage * 0.7f, monster.gameObject);
                target.GetComponent<Monster>().RendDamageUI(damage * 0.7f, monster.transform.position, true, isCri);
            }
        }
    }

}
