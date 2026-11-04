using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 이 스크립트가 붙은 오브젝트에는 Camera 컴포넌트가 반드시 있어야 함을 명시
[RequireComponent(typeof(Camera))]
public class MinimapControl : MonoBehaviour
{
    private Camera minimapCamera;
    [SerializeField] GameObject[] panels;

    private void Awake()
    {
        minimapCamera = GetComponent<Camera>();
    }


    private void LateUpdate()
    {
        bool isAnyPanelOpen = false;

        foreach (GameObject panel in panels)
        {
            if (panel != null && panel.activeInHierarchy)
            {
                isAnyPanelOpen = true;
                break;
            }
        }

        minimapCamera.enabled = !isAnyPanelOpen;
    }
}
