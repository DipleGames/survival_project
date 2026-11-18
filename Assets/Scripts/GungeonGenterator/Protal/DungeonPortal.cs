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
    
    private bool IsDoorOpen;
    private Collider doorCollider;
    private SpriteRenderer OpendDoorSprite;
    private SpriteRenderer CloseDoorSprite;

    [Header("Portal Settings")]
    [SerializeField] private PortalType portalType;

    [Header("Scene Settings")]
    [SerializeField] private string dungeonSceneName = "DungeonSample";
    [SerializeField] private string townSceneName = "Game";

    public void OnEnable()
    {
        doorCollider = GetComponent<Collider>();
        //자식 오브젝트의 SpriteRenderer 컴포넌트 가져오기
        OpendDoorSprite = transform.Find("OpenDoor").GetComponent<SpriteRenderer>();
        CloseDoorSprite = transform.Find("CloseDoor").GetComponent<SpriteRenderer>();

        IsDoorOpen = portalType == PortalType.EnterDungeon;
        SetDoorState(IsDoorOpen);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.tag.Equals("Player") && !other.tag.Equals("Character")) return;

        switch (portalType)
        {
            //마을에서 던전으로 이동 시의 포탈
            case PortalType.EnterDungeon: EnterDungeon(); break;
            //던전 보스룸을 제외한 모든 방 클리어 시 포탈 활성화
            case PortalType.RoomComplete: CompleteRoom(); break;
            //던전에서 마을로 이동 시의 포탈(보스룸 클리어 후)
            case PortalType.ExitDungeon: ExitDungeon(); break;
        }
    }

    private void CompleteRoom(){ DungeonManager.Instance.DisplayMapView(); }

    private void EnterDungeon()
    {
        Debug.LogWarning("던전 입장 실행!");
        GameManager.Instance.ToNextScene(dungeonSceneName);

        // TODO: 마을 복귀 시 플레이어 스폰 위치는 추후 협의
    }

    private void ExitDungeon()
    {
        Debug.LogWarning("던전 탈출 실행!");

        // 씬 이동 전에 플레이어 이동 제한 해제
        Player player = GameManager.Instance.Player;
        if (player != null && player.TryGetComponent(out PlayerMove playerMove)) playerMove.MovementLocked = false;

        GameManager.Instance.ToNextScene(townSceneName);
    }

    public void SetDoorState(bool isOpen)
    {
        IsDoorOpen = isOpen;
        OpendDoorSprite.enabled = isOpen;
        CloseDoorSprite.enabled = !isOpen;
        doorCollider.enabled = isOpen;
    }

    public void UpdatePortalInDungeonData(RoomType roomType)
    {
        //포탈 타입 설정
        portalType = roomType == RoomType.Boss ? PortalType.ExitDungeon : PortalType.RoomComplete;
        //포탈 위치 설정
        transform.position = DungeonManager.Instance.GetDungeonSetting(roomType).PortalSpawnPosition;
    }
}