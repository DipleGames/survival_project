using UnityEngine;

public abstract class ConsumableEffect : MonoBehaviour
{
    // 현재 상황에서 효과를 적용할 수 있는지 확인
    public abstract bool CanApply(Player player);

    // 실제 효과 적용
    public abstract void Apply(Player player);
}