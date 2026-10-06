using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public float attackRange = 2.0f;       // 공격 사거리
    public LayerMask enemyLayer;           // 적 레이어 설정 (Inspector에서 지정)

    [Header("차지 공격 설정")]
    public float minChargeTime = 0.5f;     // 강공격으로 인정되는 최소 충전 시간
    private float chargeTimer = 0f;
    private bool isCharging = false;

    [Header("공격 데미지 설정")]
    public int normalDamage = 1;           // 일반 공격 데미지
    public int heavyDamage = 3;            // 강공격(차지) 데미지

    [Header("기즈모 스타일 설정")]
    public Color gizmoColor = new Color(1f, 0f, 0f, 0.3f); // 기본 불투명도 30%의 빨간색
    public bool showSolidSphere = true;                     // 색이 꽉 찬 입체 구체로 볼 것인지 여부

    void Update()
    {
        // 1. 마우스 좌클릭을 누르기 시작할 때
        if (Input.GetMouseButtonDown(0))
        {
            isCharging = true;
            chargeTimer = 0f;
        }

        // 2. 누르고 있는 동안 시간 누적
        if (isCharging && Input.GetMouseButton(0))
        {
            chargeTimer += Time.deltaTime;
        }

        // 3. 마우스 좌클릭을 뗐을 때 공격 발동
        if (Input.GetMouseButtonUp(0) && isCharging)
        {
            isCharging = false;

            // 충전 시간이 기준치 이상이면 강공격, 아니면 일반공격
            bool isHeavyAttack = chargeTimer >= minChargeTime;
            TryAttack(isHeavyAttack);
        }
    }

    void TryAttack(bool isHeavyAttack)
    {
        int finalDamage = isHeavyAttack ? heavyDamage : normalDamage;
        Debug.Log(isHeavyAttack ? $"[플레이어] 강공격! 데미지: {finalDamage}" : $"[플레이어] 일반공격! 데미지: {finalDamage}");

        // 3D 환경에 맞춰 Physics.OverlapSphere로 반경 내의 3D Collider들을 감지
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);

        foreach (Collider enemy in hitEnemies)
        {
            ShieldEnemy shieldEnemy = enemy.GetComponent<ShieldEnemy>();
            if (shieldEnemy != null)
            {
                shieldEnemy.TakeDamage(finalDamage, isHeavyAttack);
                break;
            }

            SmallBazicEnemy normalEnemy = enemy.GetComponent<SmallBazicEnemy>();
            if (normalEnemy != null)
            {
                normalEnemy.TakeDamage(finalDamage);
                break;
            }
        }
    }

    // 유니티 에디터 상에서 실시간 사거리를 확인할 수 있는 3D 기즈모 연출
    private void OnDrawGizmos()
    {
        // 현재 플레이어의 위치를 기준으로 기즈모 매트릭스 설정 (스케일 변화 대응)
        Gizmos.matrix = transform.localToWorldMatrix;

        // 기즈모 색상 할당
        Gizmos.color = gizmoColor;

        if (showSolidSphere)
        {
            // 3D 공간을 가시적으로 파악하기 쉽게 반투명한 '속이 꽉 찬 구체'를 그립니다.
            Gizmos.DrawSphere(Vector3.zero, attackRange);
        }

        // 테두리 선을 추가하여 경계를 명확하게 만듭니다.
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1.0f); // 테두리는 선명하게 오퍼시티 100%
        Gizmos.DrawWireSphere(Vector3.zero, attackRange);
    }
}
