using UnityEngine;

public interface IKnockbackable
{
    void ApplyKnockback(Vector3 direction, float power);
}