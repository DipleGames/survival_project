using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFloater : Singleton<UIFloater>
{
    [Header("Prefab")]
    [SerializeField] GameObject clickUIPrefab;

    Object currentOwner;
    GameObject clickUI;

    protected override void Awake()
    {
        base.Awake();
        clickUI = Instantiate(clickUIPrefab);
    }

    public void RequestShowClickUI(Transform target, Object requester)
    {
        currentOwner = requester; // 최신 진입자가 소유
        clickUI.transform.position = target.position;
        clickUI.SetActive(true);
    }

    public void RequestHideClickUI(Object requester)
    {
        // 소유자만 끌 수 있음 (다른 애가 나가면서 끄는 것 방지)
        if (currentOwner == requester)
        {
            clickUI.SetActive(false);
            currentOwner = null;
        }
    }
}
