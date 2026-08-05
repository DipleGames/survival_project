using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace FishingTest
{
    public sealed class FishingTestController : MonoBehaviour
    {
        private enum FishingState
        {
            Idle,
            AutoMoving,
            Casting,
            WaitingAnimation,
            WaitingForBite,
            BiteAnimation,
            Bite,
            Hooking,
            ResolvingResult
        }

        [Header("Scene references")]
        [SerializeField] private Camera worldCamera;
        [SerializeField] private FishingTestPlayerController player;
        [SerializeField] private Tilemap lakeTilemap;
        [SerializeField] private Tilemap seaTilemap;
        [SerializeField] private Text cursorLabel;
        [SerializeField] private GameObject clickUiPrefab;
        [SerializeField] private AudioClip fishRodThrowClip;
        [SerializeField] private AudioClip fishingCatchStartClip;
        [SerializeField] private AudioClip fishingSuccessClip;
        [SerializeField] private AudioClip fishingFailClip;

        [Header("Interaction")]
        [Tooltip("1이면 캐릭터 중심 3x3 범위입니다.")]
        [SerializeField, Min(0)] private int detectionHalfExtent = 1;
        [SerializeField, Min(0.1f)] private float autoMoveSpeed = 3f;
        [SerializeField, Min(0.01f)] private float arrivalDistance = 0.05f;
        [SerializeField, Range(0f, 1f)] private float lakeOverlayAlpha = 0.5f;
        [SerializeField, Min(0f)] private float clickUiCursorOffset = 36f;

        [Header("Fishing timing")]
        [Tooltip("활성화하면 고정 시간 대신 애니메이션 이벤트가 단계 완료를 호출해야 합니다.")]
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

        private readonly Vector3Int[] adjacentCells =
        {
            Vector3Int.left, Vector3Int.right, Vector3Int.up, Vector3Int.down
        };

        private FishingState state;
        private Tilemap hoveredWater;
        private Vector3Int hoveredCell;
        private Tilemap activeWater;
        private Vector3Int activeCell;
        private Vector3 autoMoveTarget;
        private float stateTimer;
        private float moveHoldTimer;
        private bool pendingFishingSuccess;
        private bool overlayVisible;
        private GameObject clickUiInstance;
        private AudioSource fallbackAudioSource;
        private Tilemap[] landTilemaps;
        private Vector3Int transparentLakeCell;
        private Color originalLakeColor;

        private void Awake()
        {
            if (worldCamera == null)
                worldCamera = Camera.main;
            RefreshLandTilemaps();
            cursorLabel.gameObject.SetActive(false);
            CreateClickUiInstance();
            CreateFallbackAudioSource();
            ClearLakeOverlay();
        }

        private void CreateFallbackAudioSource()
        {
            fallbackAudioSource = gameObject.AddComponent<AudioSource>();
            fallbackAudioSource.playOnAwake = false;
            fallbackAudioSource.loop = false;
            fallbackAudioSource.spatialBlend = 0f;
        }

        private void CreateClickUiInstance()
        {
            if (clickUiPrefab == null)
                return;

            clickUiInstance = Instantiate(clickUiPrefab);
            clickUiInstance.name = clickUiPrefab.name + " (Fishing Test)";
            clickUiInstance.SetActive(false);
        }

        private void RefreshLandTilemaps()
        {
            landTilemaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None)
                .Where(tilemap => tilemap != null &&
                                  !tilemap.CompareTag("Lake") &&
                                  !tilemap.CompareTag("Sea"))
                .ToArray();
        }

        private void OnDisable()
        {
            ClearLakeOverlay();
            if (clickUiInstance != null)
                clickUiInstance.SetActive(false);
            if (player != null)
                player.MovementLocked = false;
        }

        private void Update()
        {
            UpdateHoveredTile();
            UpdateCursorLabel();

            if (state == FishingState.AutoMoving)
                UpdateAutoMove();
            else if (IsFishingSequenceState())
                UpdateFishing();

            if (Input.GetMouseButtonDown(0))
                HandleClick();
        }

        private void UpdateHoveredTile()
        {
            hoveredWater = null;
            ClearLakeOverlay();

            if (worldCamera == null || player == null)
                return;

            Ray mouseRay = worldCamera.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, lakeTilemap.transform.position);
            if (!groundPlane.Raycast(mouseRay, out float enter))
                return;
            Vector3 mouseWorld = mouseRay.GetPoint(enter);

            if (TryGetWaterAt(lakeTilemap, mouseWorld, out Vector3Int lakeCell))
            {
                hoveredWater = lakeTilemap;
                hoveredCell = lakeCell;
                if (IsInDetectionRange(lakeTilemap, lakeCell))
                    ShowLakeOverlay(lakeCell);
                return;
            }

            if (TryGetWaterAt(seaTilemap, mouseWorld, out Vector3Int seaCell))
            {
                hoveredWater = seaTilemap;
                hoveredCell = seaCell;
            }
        }

        private static bool TryGetWaterAt(Tilemap tilemap, Vector3 worldPosition, out Vector3Int cell)
        {
            cell = default;
            if (tilemap == null)
                return false;
            cell = tilemap.WorldToCell(worldPosition);
            return tilemap.HasTile(cell);
        }

        private bool IsInDetectionRange(Tilemap water, Vector3Int waterCell)
        {
            Vector3Int playerCell = water.WorldToCell(player.transform.position);
            return Mathf.Abs(playerCell.x - waterCell.x) <= detectionHalfExtent &&
                   Mathf.Abs(playerCell.y - waterCell.y) <= detectionHalfExtent;
        }

        private void ShowLakeOverlay(Vector3Int cell)
        {
            if (lakeTilemap == null)
                return;
            lakeTilemap.SetTileFlags(cell, TileFlags.None);
            originalLakeColor = lakeTilemap.GetColor(cell);
            Color transparentColor = originalLakeColor;
            transparentColor.a = lakeOverlayAlpha;
            lakeTilemap.SetColor(cell, transparentColor);
            transparentLakeCell = cell;
            overlayVisible = true;
        }

        private void ClearLakeOverlay()
        {
            if (!overlayVisible || lakeTilemap == null)
                return;
            lakeTilemap.SetColor(transparentLakeCell, originalLakeColor);
            overlayVisible = false;
        }

        private void UpdateCursorLabel()
        {
            bool validHover = hoveredWater != null && IsInDetectionRange(hoveredWater, hoveredCell);
            cursorLabel.gameObject.SetActive(validHover);
            if (!validHover)
            {
                SetClickUiVisible(false);
                return;
            }

            if (IsFishingAnimationState() || state == FishingState.ResolvingResult)
            {
                cursorLabel.gameObject.SetActive(false);
                SetClickUiVisible(false);
                return;
            }

            bool isActiveTile = activeWater == hoveredWater && activeCell == hoveredCell &&
                                IsFishingSequenceState();
            cursorLabel.text = isActiveTile
                ? (state == FishingState.Bite ? "낚아채기" : "낚시 중단하기")
                : "찌 던지기";
            cursorLabel.rectTransform.position = Input.mousePosition + new Vector3(18f, -18f, 0f);

            bool showClickUi = !isActiveTile || state == FishingState.Bite;
            SetClickUiVisible(showClickUi);
        }

        private void SetClickUiVisible(bool visible)
        {
            if (clickUiInstance == null)
                return;

            clickUiInstance.SetActive(visible);
            if (!visible || worldCamera == null || hoveredWater == null)
                return;

            Vector3 screenPosition = Input.mousePosition + Vector3.down * clickUiCursorOffset;
            Ray cursorRay = worldCamera.ScreenPointToRay(screenPosition);
            Plane waterPlane = new Plane(Vector3.up, hoveredWater.transform.position);
            if (!waterPlane.Raycast(cursorRay, out float enter))
            {
                clickUiInstance.SetActive(false);
                return;
            }

            clickUiInstance.transform.position = cursorRay.GetPoint(enter) - worldCamera.transform.forward * 0.01f;
            clickUiInstance.transform.rotation = Quaternion.LookRotation(
                worldCamera.transform.forward,
                worldCamera.transform.up);
        }

        private void HandleClick()
        {
            if (hoveredWater == null || !IsInDetectionRange(hoveredWater, hoveredCell))
                return;

            if (IsFishingAnimationState() || state == FishingState.ResolvingResult)
                return;

            if (IsFishingSequenceState())
            {
                bool clickedActiveTile = hoveredWater == activeWater && hoveredCell == activeCell;
                if (!clickedActiveTile)
                {
                    BeginFishingResult(false);
                    return;
                }

                if (state == FishingState.Bite && stateTimer <= hookWindow)
                    BeginHooking();
                else
                    BeginFishingResult(false);
                return;
            }

            if (!TryFindFishingPoint(hoveredWater, hoveredCell, out Vector3 destination))
            {
                Debug.Log("낚시 불가능: 인접한 육지 타일이 없습니다.");
                return;
            }

            activeWater = hoveredWater;
            activeCell = hoveredCell;
            autoMoveTarget = destination;
            state = FishingState.AutoMoving;
            player.MovementLocked = true;
            player.AutoMovingVisual = true;
        }

        private bool IsFishingSequenceState()
        {
            return state == FishingState.Casting ||
                   state == FishingState.WaitingAnimation ||
                   state == FishingState.WaitingForBite ||
                   state == FishingState.BiteAnimation ||
                   state == FishingState.Bite ||
                   state == FishingState.Hooking ||
                   state == FishingState.ResolvingResult;
        }

        private bool IsFishingAnimationState()
        {
            return state == FishingState.Casting ||
                   state == FishingState.WaitingAnimation ||
                   state == FishingState.BiteAnimation ||
                   state == FishingState.Hooking;
        }

        private bool TryFindFishingPoint(Tilemap water, Vector3Int waterCell, out Vector3 destination)
        {
            destination = default;
            if (landTilemaps == null || landTilemaps.Length == 0)
                RefreshLandTilemaps();

            var candidates = new List<Vector3>();
            foreach (Vector3Int offset in adjacentCells)
            {
                Vector3 probe = water.GetCellCenterWorld(waterCell + offset);
                foreach (Tilemap landTilemap in landTilemaps)
                {
                    Vector3Int groundCell = landTilemap.WorldToCell(probe);
                    if (landTilemap.HasTile(groundCell))
                    {
                        candidates.Add(landTilemap.GetCellCenterWorld(groundCell));
                        break;
                    }
                }
            }

            if (candidates.Count == 0)
                return false;

            destination = candidates[0];
            float bestDistance = (player.transform.position - destination).sqrMagnitude;
            for (int i = 1; i < candidates.Count; i++)
            {
                float distance = (player.transform.position - candidates[i]).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    destination = candidates[i];
                }
            }
            destination.y = player.transform.position.y;
            return true;
        }

        private void UpdateAutoMove()
        {
            if (player.HasMoveInput)
            {
                CancelToIdle();
                return;
            }

            if (worldCamera != null)
            {
                float playerScreenX = worldCamera.WorldToScreenPoint(player.transform.position).x;
                player.SetFacingLeft(Input.mousePosition.x > playerScreenX);
            }

            player.transform.position = Vector3.MoveTowards(
                player.transform.position, autoMoveTarget, autoMoveSpeed * Time.deltaTime);

            if (Vector3.Distance(player.transform.position, autoMoveTarget) <= arrivalDistance)
                StartFishing();
        }

        private void StartFishing()
        {
            state = FishingState.Casting;
            stateTimer = castAnimationDuration;
            moveHoldTimer = 0f;
            player.MovementLocked = true;
            player.AutoMovingVisual = false;
            PlayFishingSound(fishRodThrowClip);
        }

        private void UpdateFishing()
        {
            if (state == FishingState.ResolvingResult)
            {
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0f)
                    FinishFishing(pendingFishingSuccess);
                return;
            }

            if (!IsFishingAnimationState() && player.HasMoveInput)
            {
                moveHoldTimer += Time.deltaTime;
                if (moveHoldTimer >= movementCancelHoldTime)
                {
                    BeginFishingResult(false);
                    return;
                }
            }
            else
            {
                moveHoldTimer = 0f;
            }

            if (state == FishingState.Casting ||
                state == FishingState.WaitingAnimation ||
                state == FishingState.BiteAnimation ||
                state == FishingState.Hooking)
            {
                if (!useAnimationEvents)
                {
                    stateTimer -= Time.deltaTime;
                    if (stateTimer <= 0f)
                        CompleteCurrentFishingAnimation();
                }
            }
            else if (state == FishingState.WaitingForBite)
            {
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0f)
                    BeginBiteAnimation();
            }
            else if (state == FishingState.Bite)
            {
                stateTimer += Time.deltaTime;
                if (stateTimer >= hookWindow)
                {
                    BeginFishingResult(false);
                    return;
                }
            }
        }

        public void CompleteCurrentFishingAnimation()
        {
            switch (state)
            {
                case FishingState.Casting:
                    state = FishingState.WaitingAnimation;
                    stateTimer = waitingAnimationDuration;
                    break;
                case FishingState.WaitingAnimation:
                    state = FishingState.WaitingForBite;
                    stateTimer = UnityEngine.Random.Range(
                        minBiteDelay,
                        Mathf.Max(minBiteDelay, maxBiteDelay));
                    break;
                case FishingState.BiteAnimation:
                    state = FishingState.Bite;
                    stateTimer = 0f;
                    break;
                case FishingState.Hooking:
                    BeginFishingResult(true);
                    break;
            }
        }

        private void BeginBiteAnimation()
        {
            state = FishingState.BiteAnimation;
            stateTimer = biteAnimationDuration;
        }

        private void BeginHooking()
        {
            BeginFishingResult(true);
        }

        private void BeginFishingResult(bool success)
        {
            if (state == FishingState.ResolvingResult)
                return;

            pendingFishingSuccess = success;
            state = FishingState.ResolvingResult;
            moveHoldTimer = 0f;
            stateTimer = (fishingCatchStartClip != null ? fishingCatchStartClip.length : 0f) +
                         resultSoundDelay;
            stateTimer = Mathf.Max(stateTimer, hookingAnimationDuration);
            PlayFishingSound(fishingCatchStartClip);
        }

        private void PlayFishingSound(AudioClip clip)
        {
            if (clip == null)
                return;

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(clip);
                return;
            }

            if (fallbackAudioSource == null || PlayerPrefs.GetInt("Mute_Sfx", 0) != 0)
                return;

            float wholeVolume = PlayerPrefs.GetFloat("Sound_All", 0.5f);
            float sfxVolume = PlayerPrefs.GetFloat("Sound_Sfx", 0.5f);
            fallbackAudioSource.volume = wholeVolume * sfxVolume;
            fallbackAudioSource.PlayOneShot(clip);
        }

        private void FinishFishing(bool success)
        {
            // 추후 이 지점에서 낚아채기/획득 애니메이션과 인벤토리를 연결한다.
            PlayFishingSound(success ? fishingSuccessClip : fishingFailClip);
            Debug.Log(success ? "낚시 성공" : "낚시 실패");
            CancelToIdle();
        }

        private void CancelToIdle()
        {
            state = FishingState.Idle;
            activeWater = null;
            stateTimer = 0f;
            moveHoldTimer = 0f;
            pendingFishingSuccess = false;
            player.MovementLocked = false;
            player.AutoMovingVisual = false;
        }

        private void OnValidate()
        {
            if (maxBiteDelay < minBiteDelay)
                maxBiteDelay = minBiteDelay;
        }
    }
}
