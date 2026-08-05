using UnityEngine;

public class ProjectileMovement : MonoBehaviour
{
    [SerializeField] float range;
    [SerializeField] float speed;

    Vector3 dir;
    Vector3 initPos;
    float angle;

    private void Update()
    {
        if (Vector3.Distance(transform.position, initPos) > range)
        {
            GetComponent<ProjectileObjectPool>().DestroyProjectile();
        }
    }

    private void FixedUpdate()
    {
        transform.position += dir * speed * Time.deltaTime;
    }

    public virtual void Shoot(Vector3 dir, Vector3 initPos, float speed)
    {
        dir.y = 0;
        this.dir = dir;
        this.initPos = initPos;
        this.speed = speed;

        angle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(90, -angle, 0);
    }

    public virtual void Shoot(Vector3 dir, Vector3 initPos)
    {
        dir.y = 0;
        this.dir = dir;
        this.initPos = initPos;

        angle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(90, -angle, 0);
    }
}
