using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonPortal : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"포탈 충돌 감지: {other.name}");

        if (!other.CompareTag("Player")) return;

        Debug.Log("플레이어 감지 성공!");
        DungeonManager.Instance.RoomComplete();
    }
}
