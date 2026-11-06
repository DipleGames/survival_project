using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonPortal : MonoBehaviour
{
    public enum PortalType
    {
        RoomComplete,
        EnterDungeon,
        ExitDungeon
    }

    [Header("Portal Settings")]
    [SerializeField] private PortalType portalType;
    public void SetPortalType(PortalType type){ portalType = type; }

    [Header("Scene Settings")]
    [SerializeField] private string dungeonSceneName = "DungeonSample";
    [SerializeField] private string worldSceneName = "Game";

    private void OnTriggerEnter(Collider other)
    {
        if (!other.tag.Equals("Player") && !other.tag.Equals("Character")) return;

        switch (portalType)
        {
            case PortalType.RoomComplete:
                CompleteRoom();
                break;

            case PortalType.EnterDungeon:
                EnterDungeon();
                break;

            case PortalType.ExitDungeon:
                ExitDungeon();
                break;
        }
    }

    private void CompleteRoom()
    {
        DungeonManager.Instance.RoomComplete();
    }

    private void EnterDungeon()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Character");
        GameObject point = GameObject.Find("DungeonReturnPoint");
        if (player == null || point == null) return;

        Vector3 PlayerPos = player.transform.position;
        Vector3 PointPos = point.transform.position;
        player.transform.position = new Vector3(PointPos.x, PlayerPos.y, PointPos.z);


        Debug.LogWarning("던전 입장 실행!");
        SceneManager.LoadScene(dungeonSceneName);
    }

    private void ExitDungeon()
    {
        Debug.LogWarning("던전 탈출 실행!");
        SceneManager.LoadScene(worldSceneName);
    }
}
