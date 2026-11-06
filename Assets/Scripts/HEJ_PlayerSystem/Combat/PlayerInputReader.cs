using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerInputReader : MonoBehaviour
{
    public event Action<Vector2> AttackPressed;

    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform player;

    private Plane attackPlane;



    private void Awake()
    {
        mainCamera = Camera.main;

        if (player == null)
            player = transform;
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        // UI 위를 클릭했다면 공격 입력 무시
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        if (mainCamera == null || player == null) return;

        // 플레이어의 현재 높이에 맞춰 XZ 평면 갱신
        attackPlane.SetNormalAndPosition(Vector3.up, player.position);

        // 마우스 위치를 통과하는 광선 계산
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (!attackPlane.Raycast(ray, out float distance)) return;

        // 플레이어에서 마우스가 가리키는 위치로 향하는 벡터
        Vector3 direction = ray.GetPoint(distance) - player.position;

        // 월드 X와 Z 방향을 Vector2에 저장
        Vector2 attackDirection = new Vector2(direction.x, direction.z);

        // 방향을 결정할 수 없는 클릭은 무시
        if (attackDirection.sqrMagnitude < 0.0001f) return;

        attackDirection.Normalize();

        AttackPressed?.Invoke(attackDirection);
    }
}