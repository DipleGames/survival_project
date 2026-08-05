using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class AttackCutlass : MonoBehaviour
{
    [SerializeField] float damage;

    GameManager gameManager;
    Character character;

    private void Awake()
    {
        gameManager = GameManager.Instance;
        character = Character.Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<IDamageable>() != null)
        {
            bool isCri = gameManager.status[Status.Critical] >= Random.Range(0f, 100f);

            float realDamage = (damage + gameManager.status[Status.Damage] + gameManager.status[Status.CloseDamage] + gameManager.bloodDamage) * (100 + character.percentDamage) * 0.01f;

            realDamage *= isCri ? 2 : 1;

            other.GetComponent<IDamageable>().Attacked(realDamage, this.gameObject);
            other.GetComponent<IDamageable>().RendDamageUI(realDamage, other.transform.position, true, isCri);
        }
    }
}
