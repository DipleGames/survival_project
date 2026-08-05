using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class DamageUIManager : Singleton<DamageUIManager>
{
    [SerializeField] DamageUI damageUI;

    IObjectPool<DamageUI> damagePool;

    protected override void Awake()
    {
        base.Awake();
        damagePool = new ObjectPool<DamageUI>(CreateDamageUI, OnGetDamageUI, OnReleaseDamageUI, OnDestroyDamageUI, maxSize: 20);
    }

    public void RendDamageUI(float damage, Vector3 rendPos, bool canCri, bool isCri)
    {
        DamageUI _damageUI = damagePool.Get();
        _damageUI.UISetting(canCri, isCri, damage);
        _damageUI.transform.position = rendPos;
    }

    private DamageUI CreateDamageUI()
    {
        DamageUI damageUIPool = Instantiate(damageUI, transform.position, Quaternion.Euler(90, 0, 0), transform).GetComponent<DamageUI>();
        damageUIPool.SetManagedPool(damagePool);
        return damageUIPool;
    }

    private void OnGetDamageUI(DamageUI damageUIPool)
    {
        damageUIPool.gameObject.SetActive(true);
    }

    private void OnReleaseDamageUI(DamageUI damageUIPool)
    {
        damageUIPool.gameObject.SetActive(false);
    }

    private void OnDestroyDamageUI(DamageUI damageUIPool)
    {
        if (damageUIPool.gameObject != null)
        {
            Destroy(damageUIPool.gameObject);
        }
    }
}
