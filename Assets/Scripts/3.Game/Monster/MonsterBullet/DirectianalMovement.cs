using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectianalMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    [SerializeField] float shootRange;

    Vector3 dir;
    float angle;
    Vector3 startPoint, endPoint;

    private void Update()
    {
        if(Vector3.Distance(transform.position, startPoint) >= shootRange)
        {
            transform.position = endPoint;
            return;
        }

        transform.position += dir * moveSpeed * Time.deltaTime;
    }

    public void SetDir(Vector3 _dir)
    {
        dir = _dir;

        angle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(90, -angle + 180, 0);

        startPoint = transform.position;
        endPoint = startPoint + dir * shootRange;

        GetComponent<MonsterBullet>().destroyPos = endPoint;
    }
}
