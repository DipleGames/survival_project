using UnityEngine;

public class Singleton<T> : MonoBehaviour
    where T : MonoBehaviour                             // T 자료형은 Monobehaviour를 상속하고 있어야 한다.
{
    static T instance;
    public static T Instance => instance;

    protected virtual void Awake()                              // ObjectPool의 Awake에서 호출하기 위해 Protected
    {
        instance = this as T;                           // 나(Singleton)을 T형으로 변환후 대입
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}
