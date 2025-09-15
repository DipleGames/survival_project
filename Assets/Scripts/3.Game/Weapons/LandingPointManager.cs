using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Pool;

public class LandingPointManager : Singleton<LandingPointManager>
{
    [Header("Pool Setting")]
    [SerializeField] protected GameObject landingPointPrefab;
    [SerializeField] protected int poolCount;
    [SerializeField] protected AudioClip impactSound;
    Transform targetPos;

    protected IObjectPool<LandingPointObjectPool> landingPointPool;
    protected LandingPointObjectPool landingPoint;

    protected GameManager gameManager;
    protected SoundManager soundManager;
    protected WeaponManager weaponManager;

    protected virtual void Awake()
    {
        base.Awake();
        landingPointPool = new ObjectPool<LandingPointObjectPool>(CreatePool, OnGetPool, OnReleasePool, OnDestroyPool, maxSize: poolCount);

        gameManager = GameManager.Instance;
        soundManager = SoundManager.Instance;
        weaponManager = WeaponManager.Instance;
    }

    protected virtual LandingPointObjectPool CreatePool()
    {
        LandingPointObjectPool pool = Instantiate(landingPointPrefab, targetPos.position, Quaternion.identity).GetComponent<LandingPointObjectPool>();
        pool.SetManagedPool(landingPointPool);
        pool.transform.SetParent(gameManager.landingPoints);
        return pool;
    }

    protected virtual void OnGetPool(LandingPointObjectPool landingPoint)
    {
        landingPoint.gameObject.SetActive(true);
    }

    protected virtual void OnReleasePool(LandingPointObjectPool landingPoint)
    {
        landingPoint.gameObject.SetActive(false);
    }

    protected virtual void OnDestroyPool(LandingPointObjectPool landingPoint)
    {
        Destroy(landingPoint.gameObject);
    }

    protected void SetLandingPoint(Vector3 targetPos)
    {
        landingPoint = landingPointPool.Get();
        landingPoint.transform.position = targetPos;
    }



}
