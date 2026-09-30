using System;
using UnityEngine;

public class PlayerInputReader : MonoBehaviour
{
    public event Action<Vector2> AttackPressed;

    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform player;

    // 공격 방향 계산에 사용할 평면을 필드로 보관
    private Plane attackPlane;

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        // 플레이어의 현재 높이에 맞춰 XZ 평면 갱신
        attackPlane.SetNormalAndPosition(Vector3.up, player.position);

        // 마우스 위치를 통과하는 광선 계산
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (!attackPlane.Raycast(ray, out float distance)) return;

        // 플레이어에서 마우스가 가리키는 위치로 향하는 벡터
        Vector3 direction = ray.GetPoint(distance) - player.position;

        // 월드 X와 Z 방향을 Vector2에 저장
        Vector2 attackDirection = default;
        attackDirection.x = direction.x;
        attackDirection.y = direction.z;

        // 방향을 결정할 수 없는 클릭은 무시
        if (attackDirection.sqrMagnitude < 0.0001f) return;

        attackDirection.Normalize();
        Debug.Log($"{attackDirection}");
        AttackPressed?.Invoke(attackDirection);
    }
}