using UnityEngine;
using UnityEngine.Pool;

public class ObjectPooling<className> : MonoBehaviour
    where className : MonoBehaviour
{
    private IObjectPool<className> managedPool;

    public void SetManagedPool(IObjectPool<className> pool)
    {
        managedPool = pool;
    }

    public void ReleasePool()
    {
        managedPool.Release(this as className);
    }
}
