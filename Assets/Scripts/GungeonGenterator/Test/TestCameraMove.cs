using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCameraMove : MonoBehaviour
{
    [SerializeField] private Transform target;

    private void LateUpdate()
    {
        if (target == null) return;

        transform.position = new Vector3(
            target.position.x,
            target.position.y,
            transform.position.z
        );
    }
}
