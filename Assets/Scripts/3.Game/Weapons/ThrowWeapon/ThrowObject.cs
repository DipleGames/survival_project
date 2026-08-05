using UnityEngine;

public class ThrowObject : MonoBehaviour
{
    [SerializeField] float existTime;
    [SerializeField] Animator animator;

    float currentTime;

    private void Start()
    {
        currentTime = existTime;
    }

    private void Update()
    {
        if (GetComponent<ThrowObjecMovement>().CheckIsArrive())
        {
            if (currentTime <= 0)
                GetComponent<ProjectileObjectPool>().DestroyProjectile();

            currentTime -= Time.deltaTime;
        }
    }

    private void OnEnable()
    {
        currentTime = existTime;
        animator.SetBool("isGround", false);
    }

    void OnDisable() 
    {
        currentTime = 0;
    }

    public void FlipRotate(bool isFilp)
    {
        animator.SetBool("isFlip", isFilp);
    }

    public void Grounded(bool isGround)
    {
        animator.SetBool("isGround", isGround);
    }
}
