
using System.Collections;
using UnityEngine;

public class PlayerHealthBarController : MonoBehaviour
{
    [SerializeField] private PlayerHealthBarView view;

    private PlayerHealth health;
    private Coroutine connectCoroutine;

    private void OnEnable()
    {
        connectCoroutine = StartCoroutine(ConnectPlayer());
    }

    /// <summary>
    /// 생성된 Player의 PlayerHealth를 참조
    /// </summary>
    private IEnumerator ConnectPlayer()
    {
        Player player = FindObjectOfType<Player>();

        while (player == null || player.Health == null)
        {
            yield return new WaitForSecondsRealtime(0.2f);
            player = FindObjectOfType<Player>();
        }

        health = player.Health;

        // 체력 변경 이벤트 구독
        health.OnHealthChanged += RefreshUI;

        // 최초 체력 상태를 UI에 반영
        RefreshUI(health.CurrentHp, health.MaxHp);
    }

    /// <summary>
    /// PlayerHealth의 현재 체력을 View에 전달함.
    /// </summary>
    private void RefreshUI(float currentHp, float maxHp)
    {
        view.Render(currentHp, maxHp);
    }

    private void OnDisable()
    {
        if (connectCoroutine != null)
        {
            StopCoroutine(connectCoroutine);
            connectCoroutine = null;
        }

        if (health != null)
        {
            health.OnHealthChanged -= RefreshUI;
            health = null;
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Test/HP 30 감소")]
    private void TestDamage()
    {
        if (health != null)
            health.TakeDamage(30f);
    }

    [ContextMenu("Test/HP 20 회복")]
    private void TestHeal()
    {
        if (health != null)
            health.Heal(20f);
    }
#endif
}
