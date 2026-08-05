using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace FishingTest
{
    [DisallowMultipleComponent]
    public sealed class FishingTestController : MonoBehaviour
    {
        private enum FishingState { Idle, AutoMoving, Casting, WaitingAnimation, WaitingForBite, BiteAnimation, Bite, Hooking, ResolvingResult }

        [Header("Scene references")]
        [SerializeField] private Camera worldCamera;
        [SerializeField] private FishingTestPlayerController player;
        [Tooltip("When empty, every active Lake/Sea tagged Tilemap is found automatically.")]
        [SerializeField] private Tilemap[] waterTilemaps;
        [SerializeField] private Text cursorLabel;
        [SerializeField] private GameObject clickUiPrefab;
        [SerializeField] private AudioClip fishRodThrowClip;
        [SerializeField] private AudioClip fishingCatchStartClip;
        [SerializeField] private AudioClip fishingSuccessClip;
        [SerializeField] private AudioClip fishingFailClip;

        [Header("Interaction")]
        [SerializeField, Min(0)] private int detectionHalfExtent = 1;
        [SerializeField, Min(0.1f)] private float autoMoveSpeed = 3f;
        [SerializeField, Min(0.01f)] private float arrivalDistance = 0.05f;
        [Tooltip("Keeps the character's body, not only its pivot, outside water.")]
        [SerializeField, Min(0f)] private float waterAvoidanceRadius = 0.15f;
        [SerializeField, Range(0f, 1f)] private float waterOverlayAlpha = 0.5f;

        [Header("Cursor-following UI")]
        [SerializeField] private Vector2 labelCursorOffset = new Vector2(18f, -18f);
        [SerializeField] private Vector2 clickUiCursorOffset = new Vector2(0f, -36f);
        [Tooltip("Cursor texture width treated as scale 1.")]
        [SerializeField, Min(1f)] private float referenceCursorPixelSize = 64f;
        [SerializeField, Min(1)] private int baseLabelFontSize = 30;
        [SerializeField] private Vector3 baseClickUiScale = new Vector3(0.032f, 0.032f, 0.032f);
        [SerializeField, Min(0.1f)] private float uiScaleMultiplier = 1f;

        [Header("Fishing timing")]
        [SerializeField] private bool useAnimationEvents;
        [SerializeField, Min(0f)] private float castAnimationDuration = 0.5f;
        [SerializeField, Min(0f)] private float waitingAnimationDuration = 0.5f;
        [SerializeField, Min(0f)] private float minBiteDelay = 3f;
        [SerializeField, Min(0f)] private float maxBiteDelay = 7f;
        [SerializeField, Min(0f)] private float biteAnimationDuration = 0.3f;
        [SerializeField, Min(0.05f)] private float hookWindow = 1f;
        [SerializeField, Min(0f)] private float hookingAnimationDuration = 0.5f;
        [SerializeField, Min(0f)] private float resultSoundDelay = 1f;
        [SerializeField, Min(0.05f)] private float movementCancelHoldTime = 1f;

        private static readonly Vector3Int[] AdjacentCells = { Vector3Int.left, Vector3Int.right, Vector3Int.up, Vector3Int.down };

        private sealed class WaterBody
        {
            public Tilemap Tilemap;
            public readonly List<Vector3Int> Cells = new List<Vector3Int>();
        }

        private struct WaterCellKey
        {
            public Tilemap Tilemap;
            public Vector3Int Cell;

            public override bool Equals(object obj)
            {
                if (!(obj is WaterCellKey)) return false;
                WaterCellKey other = (WaterCellKey)obj;
                return Tilemap == other.Tilemap && Cell == other.Cell;
            }

            public override int GetHashCode()
            {
                return ((Tilemap != null ? Tilemap.GetHashCode() : 0) * 397) ^ Cell.GetHashCode();
            }
        }

        private FishingState state;
        private Tilemap hoveredWater, activeWater;
        private Vector3Int activeCell;
        private WaterBody hoveredBody, activeBody, overlayBody;
        private Vector3 autoMoveTarget;
        private float stateTimer, moveHoldTimer;
        private bool pendingSuccess, overlayVisible, missingPlayerReported;
        private readonly List<Color> originalWaterColors = new List<Color>();
        private readonly Dictionary<WaterCellKey, WaterBody> waterBodies = new Dictionary<WaterCellKey, WaterBody>();
        private Tilemap[] landTilemaps;
        private GameObject clickUiInstance;
        private AudioSource fallbackAudioSource;
        private Canvas cursorCanvas;

        private void Awake()
        {
            ResolveReferences();
            RefreshTilemaps();
            CreateClickUi();
            if (cursorLabel != null)
            {
                cursorCanvas = cursorLabel.GetComponentInParent<Canvas>();
                cursorLabel.transform.SetAsLastSibling();
            }
            fallbackAudioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
            fallbackAudioSource.playOnAwake = false;
            fallbackAudioSource.spatialBlend = 0f;
            HideVisuals();
        }

        private void OnEnable() { ResolveReferences(); RefreshTilemaps(); }
        private void OnDisable() { ResetFishing(); }
        private void OnDestroy() { if (clickUiInstance != null) Destroy(clickUiInstance); }

        private void Update()
        {
            ResolveReferences();
            UpdateUiScale();
            if (GameplayUnavailable())
            {
                if (state != FishingState.Idle) ResetFishing(); else HideVisuals();
                return;
            }
            if (player == null) { HideVisuals(); return; }

            UpdateHoveredTile();
            UpdateCursorUi();
            if (state == FishingState.AutoMoving) UpdateAutoMove();
            else if (IsSequenceState()) UpdateFishing();
            if (Input.GetMouseButtonDown(0) && !IsPointerOverUi()) HandleClick();
        }

        private void ResolveReferences()
        {
            if (worldCamera == null) worldCamera = Camera.main;
            if (player == null && Character.Instance != null)
                player = Character.Instance.GetComponent<FishingTestPlayerController>();
            if (player == null && !missingPlayerReported)
            {
                Debug.LogError("[Tile Fishing] Add FishingTestPlayerController to the Character GameObject.", this);
                missingPlayerReported = true;
            }
            else if (player != null) missingPlayerReported = false;
        }

        private bool GameplayUnavailable()
        {
            GameManager gm = GameManager.Instance;
            if (gm == null)
                return player != null && player.GetComponent<Character>() != null;
            if (gm.currentScene != "Game" || gm.isPause) return true;
            return GamesceneManager.Instance != null && GamesceneManager.Instance.isNight;
        }

        private static bool IsPointerOverUi() => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        private static bool IsWater(Tilemap map) => map != null && (map.CompareTag("Lake") || map.CompareTag("Sea"));

        private void RefreshTilemaps()
        {
            Tilemap[] all = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
            if (waterTilemaps == null || waterTilemaps.Length == 0) waterTilemaps = all.Where(IsWater).ToArray();
            landTilemaps = all.Where(map => map != null && !IsWater(map)).ToArray();
            RebuildWaterBodies();
        }

        private void RebuildWaterBodies()
        {
            waterBodies.Clear();
            if (waterTilemaps == null) return;

            foreach (Tilemap water in waterTilemaps)
            {
                if (water == null) continue;
                foreach (Vector3Int start in water.cellBounds.allPositionsWithin)
                {
                    var startKey = new WaterCellKey { Tilemap = water, Cell = start };
                    if (!water.HasTile(start) || waterBodies.ContainsKey(startKey)) continue;

                    var body = new WaterBody { Tilemap = water };
                    var pending = new Queue<Vector3Int>();
                    pending.Enqueue(start);
                    waterBodies.Add(startKey, body);

                    while (pending.Count > 0)
                    {
                        Vector3Int cell = pending.Dequeue();
                        body.Cells.Add(cell);
                        foreach (Vector3Int offset in AdjacentCells)
                        {
                            Vector3Int next = cell + offset;
                            var key = new WaterCellKey { Tilemap = water, Cell = next };
                            if (!water.HasTile(next) || waterBodies.ContainsKey(key)) continue;
                            waterBodies.Add(key, body);
                            pending.Enqueue(next);
                        }
                    }
                }
            }
        }

        private void CreateClickUi()
        {
            if (clickUiPrefab == null || clickUiInstance != null) return;
            clickUiInstance = Instantiate(clickUiPrefab);
            clickUiInstance.name = clickUiPrefab.name + " (Tile Fishing)";
            clickUiInstance.SetActive(false);
            UpdateUiScale();
        }

        private void UpdateUiScale()
        {
            float cursorPixels = referenceCursorPixelSize;
            if (GameManager.Instance != null && GameManager.Instance.useCursorNormal != null)
                cursorPixels = GameManager.Instance.useCursorNormal.width;
            float scale = Mathf.Max(0.1f, cursorPixels / referenceCursorPixelSize) * uiScaleMultiplier;
            if (cursorLabel != null) cursorLabel.fontSize = Mathf.Max(1, Mathf.RoundToInt(baseLabelFontSize * scale));
            if (clickUiInstance != null) clickUiInstance.transform.localScale = baseClickUiScale * scale;
        }

        private void UpdateHoveredTile()
        {
            hoveredWater = null;
            hoveredBody = null;
            ClearOverlay();
            if (worldCamera == null || waterTilemaps == null) return;
            Ray ray = worldCamera.ScreenPointToRay(Input.mousePosition);
            foreach (Tilemap water in waterTilemaps)
            {
                if (water == null || !water.isActiveAndEnabled) continue;
                Plane plane = new Plane(Vector3.up, water.transform.position);
                if (!plane.Raycast(ray, out float enter)) continue;
                Vector3Int cell = water.WorldToCell(ray.GetPoint(enter));
                if (!water.HasTile(cell)) continue;
                hoveredWater = water;
                waterBodies.TryGetValue(new WaterCellKey { Tilemap = water, Cell = cell }, out hoveredBody);
                if (InRange(hoveredBody) && water.CompareTag("Lake")) ShowOverlay(hoveredBody);
                return;
            }
        }

        private bool InRange(WaterBody body)
        {
            if (player == null || body == null || body.Tilemap == null) return false;
            Vector3Int playerCell = body.Tilemap.WorldToCell(player.transform.position);
            return body.Cells.Any(cell =>
                Mathf.Abs(playerCell.x - cell.x) <= detectionHalfExtent &&
                Mathf.Abs(playerCell.y - cell.y) <= detectionHalfExtent);
        }

        private void ShowOverlay(WaterBody body)
        {
            if (body == null) return;
            originalWaterColors.Clear();
            foreach (Vector3Int cell in body.Cells)
            {
                body.Tilemap.SetTileFlags(cell, TileFlags.None);
                Color original = body.Tilemap.GetColor(cell);
                originalWaterColors.Add(original);
                Color color = original;
                color.a = waterOverlayAlpha;
                body.Tilemap.SetColor(cell, color);
            }
            overlayBody = body;
            overlayVisible = true;
        }

        private void ClearOverlay()
        {
            if (!overlayVisible) return;
            if (overlayBody != null && overlayBody.Tilemap != null)
                for (int i = 0; i < overlayBody.Cells.Count && i < originalWaterColors.Count; i++)
                    overlayBody.Tilemap.SetColor(overlayBody.Cells[i], originalWaterColors[i]);
            overlayBody = null;
            originalWaterColors.Clear();
            overlayVisible = false;
        }

        private void UpdateCursorUi()
        {
            bool valid = hoveredBody != null && InRange(hoveredBody);
            SetLabelVisible(valid);
            if (!valid) { SetClickVisible(false); return; }
            if (IsAnimationState() || state == FishingState.ResolvingResult)
            { SetLabelVisible(false); SetClickVisible(false); return; }

            bool active = activeBody == hoveredBody && IsSequenceState();
            if (cursorLabel != null)
            {
                cursorLabel.text = active ? (state == FishingState.Bite ? "낚아채기" : "낚시 중단하기") : "찌 던지기";
                PositionCursorLabel();
            }
            SetClickVisible(!active || state == FishingState.Bite);
        }

        private void SetLabelVisible(bool visible) { if (cursorLabel != null) cursorLabel.gameObject.SetActive(visible); }

        private void PositionCursorLabel()
        {
            if (cursorLabel == null) return;
            if (cursorCanvas == null) cursorCanvas = cursorLabel.GetComponentInParent<Canvas>();

            RectTransform canvasRect = cursorCanvas != null ? cursorCanvas.transform as RectTransform : null;
            Camera uiCamera = cursorCanvas != null && cursorCanvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? cursorCanvas.worldCamera
                : null;
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

        private void SetClickVisible(bool visible)
        {
            if (clickUiInstance == null) return;
            clickUiInstance.SetActive(visible);
            if (!visible || worldCamera == null || hoveredWater == null) return;
            Ray ray = worldCamera.ScreenPointToRay(Input.mousePosition + (Vector3)clickUiCursorOffset);
            Plane plane = new Plane(Vector3.up, hoveredWater.transform.position);
            if (!plane.Raycast(ray, out float enter)) { clickUiInstance.SetActive(false); return; }
            clickUiInstance.transform.position = ray.GetPoint(enter) - worldCamera.transform.forward * 0.01f;
            clickUiInstance.transform.rotation = Quaternion.LookRotation(worldCamera.transform.forward, worldCamera.transform.up);
        }

        private void HandleClick()
        {
            if (hoveredBody == null || !InRange(hoveredBody) || IsAnimationState() || state == FishingState.ResolvingResult) return;
            if (IsSequenceState())
            {
                bool same = activeBody == hoveredBody;
                if (same && state == FishingState.Bite && stateTimer <= hookWindow) BeginHooking();
                else BeginResult(false);
                return;
            }
            if (!FindFishingPoint(hoveredBody, out Vector3 destination, out Vector3Int fishingCell))
            { Debug.Log("[Tile Fishing] 사용가능한 인접 육지 타일이 없습니다.", this); return; }
            activeBody = hoveredBody; activeWater = hoveredBody.Tilemap; activeCell = fishingCell; autoMoveTarget = destination;
            state = FishingState.AutoMoving; player.MovementLocked = true; player.AutoMovingVisual = true;
        }

        private bool FindFishingPoint(WaterBody body, out Vector3 destination, out Vector3Int fishingCell)
        {
            var candidates = new List<KeyValuePair<Vector3, Vector3Int>>();
            var uniqueGroundCells = new HashSet<WaterCellKey>();
            foreach (Vector3Int waterCell in body.Cells)
            foreach (Vector3Int offset in AdjacentCells)
            {
                Vector3 probe = body.Tilemap.GetCellCenterWorld(waterCell + offset);
                if (HasWaterAtWorldPosition(probe)) continue;
                foreach (Tilemap land in landTilemaps)
                {
                    Vector3Int groundCell = land.WorldToCell(probe);
                    var key = new WaterCellKey { Tilemap = land, Cell = groundCell };
                    if (!land.HasTile(groundCell) || !uniqueGroundCells.Add(key)) continue;
                    Vector3 point = land.GetCellCenterWorld(groundCell);
                    point.y = player.transform.position.y;
                    if (!PathStaysOutOfWater(player.transform.position, point)) continue;
                    candidates.Add(new KeyValuePair<Vector3, Vector3Int>(point, waterCell));
                }
            }
            if (candidates.Count == 0) { destination = default; fishingCell = default; return false; }
            KeyValuePair<Vector3, Vector3Int> best = candidates
                .OrderBy(candidate => (player.transform.position - candidate.Key).sqrMagnitude).First();
            destination = best.Key;
            fishingCell = best.Value;
            return true;
        }

        private bool PathStaysOutOfWater(Vector3 from, Vector3 to)
        {
            int samples = Mathf.Max(1, Mathf.CeilToInt(Vector3.Distance(from, to) / 0.05f));
            for (int i = 1; i <= samples; i++)
                if (TouchesWater(Vector3.Lerp(from, to, i / (float)samples)))
                    return false;
            return true;
        }

        private bool TouchesWater(Vector3 worldPosition)
        {
            if (HasWaterAtWorldPosition(worldPosition)) return true;
            if (waterAvoidanceRadius <= 0f) return false;

            float diagonal = waterAvoidanceRadius * 0.7071068f;
            Vector3[] offsets =
            {
                new Vector3(waterAvoidanceRadius, 0f, 0f),
                new Vector3(-waterAvoidanceRadius, 0f, 0f),
                new Vector3(0f, 0f, waterAvoidanceRadius),
                new Vector3(0f, 0f, -waterAvoidanceRadius),
                new Vector3(diagonal, 0f, diagonal),
                new Vector3(diagonal, 0f, -diagonal),
                new Vector3(-diagonal, 0f, diagonal),
                new Vector3(-diagonal, 0f, -diagonal)
            };
            foreach (Vector3 offset in offsets)
                if (HasWaterAtWorldPosition(worldPosition + offset))
                    return true;
            return false;
        }

        private bool HasWaterAtWorldPosition(Vector3 worldPosition)
        {
            if (waterTilemaps == null) return false;
            foreach (Tilemap water in waterTilemaps)
            {
                if (water == null || !water.isActiveAndEnabled) continue;
                if (water.HasTile(water.WorldToCell(worldPosition))) return true;
            }
            return false;
        }

        private void UpdateAutoMove()
        {
            if (player.HasMoveInput) { ResetFishing(); return; }

            // Face the actual travel direction. Cursor-based facing can change even
            // when the selected fishing tile and destination stay the same.
            float moveX = autoMoveTarget.x - player.transform.position.x;
            if (Mathf.Abs(moveX) > arrivalDistance)
                player.SetFacingLeft(moveX > 0f);
            Vector3 nextPosition = Vector3.MoveTowards(
                player.transform.position, autoMoveTarget, autoMoveSpeed * Time.deltaTime);
            if (TouchesWater(nextPosition))
            {
                Debug.LogWarning("[Tile Fishing] 물 타일 침범을 방지하기 위해 자동 이동을 중단했습니다.", this);
                ResetFishing();
                return;
            }
            player.Move(nextPosition - player.transform.position);
            if (Vector3.Distance(player.transform.position, autoMoveTarget) <= arrivalDistance) StartFishing();
        }

        private void StartFishing()
        {
            state = FishingState.Casting; stateTimer = castAnimationDuration; moveHoldTimer = 0f;
            player.MovementLocked = true; player.AutoMovingVisual = false; PlaySound(fishRodThrowClip);

            if (activeWater != null)
            {
                float waterX = activeWater.GetCellCenterWorld(activeCell).x - player.transform.position.x;
                if (!Mathf.Approximately(waterX, 0f))
                    player.SetFacingLeft(waterX > 0f);
            }
        }

        private void UpdateFishing()
        {
            if (state == FishingState.ResolvingResult)
            { stateTimer -= Time.deltaTime; if (stateTimer <= 0f) FinishFishing(pendingSuccess); return; }
            if (!IsAnimationState() && player.HasMoveInput)
            { moveHoldTimer += Time.deltaTime; if (moveHoldTimer >= movementCancelHoldTime) { BeginResult(false); return; } }
            else moveHoldTimer = 0f;

            if (IsAnimationState() && !useAnimationEvents)
            { stateTimer -= Time.deltaTime; if (stateTimer <= 0f) CompleteCurrentFishingAnimation(); }
            else if (state == FishingState.WaitingForBite)
            { stateTimer -= Time.deltaTime; if (stateTimer <= 0f) { state = FishingState.BiteAnimation; stateTimer = biteAnimationDuration; } }
            else if (state == FishingState.Bite)
            { stateTimer += Time.deltaTime; if (stateTimer >= hookWindow) BeginResult(false); }
        }

        public void CompleteCurrentFishingAnimation()
        {
            switch (state)
            {
                case FishingState.Casting: state = FishingState.WaitingAnimation; stateTimer = waitingAnimationDuration; break;
                case FishingState.WaitingAnimation: state = FishingState.WaitingForBite; stateTimer = Random.Range(minBiteDelay, Mathf.Max(minBiteDelay, maxBiteDelay)); break;
                case FishingState.BiteAnimation: state = FishingState.Bite; stateTimer = 0f; break;
                case FishingState.Hooking: BeginResult(true); break;
            }
        }

        private void BeginHooking()
        { state = FishingState.Hooking; stateTimer = hookingAnimationDuration; if (!useAnimationEvents && hookingAnimationDuration <= 0f) BeginResult(true); }

        private void BeginResult(bool success)
        {
            if (state == FishingState.ResolvingResult) return;
            pendingSuccess = success; state = FishingState.ResolvingResult; moveHoldTimer = 0f;
            stateTimer = (fishingCatchStartClip != null ? fishingCatchStartClip.length : 0f) + resultSoundDelay;
            PlaySound(fishingCatchStartClip);
        }

        private void FinishFishing(bool success)
        {
            PlaySound(success ? fishingSuccessClip : fishingFailClip);
            if (success) Debug.Log("[Tile Fishing] 낚시 성공.", this);
            else Debug.LogWarning("[Tile Fishing] 낚시 실패.", this);
            ResetFishing();
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip == null) return;
            if (SoundManager.Instance != null) { SoundManager.Instance.PlaySFX(clip); return; }
            if (fallbackAudioSource == null || PlayerPrefs.GetInt("Mute_Sfx", 0) != 0) return;
            fallbackAudioSource.volume = PlayerPrefs.GetFloat("Sound_All", 0.5f) * PlayerPrefs.GetFloat("Sound_Sfx", 0.5f);
            fallbackAudioSource.PlayOneShot(clip);
        }

        private bool IsSequenceState() => state >= FishingState.Casting && state <= FishingState.ResolvingResult;
        private bool IsAnimationState() => state == FishingState.Casting || state == FishingState.WaitingAnimation || state == FishingState.BiteAnimation || state == FishingState.Hooking;

        private void ResetFishing()
        {
            state = FishingState.Idle; hoveredWater = null; activeWater = null; hoveredBody = null; activeBody = null; stateTimer = moveHoldTimer = 0f; pendingSuccess = false;
            HideVisuals();
            if (player != null) { player.AutoMovingVisual = false; player.MovementLocked = false; }
        }

        private void HideVisuals() { ClearOverlay(); SetLabelVisible(false); SetClickVisible(false); }
        private void OnValidate() { if (maxBiteDelay < minBiteDelay) maxBiteDelay = minBiteDelay; }
    }
}
