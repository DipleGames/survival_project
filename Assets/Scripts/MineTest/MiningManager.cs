using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
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
        [SerializeField] private GameObject jewelPrefab;
        [SerializeField] private GameObject rockPrefab;
        [SerializeField] private GameObject clickUiPrefab;
        [SerializeField] private Text cursorLabel;

        [Header("Interaction")]
        [SerializeField, Min(0.1f)] private float miningRadius = 3f;
        [SerializeField, Min(0.05f)] private float miningDuration = 2f;
        [SerializeField, Min(1)] private int damagePerHit = 10;
        [SerializeField] private bool useAnimationEvent;
        [SerializeField] private LayerMask miningRaycastMask = ~0;

        [Header("Auto movement")]
        [SerializeField, Min(0.1f)] private float autoMoveSpeed = 3f;
        [Tooltip("광물이 캐릭터 하체에 오도록 광물보다 화면 위쪽에 서는 거리")]
        [SerializeField, Min(0.05f)] private float approachDistance = 0.8f;
        [SerializeField, Min(0.01f)] private float arrivalDistance = 0.05f;

        [Header("Pickaxe")]
        [SerializeField, Min(1)] private int initialPickaxeDurability = 500;

        [Header("Spawning")]
        [SerializeField, Min(1)] private int maxNodeCount = 10;
        [SerializeField, Min(0f)] private float respawnDelay = 10f;
        [SerializeField, Range(0f, 1f)] private float jewelProbability = 0.5f;
        [SerializeField, Min(0)] private int edgeMarginCells = 1;
        [SerializeField] private Vector3 spawnOverlapHalfExtents = new Vector3(0.42f, 0.5f, 0.42f);
        [SerializeField, Min(0.1f)] private float minimumNodeSpacing = 1.5f;
        [SerializeField] private LayerMask spawnBlockingMask = ~0;
        [SerializeField, Min(1)] private int maxSpawnAttempts = 100;

        [Header("Cursor UI")]
        [SerializeField] private Vector2 labelCursorOffset = new Vector2(18f, -18f);
        [SerializeField] private Vector2 clickUiCursorOffset = new Vector2(0f, -36f);
        [SerializeField] private Vector3 clickUiScale = new Vector3(0.032f, 0.032f, 0.032f);

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

        public int PickaxeDurability => pickaxeDurability;
        public bool PickaxeBroken => pickaxeDurability <= 0;

        private void Awake()
        {
            if (worldCamera == null) worldCamera = Camera.main;
            if (player == null) player = FindObjectOfType<MineTestPlayerController>();
            if (ground == null)
                ground = FindObjectsOfType<Tilemap>()
                    .FirstOrDefault(map => map.name.Equals("ground", System.StringComparison.OrdinalIgnoreCase));

            pickaxeDurability = initialPickaxeDurability;
            CreateClickUi();
            if (cursorLabel != null)
            {
                cursorCanvas = cursorLabel.GetComponentInParent<Canvas>();
                cursorLabel.text = "채광하기";
            }
            HideHoverVisuals();
        }

        private void Start()
        {
            for (int i = 0; i < maxNodeCount; i++) TrySpawnNode();
        }

        private void Update()
        {
            if (autoMoving) UpdateAutoMove();

            if (miningRoutine == null && !autoMoving) UpdateHoveredNode();
            else ClearHoveredNode();

            UpdateCursorUi();
            if (Input.GetMouseButtonDown(0) && !IsPointerOverUi()) TryBeginMining();
        }

        private void LateUpdate()
        {
            UpdateNodeOcclusion();
        }

        private void OnDisable()
        {
            if (miningRoutine != null) StopCoroutine(miningRoutine);
            miningRoutine = null;
            autoMoving = false;
            activeNode = null;
            if (player != null) player.MovementLocked = false;
            HideHoverVisuals();
            RestoreNodeOpacity();
        }

        private void OnDestroy()
        {
            if (clickUiInstance != null) Destroy(clickUiInstance);
        }

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
                    if (next != null) break;
                }
            }

            if (next != null && !IsInRange(next)) next = null;
            if (hoveredNode == next) return;
            ClearHoveredNode();
            hoveredNode = next;
            if (hoveredNode != null) hoveredNode.SetHighlighted(true);
        }

        private bool IsInRange(MiningNode node)
        {
            if (node == null || player == null) return false;
            Vector3 delta = node.transform.position - player.transform.position;
            delta.y = 0f;
            return delta.sqrMagnitude <= miningRadius * miningRadius;
        }

        private void TryBeginMining()
        {
            if (miningRoutine != null || autoMoving || hoveredNode == null || !IsInRange(hoveredNode)) return;
            if (PickaxeBroken)
            {
                Debug.LogWarning("[MineTest] 곡괭이가 파괴되어 채광을 진행할 수 없습니다.", this);
                return;
            }

            activeNode = hoveredNode;
            activeNode.SetHighlighted(false);
            hoveredNode = null;
            hitApplied = false;
            if (player == null) return;

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

        private void BeginMiningSequence()
        {
            miningRoutine = StartCoroutine(MiningSequence());
        }

        private void CancelActiveInteraction()
        {
            autoMoving = false;
            activeNode = null;
            if (player != null)
            {
                player.StopMovementVisual();
                player.MovementLocked = false;
            }
        }

        private IEnumerator MiningSequence()
        {
            if (useAnimationEvent)
            {
                while (!hitApplied) yield return null;
            }
            else
            {
                yield return new WaitForSeconds(miningDuration);
                ApplyMiningHit();
            }

            activeNode = null;
            miningRoutine = null;
            if (player != null) player.MovementLocked = false;
        }

        // Call this from the final frame of a future pickaxe swing animation.
        public void ApplyMiningHit()
        {
            if (hitApplied || activeNode == null) return;
            hitApplied = true;
            pickaxeDurability -= damagePerHit;
            activeNode.TakeDamage(damagePerHit);
            Debug.Log($"[MineTest] 곡괭이 남은 내구도: {Mathf.Max(0, pickaxeDurability)}/{initialPickaxeDurability}", this);
            if (pickaxeDurability <= 0)
                Debug.LogWarning("[MineTest] 곡괭이가 파괴되어 채광을 진행할 수 없습니다.", this);
        }

        public void NotifyNodeDestroyed(MiningNode node)
        {
            nodes.Remove(node);
            pendingRespawns++;
            StartCoroutine(RespawnAfterDelay());
        }

        private IEnumerator RespawnAfterDelay()
        {
            yield return new WaitForSeconds(respawnDelay);
            pendingRespawns = Mathf.Max(0, pendingRespawns - 1);
            nodes.RemoveAll(node => node == null);
            if (nodes.Count < maxNodeCount) TrySpawnNode();
        }

        private bool TrySpawnNode()
        {
            if (ground == null || nodes.Count >= maxNodeCount) return false;
            var candidates = new List<Vector3Int>();
            BoundsInt bounds = ground.cellBounds;
            foreach (Vector3Int cell in bounds.allPositionsWithin)
            {
                if (!ground.HasTile(cell)) continue;
                if (cell.x < bounds.xMin + edgeMarginCells || cell.x >= bounds.xMax - edgeMarginCells ||
                    cell.y < bounds.yMin + edgeMarginCells || cell.y >= bounds.yMax - edgeMarginCells)
                    continue;
                candidates.Add(cell);
            }

            for (int i = 0; i < candidates.Count; i++)
            {
                int swap = Random.Range(i, candidates.Count);
                (candidates[i], candidates[swap]) = (candidates[swap], candidates[i]);
            }

            int attempts = Mathf.Min(maxSpawnAttempts, candidates.Count);
            for (int i = 0; i < attempts; i++)
            {
                Vector3 position = ground.GetCellCenterWorld(candidates[i]);
                position.y = 0.1f;
                if (IsTooCloseToExistingNode(position)) continue;

                // Nodes are spawned repeatedly in one frame. Synchronize newly created
                // colliders before checking the next candidate so Physics.CheckBox sees them.
                Physics.SyncTransforms();
                if (Physics.CheckBox(position, spawnOverlapHalfExtents, Quaternion.identity,
                        spawnBlockingMask, QueryTriggerInteraction.Collide))
                    continue;

                GameObject prefab = Random.value < jewelProbability ? jewelPrefab : rockPrefab;
                if (prefab == null) continue;
                GameObject instance = Instantiate(prefab, position, Quaternion.Euler(90f, 0f, 0f));
                MiningNode node = instance.GetComponent<MiningNode>();
                if (node == null) { Destroy(instance); continue; }
                node.Initialize(this);
                nodes.Add(node);
                Physics.SyncTransforms();
                return true;
            }

            Debug.LogWarning("[MineTest] 겹치지 않는 채광 오브젝트 생성 위치를 찾지 못했습니다.", this);
            return false;
        }

        private bool IsTooCloseToExistingNode(Vector3 candidate)
        {
            float minimumSqrDistance = minimumNodeSpacing * minimumNodeSpacing;
            foreach (MiningNode node in nodes)
            {
                if (node == null) continue;
                Vector3 delta = node.transform.position - candidate;
                delta.y = 0f;
                if (delta.sqrMagnitude < minimumSqrDistance) return true;
            }
            return false;
        }

        private void CreateClickUi()
        {
            if (clickUiPrefab == null) return;
            clickUiInstance = Instantiate(clickUiPrefab);
            clickUiInstance.name = clickUiPrefab.name + " (MineTest)";
            clickUiInstance.transform.localScale = clickUiScale;
            clickUiInstance.SetActive(false);
        }

        private void UpdateCursorUi()
        {
            bool visible = hoveredNode != null && miningRoutine == null;
            if (cursorLabel != null)
            {
                cursorLabel.gameObject.SetActive(visible);
                if (visible) PositionCursorLabel();
            }
            if (clickUiInstance != null)
            {
                clickUiInstance.SetActive(visible);
                if (visible) PositionClickUi();
            }
        }

        private void PositionCursorLabel()
        {
            RectTransform canvasRect = cursorCanvas != null ? cursorCanvas.transform as RectTransform : null;
            Camera uiCamera = cursorCanvas != null && cursorCanvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? cursorCanvas.worldCamera : null;
            Vector2 screenPosition = (Vector2)Input.mousePosition + labelCursorOffset;
            if (canvasRect != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect, screenPosition, uiCamera, out Vector2 localPosition))
                cursorLabel.rectTransform.anchoredPosition = localPosition;
            else
                cursorLabel.rectTransform.position = screenPosition;
        }

        private void PositionClickUi()
        {
            Ray ray = worldCamera.ScreenPointToRay(Input.mousePosition + (Vector3)clickUiCursorOffset);
            Plane plane = new Plane(Vector3.up, hoveredNode.transform.position);
            if (!plane.Raycast(ray, out float enter)) { clickUiInstance.SetActive(false); return; }
            clickUiInstance.transform.position = ray.GetPoint(enter) - worldCamera.transform.forward * 0.01f;
            clickUiInstance.transform.rotation = Quaternion.LookRotation(worldCamera.transform.forward, worldCamera.transform.up);
        }

        private void ClearHoveredNode()
        {
            if (hoveredNode != null) hoveredNode.SetHighlighted(false);
            hoveredNode = null;
        }

        private void HideHoverVisuals()
        {
            ClearHoveredNode();
            if (cursorLabel != null) cursorLabel.gameObject.SetActive(false);
            if (clickUiInstance != null) clickUiInstance.SetActive(false);
        }

        private void UpdateNodeOcclusion()
        {
            if (worldCamera == null || player == null) return;
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

        private bool TryGetPlayerScreenRect(out Rect result)
        {
            result = default;
            bool found = false;
            foreach (SpriteRenderer renderer in player.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy) continue;
                Rect rect = GetScreenRect(renderer.bounds);
                result = found ? Union(result, rect) : rect;
                found = true;
            }
            return found;
        }

        private Rect GetScreenRect(Bounds bounds)
        {
            Vector3 min = worldCamera.WorldToScreenPoint(bounds.min);
            Vector3 max = worldCamera.WorldToScreenPoint(bounds.max);
            float xMin = Mathf.Min(min.x, max.x);
            float yMin = Mathf.Min(min.y, max.y);
            return Rect.MinMaxRect(xMin, yMin, Mathf.Max(min.x, max.x), Mathf.Max(min.y, max.y));
        }

        private static Rect Union(Rect a, Rect b) => Rect.MinMaxRect(
            Mathf.Min(a.xMin, b.xMin), Mathf.Min(a.yMin, b.yMin),
            Mathf.Max(a.xMax, b.xMax), Mathf.Max(a.yMax, b.yMax));

        private void RestoreNodeOpacity()
        {
            foreach (MiningNode node in nodes)
                if (node != null) node.SetOccluded(false);
        }

        private static bool IsPointerOverUi() =>
            EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        private void OnDrawGizmosSelected()
        {
            if (player == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(player.transform.position, miningRadius);
        }
    }
}
