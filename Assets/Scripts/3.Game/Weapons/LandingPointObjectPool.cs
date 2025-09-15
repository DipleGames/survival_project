using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class LandingPointObjectPool : MonoBehaviour
{
    IObjectPool<LandingPointObjectPool> objectPool;
    [SerializeField] Transform LandingpointParent;

    GameManager gameManager;

    void Start()
    {
        gameManager = GameManager.Instance;
    }

    public void SetManagedPool(IObjectPool<LandingPointObjectPool> pool)
    {
        objectPool = pool;
    }
    protected void ShowLandingPoint(LandingPointObjectPool poolObject)
    {
        poolObject.enabled = true;
    }
    protected void HideLandingPoint()
    {
        if (gameObject.activeSelf)
        {
            objectPool.Release(this);
        }
    }

}
