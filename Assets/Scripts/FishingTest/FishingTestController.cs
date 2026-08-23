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

        [Tooltip("비어 있을 때, 'Lake' 또는 'Sea' 태그가 지정된 모든 활성 타일맵이 자동으로 감지")]
        [SerializeField] private Tilemap[] waterTilemaps;

        [SerializeField] private Text cursorLabel;
        [SerializeField] private GameObject clickUiPrefab;
        [SerializeField] private AudioClip fishRodThrowClip;
        [SerializeField] private AudioClip fishingCatchStartClip;
        [SerializeField] private AudioClip fishingSuccessClip;
        [SerializeField] private AudioClip fishingFailClip;

        [Header("Interaction")]
        [SerializeField, Min(0)] private int detectionHalfExtent = 2;
        [SerializeField, Min(0.1f)] private float autoMoveSpeed = 3f;
        [SerializeField, Min(0.01f)] private float arrivalDistance = 0.05f;

        [Tooltip("캐릭터 반경을 고려하여 수역과 접촉하지 않도록 자동 이동 경로를 샘플링할 때 사용되는 반지름")]
        [SerializeField, Min(0f)] private float waterAvoidanceRadius = 0.15f;

        [SerializeField, Range(0f, 1f)] private float waterOverlayAlpha = 0.5f;

        [Header("Cursor-following UI")]
        [SerializeField] private Vector2 labelCursorOffset = new Vector2(-120f, 30f);
        [SerializeField] private Vector2 clickUiCursorOffset = new Vector2(-20f, -80f);

        [Tooltip("낚시 커서 라벨과 클릭 안내 UI의 크기를 조정할 때 기준이 되는 커서 이미지의 픽셀 크기")]
        [SerializeField, Min(1f)] private float referenceCursorPixelSize = 75f;

        [SerializeField, Min(1)] private int baseLabelFontSize = 15;
        [SerializeField] private Vector3 baseClickUiScale = new Vector3(0.04f, 0.04f, 0.04f);
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

            // 두 수역 셀 키가 같은 Tilemap과 셀 좌표를 가리키는지 비교한다.
            public override bool Equals(object obj)
            {
                if (!(obj is WaterCellKey)) 
                {
                    return false;
                }

                WaterCellKey other = (WaterCellKey)obj;

                return Tilemap == other.Tilemap && Cell == other.Cell;
            }

            // Tilemap과 셀 좌표를 조합한 해시 코드를 생성한다.
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

        // 씬 참조, 수역 정보, 커서 UI와 효과음 재생용 AudioSource를 초기화한다.
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

        // 컴포넌트가 활성화될 때 씬 참조와 Tilemap 목록을 다시 구성한다.
        private void OnEnable() { ResolveReferences(); RefreshTilemaps(); }
        // 컴포넌트가 비활성화될 때 진행 중인 낚시와 화면 연출을 초기화한다.
        private void OnDisable() { ResetFishing(); }
        // 컴포넌트가 파괴될 때 생성했던 클릭 안내 UI를 제거한다.
        private void OnDestroy() { if (clickUiInstance != null) Destroy(clickUiInstance); }

        // 게임 가능 상태를 확인하고 호버, 클릭, 자동 이동과 낚시 상태를 갱신한다.
        private void Update()
        {
            ResolveReferences();
            UpdateUiScale();

            if (GameplayUnavailable())
            {
                if (state != FishingState.Idle) 
                {
                    ResetFishing();
                }
                else
                {
                    HideVisuals();
                }

                return;
            }

            if (player == null) 
            { 
                HideVisuals(); return; 
            }

            UpdateHoveredTile();
            UpdateCursorUi();

            if (state == FishingState.AutoMoving) 
            {
                UpdateAutoMove();
            }
            else if (IsSequenceState()) 
            {
                UpdateFishing();
            }

            if (Input.GetMouseButtonDown(0) && !IsPointerOverUi()) 
            {
                HandleClick();
            }
        }

        // 메인 카메라와 Character에 연결된 낚시 플레이어 컨트롤러를 찾는다.
        private void ResolveReferences()
        {
            if (worldCamera == null) 
            {
                worldCamera = Camera.main;
            }

            if (player == null && Character.Instance != null)
            {
                player = Character.Instance.GetComponent<FishingTestPlayerController>();
            }

            if (player == null && !missingPlayerReported)
            {
                Debug.LogError("[Tile Fishing] Add FishingTestPlayerController to the Character GameObject.", this);
                missingPlayerReported = true;
            }
            else if (player != null) 
            {
                missingPlayerReported = false;
            }
        }

        // 현재 씬, 일시정지, 밤 상태를 기준으로 낚시가 금지되었는지 확인한다.
        private bool GameplayUnavailable()
        {
            GameManager gm = GameManager.Instance;

            if (gm == null)
            {
                return player != null && player.GetComponent<Character>() != null;
            }

            if (gm.currentScene != "Game" || gm.isPause) 
            {
                return true;
            }

            return GamesceneManager.Instance != null && GamesceneManager.Instance.isNight;
        }

        // 현재 마우스 포인터가 EventSystem UI 위에 있는지 확인한다.
        private static bool IsPointerOverUi() => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        // Tilemap의 태그가 Lake 또는 Sea인지 판별한다.
        private static bool IsWater(Tilemap map) => map != null && (map.CompareTag("Lake") || map.CompareTag("Sea"));

        // 씬의 수역 및 육지 Tilemap 목록을 분류하고 수역 연결 정보를 다시 만든다.
        private void RefreshTilemaps()
        {
            Tilemap[] all = FindObjectsOfType<Tilemap>();

            if (waterTilemaps == null || waterTilemaps.Length == 0) 
            {
                waterTilemaps = all.Where(IsWater).ToArray();
            }

            landTilemaps = all.Where(map => map != null && !IsWater(map)).ToArray();
            RebuildWaterBodies();
        }

        // 인접한 물 타일들을 하나의 연결된 수역 단위로 묶어 검색 사전을 생성한다.
        private void RebuildWaterBodies()
        {
            waterBodies.Clear();

            if (waterTilemaps == null) 
            {
                return;
            }

            foreach (Tilemap water in waterTilemaps)
            {
                if (water == null) 
                {
                    continue;
                }

                foreach (Vector3Int start in water.cellBounds.allPositionsWithin)
                {
                    var startKey = new WaterCellKey { Tilemap = water, Cell = start };

                    if (!water.HasTile(start) || waterBodies.ContainsKey(startKey)) 
                    {
                        continue;
                    }

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

                            if (!water.HasTile(next) || waterBodies.ContainsKey(key)) 
                            {
                                continue;
                            }

                            waterBodies.Add(key, body);
                            pending.Enqueue(next);
                        }
                    }
                }
            }
        }

        // 낚시 가능 위치에 표시할 클릭 안내 UI 인스턴스를 생성한다.
        private void CreateClickUi()
        {
            if (clickUiPrefab == null || clickUiInstance != null) 
            {
                return;
            }

            clickUiInstance = Instantiate(clickUiPrefab);
            clickUiInstance.name = clickUiPrefab.name + " (Tile Fishing)";
            clickUiInstance.SetActive(false);

            UpdateUiScale();
        }

        // 현재 커서 크기와 배율 설정에 맞춰 라벨과 클릭 UI 크기를 조절한다.
        private void UpdateUiScale()
        {
            float cursorPixels = referenceCursorPixelSize;

            if (GameManager.Instance != null && GameManager.Instance.useCursorNormal != null)
            {
                cursorPixels = GameManager.Instance.useCursorNormal.width;
            }

            float scale = Mathf.Max(0.1f, cursorPixels / referenceCursorPixelSize) * uiScaleMultiplier;

            if (cursorLabel != null) 
            {
                cursorLabel.fontSize = Mathf.Max(1, Mathf.RoundToInt(baseLabelFontSize * scale));
            }

            if (clickUiInstance != null) 
            {
                clickUiInstance.transform.localScale = baseClickUiScale * scale;
            }
        }

        // 마우스 레이와 교차하는 수역 타일 및 연결된 수역을 찾아 호버 상태를 갱신한다.
        private void UpdateHoveredTile()
        {
            hoveredWater = null;
            hoveredBody = null;

            ClearOverlay();

            if (worldCamera == null || waterTilemaps == null) 
            {
                return;
            }

            Ray ray = worldCamera.ScreenPointToRay(Input.mousePosition);

            foreach (Tilemap water in waterTilemaps)
            {
                if (water == null || !water.isActiveAndEnabled) 
                {
                    continue;
                }

                Plane plane = new Plane(Vector3.up, water.transform.position);

                if (!plane.Raycast(ray, out float enter)) 
                {
                    continue;
                }

                Vector3Int cell = water.WorldToCell(ray.GetPoint(enter));

                if (!water.HasTile(cell)) 
                {
                    continue;
                }

                hoveredWater = water;
                waterBodies.TryGetValue(new WaterCellKey { Tilemap = water, Cell = cell }, out hoveredBody);
                
                if (InRange(hoveredBody) && water.CompareTag("Lake")) 
                {
                    ShowOverlay(hoveredBody);
                }
                return;
            }
        }

        // 연결된 수역이 캐릭터 주변의 낚시 감지 범위 안에 있는지 확인한다.
        private bool InRange(WaterBody body)
        {
            if (player == null || body == null || body.Tilemap == null) 
            {
                return false;
            }

            Vector3Int playerCell = body.Tilemap.WorldToCell(player.transform.position);

            return body.Cells.Any(cell =>
                Mathf.Abs(playerCell.x - cell.x) <= detectionHalfExtent &&
                Mathf.Abs(playerCell.y - cell.y) <= detectionHalfExtent);
        }

        // 선택한 호수 수역 전체를 반투명하게 표시하고 원래 색상을 저장한다.
        private void ShowOverlay(WaterBody body)
        {
            if (body == null) 
            {
                return;
            }

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

        // 수역 강조에 사용한 타일 색상을 원래 값으로 복구한다.
        private void ClearOverlay()
        {
            if (!overlayVisible) 
            {
                return;
            }

            if (overlayBody != null && overlayBody.Tilemap != null)
            {
                for (int i = 0; i < overlayBody.Cells.Count && i < originalWaterColors.Count; i++)
                {
                    overlayBody.Tilemap.SetColor(overlayBody.Cells[i], originalWaterColors[i]);
                }
            }

            overlayBody = null;
            originalWaterColors.Clear();
            overlayVisible = false;
        }

        // 호버와 낚시 진행 상태에 따라 커서 라벨의 문구와 클릭 UI를 갱신한다.
        private void UpdateCursorUi()
        {
            bool valid = hoveredBody != null && InRange(hoveredBody);
            SetLabelVisible(valid);

            if (!valid) 
            {
                SetClickVisible(false);
                return;
            }

            if (IsAnimationState() || state == FishingState.ResolvingResult)
            { 
                SetLabelVisible(false); SetClickVisible(false); return; 
            }

            bool active = activeBody == hoveredBody && IsSequenceState();

            if (cursorLabel != null)
            {
                cursorLabel.text = active ? (state == FishingState.Bite ? "낚아채기" : "낚시 중단하기") : "찌 던지기";
                PositionCursorLabel();
            }

            SetClickVisible(!active || state == FishingState.Bite);
        }

        // 낚시 커서 라벨의 활성 상태를 변경한다.
        private void SetLabelVisible(bool visible) 
        {   if (cursorLabel != null) 
            {
                cursorLabel.gameObject.SetActive(visible); 
            }
        }

        // 낚시 커서 라벨을 마우스 위치에 지정된 화면 오프셋으로 배치한다.
        private void PositionCursorLabel()
        {
            if (cursorLabel == null) 
            {
                return;
            }

            if (cursorCanvas == null) 
            {
                cursorCanvas = cursorLabel.GetComponentInParent<Canvas>();
            }

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

        // 클릭 안내 UI의 표시 여부를 변경하고 호버 수면 위에 배치한다.
        private void SetClickVisible(bool visible)
        {
            if (clickUiInstance == null) 
            {
                return;
            }

            clickUiInstance.SetActive(visible);

            if (!visible || worldCamera == null || hoveredWater == null) 
            {
                return;
            }

            Ray ray = worldCamera.ScreenPointToRay(Input.mousePosition + (Vector3)clickUiCursorOffset);
            Plane plane = new Plane(Vector3.up, hoveredWater.transform.position);

            if (!plane.Raycast(ray, out float enter)) 
            { 
                clickUiInstance.SetActive(false); 
                return; 
            }

            clickUiInstance.transform.position = ray.GetPoint(enter) - worldCamera.transform.forward * 0.01f;
            clickUiInstance.transform.rotation = Quaternion.LookRotation(worldCamera.transform.forward, worldCamera.transform.up);
        }

        // 수역 클릭에 따라 낚시 시작, 중단 또는 입질 성공 입력을 처리한다.
        private void HandleClick()
        {
            if (hoveredBody == null || !InRange(hoveredBody) || IsAnimationState() || state == FishingState.ResolvingResult) 
            {
                return;
            }

            if (IsSequenceState())
            {
                bool same = activeBody == hoveredBody;
                if (same && state == FishingState.Bite && stateTimer <= hookWindow) 
                {
                    BeginHooking();
                }
                else
                {
                    BeginResult(false);
                }

                return;
            }

            if (!FindFishingPoint(hoveredBody, out Vector3 destination, out Vector3Int fishingCell))
            { 
                Debug.Log("[Tile Fishing] 사용가능한 인접 육지 타일이 없습니다.", this); 
                return; 
            }

            activeBody = hoveredBody; activeWater = hoveredBody.Tilemap; activeCell = fishingCell; autoMoveTarget = destination;
            state = FishingState.AutoMoving; player.MovementLocked = true; player.AutoMovingVisual = true;
        }

        // 수역에 인접하고 물을 통과하지 않는 가장 가까운 낚시 대기 위치를 찾는다.
        private bool FindFishingPoint(WaterBody body, out Vector3 destination, out Vector3Int fishingCell)
        {
            var candidates = new List<KeyValuePair<Vector3, Vector3Int>>();
            var uniqueGroundCells = new HashSet<WaterCellKey>();

            foreach (Vector3Int waterCell in body.Cells)
            foreach (Vector3Int offset in AdjacentCells)
            {
                Vector3 probe = body.Tilemap.GetCellCenterWorld(waterCell + offset);

                if (HasWaterAtWorldPosition(probe)) 
                {
                    continue;
                }

                foreach (Tilemap land in landTilemaps)
                {
                    Vector3Int groundCell = land.WorldToCell(probe);
                    var key = new WaterCellKey { Tilemap = land, Cell = groundCell };

                    if (!land.HasTile(groundCell) || !uniqueGroundCells.Add(key)) 
                    {
                        continue;
                    }

                    Vector3 point = land.GetCellCenterWorld(groundCell);
                    point.y = player.transform.position.y;

                    if (!PathStaysOutOfWater(player.transform.position, point)) 
                    {
                        continue;
                    }
                    
                    candidates.Add(new KeyValuePair<Vector3, Vector3Int>(point, waterCell));
                }
            }

            if (candidates.Count == 0) 
            { 
                destination = default; 
                fishingCell = default; 
                return false; 
            }

            KeyValuePair<Vector3, Vector3Int> best = candidates
                .OrderBy(candidate => (player.transform.position - candidate.Key).sqrMagnitude).First();

            destination = best.Key;
            fishingCell = best.Value;

            return true;
        }

        // 시작점에서 목적지까지 직선 경로가 물 영역과 접촉하지 않는지 표본 검사한다.
        private bool PathStaysOutOfWater(Vector3 from, Vector3 to)
        {
            int samples = Mathf.Max(1, Mathf.CeilToInt(Vector3.Distance(from, to) / 0.05f));

            for (int i = 1; i <= samples; i++)
            {
                if (TouchesWater(Vector3.Lerp(from, to, i / (float)samples)))
                {
                    return false;
                }
            }
            return true;
        }

        // 캐릭터 반경을 고려하여 지정 위치가 물 타일에 닿는지 확인한다.
        private bool TouchesWater(Vector3 worldPosition)
        {
            if (HasWaterAtWorldPosition(worldPosition)) 
            {
                return true;
            }

            if (waterAvoidanceRadius <= 0f) 
            {
                return false;
            }

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
            {
                if (HasWaterAtWorldPosition(worldPosition + offset))
                {
                    return true;
                }
            }
            return false;
        }

        // 지정한 월드 위치에 활성 수역 Tilemap의 물 타일이 있는지 확인한다.
        private bool HasWaterAtWorldPosition(Vector3 worldPosition)
        {
            if (waterTilemaps == null) 
            {
                return false;
            }

            foreach (Tilemap water in waterTilemaps)
            {
                if (water == null || !water.isActiveAndEnabled) 
                {
                    continue;
                }

                if (water.HasTile(water.WorldToCell(worldPosition))) 
                {
                    return true;
                }
            }

            return false;
        }

        // 캐릭터를 낚시 지점으로 이동시키며 수동 입력과 물 침범 시 이동을 취소한다.
        private void UpdateAutoMove()
        {
            if (player.HasMoveInput) 
            {
                ResetFishing();
                return;
            }

            // Face the actual travel direction. Cursor-based facing can change even
            // when the selected fishing tile and destination stay the same.
            float moveX = autoMoveTarget.x - player.transform.position.x;
            if (Mathf.Abs(moveX) > arrivalDistance)
            {
                player.SetFacingLeft(moveX > 0f);
            }

            Vector3 nextPosition = Vector3.MoveTowards(
                player.transform.position, autoMoveTarget, autoMoveSpeed * Time.deltaTime);
            
            if (TouchesWater(nextPosition))
            {
                Debug.LogWarning("[Tile Fishing] 물 타일 침범을 방지하기 위해 자동 이동을 중단했습니다.", this);
                ResetFishing();
                return;
            }

            player.Move(nextPosition - player.transform.position);

            if (Vector3.Distance(player.transform.position, autoMoveTarget) <= arrivalDistance) 
            {
                StartFishing();
            }
        }

        // 자동 이동 완료 후 캐스팅 상태로 전환하고 방향과 효과음을 설정한다.
        private void StartFishing()
        {
            state = FishingState.Casting; stateTimer = castAnimationDuration; moveHoldTimer = 0f;
            player.MovementLocked = true; player.AutoMovingVisual = false; PlaySound(fishRodThrowClip);

            if (activeWater != null)
            {
                float waterX = activeWater.GetCellCenterWorld(activeCell).x - player.transform.position.x;

                if (!Mathf.Approximately(waterX, 0f))
                {
                    player.SetFacingLeft(waterX > 0f);
                }
            }
        }

        // 현재 낚시 단계의 타이머, 취소 입력, 입질과 결과 전환을 처리한다.
        private void UpdateFishing()
        {
            if (state == FishingState.ResolvingResult)
            { 
                stateTimer -= Time.deltaTime; 

                if (stateTimer <= 0f) 
                {
                    FinishFishing(pendingSuccess); 
                    return; 
                }
            }

            if (!IsAnimationState() && player.HasMoveInput)
            { 
                moveHoldTimer += Time.deltaTime; 

                if (moveHoldTimer >= movementCancelHoldTime) 
                { 
                    BeginResult(false); 
                    return; 
                } 
            }
            else moveHoldTimer = 0f;

            if (IsAnimationState() && !useAnimationEvents)
            { 
                stateTimer -= Time.deltaTime; 

                if (stateTimer <= 0f) 
                {
                    CompleteCurrentFishingAnimation();
                }
            }
            else if (state == FishingState.WaitingForBite)
            { 
                stateTimer -= Time.deltaTime; 

                if (stateTimer <= 0f) 
                { 
                    state = FishingState.BiteAnimation; 
                    stateTimer = biteAnimationDuration; 
                } 
            }
            else if (state == FishingState.Bite)
            { 
                stateTimer += Time.deltaTime; if (stateTimer >= hookWindow) BeginResult(false); 
            }
        }

        // 현재 낚시 애니메이션 단계의 완료를 받아 다음 상태로 전환한다.
        public void CompleteCurrentFishingAnimation()
        {
            switch (state)
            {
                case FishingState.Casting: 
                {
                    state = FishingState.WaitingAnimation; stateTimer = waitingAnimationDuration; 
                    break;
                }
                case FishingState.WaitingAnimation: 
                {
                    state = FishingState.WaitingForBite; stateTimer = Random.Range(minBiteDelay, Mathf.Max(minBiteDelay, maxBiteDelay)); 
                    break;
                }
                case FishingState.BiteAnimation: 
                {
                    state = FishingState.Bite; stateTimer = 0f; 
                    break;
                }
                case FishingState.Hooking: 
                {
                    BeginResult(true); 
                    break;
                }
            }
        }

        // 입질 입력 성공 후 낚싯줄을 당기는 Hooking 상태를 시작한다.
        private void BeginHooking()
        {   
            state = FishingState.Hooking; 
            stateTimer = hookingAnimationDuration; 

            if (!useAnimationEvents && hookingAnimationDuration <= 0f) 
            {
                BeginResult(true);
            }
        }

        // 성공 여부를 저장하고 낚시 결과 처리 및 효과음 대기 상태로 전환한다.
        private void BeginResult(bool success)
        {
            if (state == FishingState.ResolvingResult) 
            {
                return;
            }

            pendingSuccess = success; state = FishingState.ResolvingResult; moveHoldTimer = 0f;
            stateTimer = (fishingCatchStartClip != null ? fishingCatchStartClip.length : 0f) + resultSoundDelay;

            PlaySound(fishingCatchStartClip);
        }

        // 성공 또는 실패 효과음과 로그를 출력한 뒤 낚시 상태를 초기화한다.
        private void FinishFishing(bool success)
        {
            PlaySound(success ? fishingSuccessClip : fishingFailClip);

            if (success) 
            {
                Debug.Log("[Tile Fishing] 낚시 성공.", this);
            }
            else 
            {
                Debug.LogWarning("[Tile Fishing] 낚시 실패.", this);
            }

            ResetFishing();
        }

        // SoundManager 또는 대체 AudioSource를 통해 지정한 효과음을 재생한다.
        private void PlaySound(AudioClip clip)
        {
            if (clip == null) 
            {
                return;
            }

            if (SoundManager.Instance != null) 
            {
                SoundManager.Instance.PlaySFX(clip);
                return;
            }

            if (fallbackAudioSource == null || PlayerPrefs.GetInt("Mute_Sfx", 0) != 0) 
            {
                return;
            }

            fallbackAudioSource.volume = PlayerPrefs.GetFloat("Sound_All", 0.5f) * PlayerPrefs.GetFloat("Sound_Sfx", 0.5f);
            fallbackAudioSource.PlayOneShot(clip);
        }

        // 현재 상태가 캐스팅 이후 결과 처리까지의 낚시 진행 단계인지 확인한다.
        private bool IsSequenceState() => state >= FishingState.Casting && state <= FishingState.ResolvingResult;
        // 현재 상태가 낚시 애니메이션이 재생되는 단계인지 확인한다.
        private bool IsAnimationState() => state == FishingState.Casting || state == FishingState.WaitingAnimation || state == FishingState.BiteAnimation || state == FishingState.Hooking;

        // 낚시 상태, 수역 선택, 타이머, UI와 캐릭터 이동 잠금을 초기화한다.
        private void ResetFishing()
        {
            state = FishingState.Idle; hoveredWater = null; activeWater = null; hoveredBody = null; activeBody = null; stateTimer = moveHoldTimer = 0f; pendingSuccess = false;
            HideVisuals();

            if (player != null) 
            {
                player.AutoMovingVisual = false;
                player.MovementLocked = false;
            }
        }

        // 수역 강조와 커서 라벨 및 클릭 안내 UI를 모두 숨긴다.
        private void HideVisuals() 
        { 
            ClearOverlay(); 
            SetLabelVisible(false); 
            SetClickVisible(false); 
        }

        // 인스펙터 값 변경 시 최대 입질 대기 시간이 최소값보다 작아지지 않게 보정한다.
        private void OnValidate() 
        { 
            if (maxBiteDelay < minBiteDelay) 
            {
                maxBiteDelay = minBiteDelay;
            }
        }
    }
}
