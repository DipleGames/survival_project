using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CraftingTablePlace : MonoBehaviour
{
    [SerializeField] private HouseManager houseManager; // 집 매니저 참초
    [SerializeField] private HouseRange houseRange;     // 집 범위 참조
    [SerializeField] private GameObject tablePrefab;    // 설치할 제작대 프리팹
    [SerializeField] private Tilemap groundTilemap;     // 타일맵 참조
    [SerializeField] private int requiredLevel = 1;     // 제작 필요 레벨
    private GameObject placedDesk;
    private Camera mainCam;
    private Coroutine removeRoutine;
    private bool isRemoving;

    private void Start()
    {
        mainCam = Camera.main;
        if (!houseManager)
            houseManager = FindObjectOfType<HouseManager>();
        if (!houseRange)
            houseRange = FindObjectOfType<HouseRange>();
        if (!groundTilemap)
            groundTilemap = FindObjectOfType<Tilemap>();
    }

    private void Update()
    {
        if (!placedDesk)
        {
            if (Input.GetMouseButtonDown(0))
            {
                TryPlaceDesk();
            }
        }
        if (placedDesk)
        {
            if (Input.GetMouseButtonDown(1))
            {
                TryStartRemoveDesk();
            }
        }
    }

    private void TryPlaceDesk()
    {
        if (houseManager.Level < requiredLevel)
        {
            Debug.Log($"집 레벨이 {requiredLevel} 미만입니다. 설치 불가.");
            return;
        }

        if (placedDesk != null)
        {
            Debug.Log("이미 설치된 제작대가 있습니다.");
            return;
        }

        if (!RayToGround(out var worldPos)) return;

        if (!houseRange.CanInteractAtWorld(worldPos))
        {
            Debug.Log("활동 범위 밖에는 설치할 수 없습니다.");
            return;
        }

        var cell = groundTilemap.WorldToCell(new Vector3(worldPos.x, 0f, worldPos.z));
        var center = groundTilemap.GetCellCenterWorld(cell);
        // 설치 높이를 집 높이에 맞춤
        center.y = houseManager.transform.position.y;
        placedDesk = Instantiate(tablePrefab, center, Quaternion.Euler(90f, 0f, 0f));
        Debug.Log($"제작대 설치 완료! {center}");
    }

    private void TryStartRemoveDesk()
    {
        if (placedDesk == null) return;
        
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject == placedDesk)
            {
                Debug.Log("제작대 제거 준비...");
                removeRoutine = StartCoroutine(RemoveAfterHold());
            }
        }
    }

    private IEnumerator RemoveAfterHold()
    {
        isRemoving = true;
        float holdTime = 0f;
        const float requiredTime = 3f;

        while (holdTime < requiredTime)
        {
            if (!Input.GetMouseButton(1))
            {
                CancelRemoveDesk();
                yield break;
            }

            holdTime += Time.deltaTime;
            yield return null;
        }

        if (isRemoving && placedDesk)
        {
            Destroy(placedDesk);
            placedDesk = null;
            Debug.Log("제작대 제거 완료");
        }

        isRemoving = false;
    }

    private void CancelRemoveDesk()
    {
        if (isRemoving && removeRoutine != null)
        {
            StopCoroutine(removeRoutine);
            isRemoving = false;
            Debug.Log("제거 취소됨");
        }
    }
    
    private bool RayToGround(out Vector3 hitPoint)
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, 200f))
        {
            hitPoint = hit.point;
            return true;
        }
        hitPoint = default;
        return false;
    }
}
