using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace MineTest
{
    [DisallowMultipleComponent]
    public sealed class MiningManager : MonoBehaviour
    {
        [Header("Scene references")]
        [SerializeField] private Camera worldCamera;
        [SerializeField] private MineTestPlayerController player;
        [SerializeField] private Tilemap ground;

        [Tooltip("광물 생성을 허용할 Ground 태그 Tilemap. 비워두면 씬의 모든 Ground Tilemap을 자동으로 참조")]
        [SerializeField] private Tilemap[] spawnGrounds;

        [SerializeField] private GameObject jewelPrefab;
        [SerializeField] private GameObject rockPrefab;
        [SerializeField] private GameObject clickUiPrefab;
        [SerializeField] private Text cursorLabel;
        [SerializeField] private bool cloneCursorLabel;

        [Header("Interaction")]
        [SerializeField, Min(0.1f)] private float miningRadius = 3f;
        [SerializeField, Min(1)] private int miningAnimationLoops = 2;
        [SerializeField, Min(1)] private int damagePerHit = 10;
        [SerializeField] private LayerMask miningRaycastMask = ~0;

        [Header("Auto movement")]
        [SerializeField, Min(0.1f)] private float autoMoveSpeed = 3f;

        [Tooltip("광물이 캐릭터 하체에 오도록 광물보다 화면 위쪽에 서는 거리")]
        [SerializeField, Min(0.05f)] private float approachDistance = 0.8f;
        [SerializeField, Min(0.01f)] private float arrivalDistance = 0.05f;

        [Header("Pickaxe")]
        [SerializeField, Min(1)] private int initialPickaxeDurability = 500;

        [Header("Spawning")]
        [SerializeField, Min(1)] private int maxNodeCount = 20;
        [SerializeField, Min(0f)] private float respawnDelay = 10f;
        [SerializeField, Range(0f, 1f)] private float jewelProbability = 0.5f;
        [SerializeField, Min(0)] private int edgeMarginCells = 1;
        [SerializeField, Range(0f, 0.49f)] private float randomOffsetWithinCell = 0.35f;
        [SerializeField] private Vector3 spawnOverlapHalfExtents = new Vector3(0.42f, 0.5f, 0.42f);
        [SerializeField, Min(0.1f)] private float minimumNodeSpacing = 1.5f;
        [SerializeField] private LayerMask spawnBlockingMask = ~0;
        [SerializeField, Min(1)] private int maxSpawnAttempts = 100;

        [Header("Cursor UI")]
        [SerializeField] private Vector2 labelCursorOffset = new Vector2(-120f, 30f);
        [SerializeField] private Vector2 clickUiCursorOffset = new Vector2(-20f, -80f);
        [SerializeField] private Vector3 clickUiScale = new Vector3(0.04f, 0.04f, 0.04f);
        [SerializeField, Min(1f)] private float referenceCursorPixelSize = 75f;
        [SerializeField, Min(0.1f)] private float uiScaleMultiplier = 1f;

        private readonly List<MiningNode> nodes = new List<MiningNode>();
        private MiningNode hoveredNode;
        private MiningNode activeNode;
        private GameObject clickUiInstance;
        private Canvas cursorCanvas;
        private Coroutine miningRoutine;
        private bool autoMoving;
        private Vector3 autoMoveTarget;
        private int pickaxeDurability;
        private int pendingRespawns;
        private bool hitApplied;
        private bool gameplayWasUnavailable;

        public int PickaxeDurability => pickaxeDurability;
        public bool PickaxeBroken => pickaxeDurability <= 0;

        // 씬 참조, 생성 지형, 곡괭이 내구도와 커서 UI를 초기화한다.
        private void Awake()
        {
            if (worldCamera == null) 
            {
                worldCamera = Camera.main;
            }

            ResolveReferences();
            RefreshSpawnGrounds();

            pickaxeDurability = initialPickaxeDurability;

            if (cloneCursorLabel && cursorLabel != null)
            {
                cursorLabel = Instantiate(cursorLabel, cursorLabel.transform.parent);
                cursorLabel.name = "Mining Action Label";
            }

            CreateClickUi();

            if (cursorLabel != null)
            {
                cursorCanvas = cursorLabel.GetComponentInParent<Canvas>();
                cursorLabel.text = "채광하기";
            }

            HideHoverVisuals();
        }

        // 씬 시작 시 설정된 최대 개수만큼 광석과 보석 생성을 시도한다.
        private void Start()
        {
            for (int i = 0; i < maxNodeCount; i++) 
            {
                TrySpawnNode();
            }

            if (nodes.Count < maxNodeCount)
                {
                    Debug.LogWarning($"[MineTest] 채광 오브젝트를 {nodes.Count}/{maxNodeCount}개 생성했습니다. " +
                    "나무, 장애물 또는 다른 광석과 겹치지 않는 위치가 부족합니다.", this);
                }
        }

        // 게임 가능 상태를 확인하고 호버, 클릭, 자동 접근과 채광 시작을 처리한다.
        private void Update()
        {
            ResolveReferences();

            bool unavailable = GameplayUnavailable();

            if (unavailable)
            {
                if (!gameplayWasUnavailable || autoMoving || miningRoutine != null)
                {
                    CancelActiveInteraction();
                }

                HideHoverVisuals();
                gameplayWasUnavailable = true;
                return;
            }

            gameplayWasUnavailable = false;

            if (autoMoving) 
            {
                UpdateAutoMove();
            }

            if (miningRoutine == null && !autoMoving) 
            {
                UpdateHoveredNode();
            }
            else 
            {
                ClearHoveredNode();
            }

            UpdateCursorUi();

            if (Input.GetMouseButtonDown(0) && !IsPointerOverUi()) 
            {
                TryBeginMining();
            }
        }

        // 캐릭터와 겹쳐 보이는 광물의 투명도를 프레임 마지막에 갱신한다.
        private void LateUpdate()
        {
            UpdateNodeOcclusion();
        }

        // 비활성화될 때 진행 중인 채광을 중단하고 캐릭터와 화면 연출을 복원한다.
        private void OnDisable()
        {
            if (miningRoutine != null) 
            {
                StopCoroutine(miningRoutine);
            }

            miningRoutine = null;
            autoMoving = false;
            activeNode = null;

            if (player != null)
            {
                player.RestoreDefaultAnimation();
                player.MovementLocked = false;
            }

            HideHoverVisuals();
            RestoreNodeOpacity();
        }

        // 매니저가 생성한 클릭 UI와 복제 커서 라벨을 제거한다.
        private void OnDestroy()
        {
            if (clickUiInstance != null) 
            {
                Destroy(clickUiInstance);
            }

            if (cloneCursorLabel && cursorLabel != null) 
            {
                Destroy(cursorLabel.gameObject);
            }
        }

        // 마우스 레이캐스트로 범위 안의 광물을 찾고 선택 외곽선을 갱신한다.
        private void UpdateHoveredNode()
        {
            MiningNode next = null;

            if (worldCamera != null)
            {
                Ray ray = worldCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit[] hits = Physics.RaycastAll(ray, 1000f, miningRaycastMask, QueryTriggerInteraction.Collide);
                System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

                foreach (RaycastHit hit in hits)
                {
                    next = hit.collider.GetComponentInParent<MiningNode>();

                    if (next != null) 
                    {
                        break;
                    }
                }
            }

            if (next != null && !IsInRange(next))
            {
                next = null;
            }

            if (hoveredNode == next) 
            {
                return;
            }

            ClearHoveredNode();
            hoveredNode = next;

            if (hoveredNode != null) 
            {
                hoveredNode.SetHighlighted(true);
            }
        }

        // 지정한 광물이 캐릭터의 채광 가능 반경 안에 있는지 확인한다.
        private bool IsInRange(MiningNode node)
        {
            if (node == null || player == null) 
            {
                return false;
            }

            Vector3 delta = node.transform.position - player.transform.position;
            delta.y = 0f;

            return delta.sqrMagnitude <= miningRadius * miningRadius;
        }

        // 선택 광물과 곡괭이 상태를 검증한 뒤 자동 접근 또는 즉시 채광을 시작한다.
        private void TryBeginMining()
        {
            if (miningRoutine != null || autoMoving || hoveredNode == null ||
                !IsInRange(hoveredNode))
            {
                return;
            }

            if (PickaxeBroken)
            {
                Debug.LogWarning("[MineTest] 곡괭이가 파괴되어 채광을 진행할 수 없습니다.", this);
                return;
            }

            activeNode = hoveredNode;
            activeNode.SetHighlighted(false);
            hoveredNode = null;
            hitApplied = false;

            if (player == null) 
            {
                return;
            }

            Vector3 toNode = activeNode.transform.position - player.transform.position;
            toNode.y = 0f;
            
            player.SetFacingFromWorldX(toNode.x);
            player.MovementLocked = true;

            // The camera looks straight down and its screen-up direction is world +Z.
            // Always stand above the mineral on screen so it aligns with the lower body,
            // instead of choosing a radial point that can overlap the waist or head.
            autoMoveTarget = activeNode.transform.position + Vector3.forward * approachDistance;
            autoMoveTarget.y = player.transform.position.y;

            if (Vector3.Distance(player.transform.position, autoMoveTarget) <= arrivalDistance)
            {
                BeginMiningSequence();
                return;
            }

            autoMoving = true;
        }

        // 캐릭터를 광물 앞의 목표 지점까지 이동시키고 도착하면 채광으로 전환한다.
        private void UpdateAutoMove()
        {
            if (player == null || activeNode == null)
            {
                CancelActiveInteraction();
                return;
            }

            Vector3 next = Vector3.MoveTowards(
                player.transform.position, autoMoveTarget, autoMoveSpeed * Time.deltaTime);
            player.Move(next - player.transform.position);

            if (Vector3.Distance(player.transform.position, autoMoveTarget) <= arrivalDistance)
            {
                autoMoving = false;
                player.StopMovementVisual();

                BeginMiningSequence();
            }
        }

        // 캐릭터가 광물을 바라보게 한 후 채광 코루틴을 시작한다.
        private void BeginMiningSequence()
        {
            Vector3 toNode = activeNode.transform.position - player.transform.position;
            player.SetFacingFromWorldX(toNode.x);
            miningRoutine = StartCoroutine(MiningSequence());
        }

        // 진행 중인 자동 이동과 채광을 취소하고 캐릭터 제어권과 애니메이션을 복원한다.
        private void CancelActiveInteraction()
        {
            if (miningRoutine != null) 
            {
                StopCoroutine(miningRoutine);
            }

            miningRoutine = null;
            autoMoving = false;
            activeNode = null;

            if (player != null)
            {
                player.StopMovementVisual();
                player.RestoreDefaultAnimation();
                player.MovementLocked = false;
            }
        }

        // 채광 애니메이션을 지정 횟수 재생한 뒤 피해를 적용하고 기본 상태로 복귀한다.
        private IEnumerator MiningSequence()
        {
            player.StartMiningAnimation();

            yield return null;

            while (activeNode != null && !player.HasCompletedMiningLoops(miningAnimationLoops))
            {
                yield return null;
            }

            ApplyMiningHit();
            player.RestoreDefaultAnimation();

            activeNode = null;
            miningRoutine = null;

            if (player != null) 
            {
                player.MovementLocked = false;
            }
        }

        // 한 번의 채광 피해와 곡괭이 내구도 감소를 적용하고 결과를 로그로 남긴다.
        public void ApplyMiningHit()
        {
            if (hitApplied || activeNode == null) 
            {
                return;
            }

            hitApplied = true;
            pickaxeDurability -= damagePerHit;
            activeNode.TakeDamage(damagePerHit);

            Debug.Log($"[MineTest] 곡괭이 남은 내구도: {Mathf.Max(0, pickaxeDurability)}/{initialPickaxeDurability}", this);

            if (pickaxeDurability <= 0)
            {
                Debug.LogWarning("[MineTest] 곡괭이가 파괴되어 채광을 진행할 수 없습니다.", this);
            }
        }

        // 파괴된 광물을 목록에서 제거하고 지연 재생성을 예약한다.
        public void NotifyNodeDestroyed(MiningNode node)
        {
            nodes.Remove(node);
            pendingRespawns++;
            StartCoroutine(RespawnAfterDelay());
        }

        // 설정된 재생성 시간을 기다린 뒤 부족한 광물 하나를 다시 생성한다.
        private IEnumerator RespawnAfterDelay()
        {
            yield return new WaitForSeconds(respawnDelay);

            pendingRespawns = Mathf.Max(0, pendingRespawns - 1);
            nodes.RemoveAll(node => node == null);

            if (nodes.Count < maxNodeCount) 
            {
                TrySpawnNode();
            }
        }

        // Ground 태그 Tilemap의 유효한 셀을 골라 겹치지 않는 광물 생성을 시도한다.
        private bool TrySpawnNode()
        {
            if (spawnGrounds == null || spawnGrounds.Length == 0 || nodes.Count >= maxNodeCount) 
            {
                return false;
            }

            var candidates = new List<KeyValuePair<Tilemap, Vector3Int>>();

            foreach (Tilemap map in spawnGrounds)
            {
                if (!IsSpawnGround(map)) 
                {
                    continue;
                }

                BoundsInt bounds = map.cellBounds;

                foreach (Vector3Int cell in bounds.allPositionsWithin)
                {
                    if (!map.HasTile(cell)) 
                    {
                        continue;
                    }

                    if (cell.x < bounds.xMin + edgeMarginCells || cell.x >= bounds.xMax - edgeMarginCells ||
                        cell.y < bounds.yMin + edgeMarginCells || cell.y >= bounds.yMax - edgeMarginCells)
                    {
                        continue;
                    }

                    candidates.Add(new KeyValuePair<Tilemap, Vector3Int>(map, cell));
                }
            }

            for (int i = 0; i < candidates.Count; i++)
            {
                int swap = Random.Range(i, candidates.Count);
                (candidates[i], candidates[swap]) = (candidates[swap], candidates[i]);
            }

            int attempts = Mathf.Min(maxSpawnAttempts, candidates.Count);

            for (int i = 0; i < attempts; i++)
            {
                Tilemap candidateMap = candidates[i].Key;
                Vector3Int targetCell = candidates[i].Value;

                if (!candidateMap.HasTile(targetCell))
                {
                    continue;
                }

                Vector3 positionOnGround = GetRandomPositionInCell(candidateMap, targetCell);

                // Validate while the point is still on the tilemap plane. MineTest rotates
                // its Grid by 90 degrees, so raising world Y before WorldToCell can change
                // the calculated cell Z and reject every otherwise valid ground tile.
                if (!candidateMap.HasTile(candidateMap.WorldToCell(positionOnGround)))
                {
                    continue;
                }

                Vector3 position = positionOnGround;
                position.y = candidateMap.transform.position.y + 0.1f;

                if (HasWaterAtWorldPosition(position)) 
                {
                    continue;
                }

                if (IsTooCloseToExistingNode(position)) 
                {
                    continue;
                }

                // 같은 프레임에 연속 생성된 광석 Collider도 다음 검사에 포함되게 동기화한다.
                Physics.SyncTransforms();

                if (IsSpawnBlocked(position)) 
                {
                    continue;
                }

                GameObject prefab = Random.value < jewelProbability ? jewelPrefab : rockPrefab;

                if (prefab == null) 
                {
                    continue;
                }

                GameObject instance = Instantiate(prefab, position, Quaternion.Euler(90f, 0f, 0f));
                MiningNode node = instance.GetComponent<MiningNode>();

                if (node == null) 
                {
                    Destroy(instance);
                    continue;
                }

                node.Initialize(this);
                nodes.Add(node);
                Physics.SyncTransforms();

                return true;
            }

            return false;
        }

        // 생성 범위와 겹친 Collider 중 나무, 장애물 또는 다른 광석이 있는지 확인한다.
        private bool IsSpawnBlocked(Vector3 position)
        {
            Collider[] overlaps = Physics.OverlapBox(position, spawnOverlapHalfExtents,
                Quaternion.identity, spawnBlockingMask, QueryTriggerInteraction.Collide);

            foreach (Collider overlap in overlaps)
            {
                if (overlap == null) 
                {
                    continue;
                }

                if (overlap.GetComponentInParent<MiningNode>() != null) 
                {
                    return true;
                }

                if (HasTagInParents(overlap.transform, "Tree")) 
                {
                    return true;
                }

                if (HasTagInParents(overlap.transform, "Obstacle")) 
                {
                    return true;
                }
            }
            return false;
        }

        // 대상 Transform부터 부모 방향으로 올라가며 지정한 태그가 있는지 확인한다.
        private static bool HasTagInParents(Transform target, string tagName)
        {
            while (target != null)
            {
                if (target.CompareTag(tagName)) 
                {
                    return true;
                }
                target = target.parent;
            }
            return false;
        }

        // 선택된 Ground 셀의 중앙에 고정하지 않고 셀 내부의 무작위 월드 위치를 반환한다.
        private Vector3 GetRandomPositionInCell(Tilemap map, Vector3Int cell)
        {
            Vector3 center = map.GetCellCenterWorld(cell);
            Vector3 cellSize = map.layoutGrid != null ? map.layoutGrid.cellSize : Vector3.one;

            Vector3 localOffset = new Vector3(
                Random.Range(-randomOffsetWithinCell, randomOffsetWithinCell) * cellSize.x,
                Random.Range(-randomOffsetWithinCell, randomOffsetWithinCell) * cellSize.y,
                0f);

            return center + map.transform.TransformVector(localOffset);
        }

        // 카메라와 Character의 채광용 플레이어 컨트롤러 참조를 자동으로 찾는다.
        private void ResolveReferences()
        {
            if (worldCamera == null) 
            {
                worldCamera = Camera.main;
            }

            if (player != null) 
            {
                return;
            }

            Character character = Character.Instance != null ? Character.Instance : FindObjectOfType<Character>();

            if (character == null) 
            {
                return;
            }

            player = character.GetComponent<MineTestPlayerController>();

            if (player == null) 
            {
                player = character.gameObject.AddComponent<MineTestPlayerController>();
            }
        }

        // 활성 Tilemap 중 Ground 태그를 가진 생성 가능 지형 목록을 구성한다.
        private void RefreshSpawnGrounds()
        {
            if (spawnGrounds != null && spawnGrounds.Length > 0) 
            {
                return;
            }

            Tilemap[] all = FindObjectsOfType<Tilemap>();
            spawnGrounds = all.Where(IsSpawnGround).ToArray();

            if (IsSpawnGround(ground) && !spawnGrounds.Contains(ground))
                {
                    spawnGrounds = spawnGrounds.Concat(new[] { ground }).ToArray();
                }
        }

        // Tilemap이 Ground 태그를 가진 비수역 생성 지형인지 판별한다.
        private static bool IsSpawnGround(Tilemap map) =>
            map != null && map.CompareTag("Ground") && !IsWater(map);

        // Tilemap의 태그가 Sea 또는 Lake인지 판별한다.
        private static bool IsWater(Tilemap map) =>
            map != null && (map.CompareTag("Sea") || map.CompareTag("Lake"));

        // 지정한 월드 위치에 Sea 또는 Lake Tilemap 셀이 존재하는지 검사한다.
        private static bool HasWaterAtWorldPosition(Vector3 worldPosition)
        {
            foreach (Tilemap map in FindObjectsOfType<Tilemap>())
            {
                if (IsWater(map) && map.HasTile(map.WorldToCell(worldPosition))) 
                {
                    return true;
                }
            }

            return false;
        }

        // 현재 씬, 일시정지, 밤 상태를 기준으로 채광이 금지된 상태인지 확인한다.
        private bool GameplayUnavailable()
        {
            // MineTest is a standalone interaction scene. A persistent GameManager can
            // still report a scene name other than "Game", which previously caused
            // Update to cancel hover, movement and mining every frame in this scene.
            if (IsMineTestScene())
            {
                return false;
            }

            GameManager game = GameManager.Instance;

            if (game != null && (game.currentScene != "Game" || game.isPause)) 
            {
                return true;
            }

            return GamesceneManager.Instance != null && GamesceneManager.Instance.isNight;
        }

        private static bool IsMineTestScene() =>
            SceneManager.GetActiveScene().name == "MineTest";

        // 생성 후보가 기존 광물과 최소 간격보다 가까운지 확인한다.
        private bool IsTooCloseToExistingNode(Vector3 candidate)
        {
            float minimumSqrDistance = minimumNodeSpacing * minimumNodeSpacing;

            foreach (MiningNode node in nodes)
            {
                if (node == null) 
                {
                    continue;
                }
                Vector3 delta = node.transform.position - candidate;
                delta.y = 0f;

                if (delta.sqrMagnitude < minimumSqrDistance) 
                {
                    return true;
                }
            }
            return false;
        }

        // 광물 호버 위치에 표시할 클릭 안내 UI 인스턴스를 생성한다.
        private void CreateClickUi()
        {
            if (clickUiPrefab == null) 
            {
                return;
            }

            clickUiInstance = Instantiate(clickUiPrefab);
            clickUiInstance.name = clickUiPrefab.name + " (MineTest)";

            UpdateUiScale();

            clickUiInstance.SetActive(false);
        }

        // FishingTestController와 같은 계산식으로 현재 커서 크기에 맞춰 클릭 UI 배율을 조절한다.
        private void UpdateUiScale()
        {
            if (clickUiInstance == null) 
            {
                return;
            }

            float cursorPixels = referenceCursorPixelSize;

            if (GameManager.Instance != null && GameManager.Instance.useCursorNormal != null)
            {
                cursorPixels = GameManager.Instance.useCursorNormal.width;
            }

            float scale = Mathf.Max(0.1f, cursorPixels / referenceCursorPixelSize) * uiScaleMultiplier;

            clickUiInstance.transform.localScale = clickUiScale * scale;
        }

        // 현재 호버와 채광 상태에 따라 커서 라벨과 클릭 UI를 표시하고 배치한다.
        private void UpdateCursorUi()
        {
            UpdateUiScale();

            bool visible = hoveredNode != null && miningRoutine == null;

            if (cursorLabel != null)
            {
                cursorLabel.gameObject.SetActive(visible);

                if (visible) 
                {
                    PositionCursorLabel();
                }
            }

            if (clickUiInstance != null)
            {
                clickUiInstance.SetActive(visible);

                if (visible) 
                {
                    PositionClickUi();
                }
            }
        }

        // 커서 라벨을 마우스 위치에 지정된 화면 오프셋으로 배치한다.
        private void PositionCursorLabel()
        {
            RectTransform canvasRect = cursorCanvas != null ? cursorCanvas.transform as RectTransform : null;

            Camera uiCamera = cursorCanvas != null && cursorCanvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? cursorCanvas.worldCamera : null;

            Vector2 screenPosition = (Vector2)Input.mousePosition + labelCursorOffset;

            if (canvasRect != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect, screenPosition, uiCamera, out Vector2 localPosition))
                    {
                        cursorLabel.rectTransform.anchoredPosition = localPosition;
                    }
            else
            {
                cursorLabel.rectTransform.position = screenPosition;
            }
        }

        // 클릭 안내 UI를 호버 광물 높이의 월드 좌표에 배치한다.
        private void PositionClickUi()
        {
            Ray ray = worldCamera.ScreenPointToRay(Input.mousePosition + (Vector3)clickUiCursorOffset);

            Plane plane = new Plane(Vector3.up, hoveredNode.transform.position);

            if (!plane.Raycast(ray, out float enter)) 
            { 
                clickUiInstance.SetActive(false); return; 
            }

            clickUiInstance.transform.position = ray.GetPoint(enter) - worldCamera.transform.forward * 0.01f;
            clickUiInstance.transform.rotation = Quaternion.LookRotation(worldCamera.transform.forward, worldCamera.transform.up);
        }

        // 현재 호버 광물의 외곽선을 끄고 선택 참조를 제거한다.
        private void ClearHoveredNode()
        {
            if (hoveredNode != null) 
            {
                hoveredNode.SetHighlighted(false);
            }

            hoveredNode = null;
        }

        // 광물 외곽선, 커서 라벨과 클릭 안내 UI를 모두 숨긴다.
        private void HideHoverVisuals()
        {
            ClearHoveredNode();

            if (cursorLabel != null) 
            {
                cursorLabel.gameObject.SetActive(false);
            }

            if (clickUiInstance != null) 
            {
                clickUiInstance.SetActive(false);
            }
        }

        // 화면상 캐릭터를 가리는 광물 노드를 찾아 반투명하게 표시한다.
        private void UpdateNodeOcclusion()
        {
            if (worldCamera == null || player == null) 
            {
                return;
            }

            if (!TryGetPlayerScreenRect(out Rect playerRect))
            {
                RestoreNodeOpacity();
                return;
            }

            nodes.RemoveAll(node => node == null);

            foreach (MiningNode node in nodes)
            {
                bool obscuresPlayer = node != activeNode &&
                    node.MainRenderer != null && node.MainRenderer.enabled &&
                    GetScreenRect(node.MainRenderer.bounds).Overlaps(playerRect);

                node.SetOccluded(obscuresPlayer);
            }
        }

        // 활성 캐릭터 SpriteRenderer들을 합쳐 화면상의 전체 사각형을 계산한다.
        private bool TryGetPlayerScreenRect(out Rect result)
        {
            result = default;
            bool found = false;

            foreach (SpriteRenderer renderer in player.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy) 
                {
                    continue;
                }

                Rect rect = GetScreenRect(renderer.bounds);
                result = found ? Union(result, rect) : rect;
                found = true;
            }
            return found;
        }

        // 월드 Bounds를 카메라 화면 좌표의 사각형으로 변환한다.
        private Rect GetScreenRect(Bounds bounds)
        {
            Vector3 min = worldCamera.WorldToScreenPoint(bounds.min);
            Vector3 max = worldCamera.WorldToScreenPoint(bounds.max);

            float xMin = Mathf.Min(min.x, max.x);
            float yMin = Mathf.Min(min.y, max.y);

            return Rect.MinMaxRect(xMin, yMin, Mathf.Max(min.x, max.x), Mathf.Max(min.y, max.y));
        }

        // 두 화면 사각형을 모두 포함하는 하나의 합집합 사각형을 반환한다.
        private static Rect Union(Rect a, Rect b) => Rect.MinMaxRect(
            Mathf.Min(a.xMin, b.xMin), Mathf.Min(a.yMin, b.yMin),
            Mathf.Max(a.xMax, b.xMax), Mathf.Max(a.yMax, b.yMax));

        // 모든 광물 노드의 가림 투명도를 원래 상태로 복구한다.
        private void RestoreNodeOpacity()
        {
            foreach (MiningNode node in nodes)
            {
                if (node != null) 
                {
                    node.SetOccluded(false);
                }
            }
        }

        // 현재 마우스 포인터가 EventSystem UI 위에 있는지 확인한다.
        private static bool IsPointerOverUi() =>
            EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        // 에디터에서 매니저 선택 시 캐릭터 중심의 채광 가능 반경을 표시한다.
        private void OnDrawGizmosSelected()
        {
            if (player == null) 
            {
                return;
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(player.transform.position, miningRadius);
        }
    }
}
