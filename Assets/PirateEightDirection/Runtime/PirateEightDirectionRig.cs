using System;
using UnityEngine;
using UnityEngine.AI;

namespace PirateEightDirection
{
    // XZ 이동: 부모는 Rigidbody, 이 컴포넌트는 시각 자식에 배치합니다.
    [DisallowMultipleComponent]
    public sealed class PirateEightDirectionRig : MonoBehaviour
    {
        private enum Facing { South, SouthWest, West, NorthWest, North, NorthEast, East, SouthEast }

        [Header("Motion")]
        [SerializeField] private Rigidbody motionSource;
        [SerializeField] private NavMeshAgent navMeshMotionSource;
        [SerializeField, Min(0.01f)] private float walkingThreshold = 0.08f;
        [SerializeField, Min(0.1f)] private float referenceWalkSpeed = 2.5f;
        [SerializeField, Min(0.1f)] private float walkCyclesPerSecond = 2.1f;
        [SerializeField, Min(0.02f)] private float walkBlendTime = 0.10f;
        [SerializeField] private Vector2 initialFacing = Vector2.down;

        [Header("Relaxed Arms (local Z degrees)")]
        [Tooltip("For the supplied left-screen BackArm pivot, positive values lower the hand.")]
        [SerializeField, Range(-60f, 60f)] private float backArmRestAngle = 22f;
        [SerializeField, Range(-60f, 60f)] private float frontArmRestAngle = -22f;
        [Tooltip("Optional extra angles (BackArm, FrontArm) for S, SW, W, NW, N, NE, E, SE.")]
        [SerializeField] private Vector2[] directionArmOffsets = new Vector2[8];

        [Header("Walk Detail")]
        [SerializeField, Range(0f, 15f)] private float armSwingDegrees = 4f;
        [SerializeField, Range(0f, 20f)] private float legSwingDegrees = 7f;
        [SerializeField, Range(0f, 0.2f)] private float footLift = 0.055f;
        [SerializeField, Range(0f, 0.2f)] private float strideLength = 0.055f;
        [SerializeField, Range(0f, 0.05f)] private float bodyBob = 0.009f;
        [SerializeField, Range(0f, 8f)] private float forwardLeanDegrees = 2.5f;
        [SerializeField, Range(0f, 0.05f)] private float idleBreathing = 0.008f;

        [Header("Roll")]
        [SerializeField, Min(0.1f)] private float rollDuration = 0.32f;
        [SerializeField, Range(0f, 0.5f)] private float rollHopHeight = 0.08f;
        [SerializeField, Range(0f, 1f)] private float rollTuckStrength = 0.8f;
        [SerializeField] private Vector2 rollPivot = new Vector2(0f, -0.15f);

        [Header("Rendering")]
        [SerializeField] private string sortingLayerName = "Default";
        [SerializeField] private int sortingOrder;
        [SerializeField] private Color tint = Color.white;

        private static readonly string[] DirectionNames = Enum.GetNames(typeof(Facing));
        private static readonly string[] PartNames = { "BackArm", "BackLeg", "Torso", "FrontLeg", "FrontArm", "Head" };
        private static readonly Vector2[] JointPixels =
        {
            new Vector2(125, 190), new Vector2(155, 338), new Vector2(192, 338),
            new Vector2(230, 338), new Vector2(260, 190), new Vector2(192, 178)
        };

        private readonly Transform[] bones = new Transform[6];
        private readonly SpriteRenderer[] renderers = new SpriteRenderer[6];
        private readonly Sprite[,] sprites = new Sprite[8, 6];
        private readonly Vector3[] rollStartPositions = new Vector3[6];
        private readonly Vector3[] rollStartScales = new Vector3[6];
        private readonly Quaternion[] rollStartRotations = new Quaternion[6];

        private Facing facing = Facing.South;
        private Vector2 externalMotion;
        private bool useExternalMotion;
        private Vector3 restScale;
        private Vector3 restPosition;
        private Quaternion restRotation;
        [NonSerialized] private bool initialized;
        [NonSerialized] private bool built;
        [NonSerialized] private bool spritesCached;
        private float walkPhase;
        private float walkWeight;
        private float idleTime;
        private float gaitSpeed = 1f;
        [NonSerialized] private bool rolling;
        private float rollStartedAt;
        private float activeRollDuration;
        private float rollSpinSign;

        public Vector2 Motion { get; private set; }
        public bool IsRolling => rolling && Time.time < rollStartedAt + activeRollDuration;
        public float RollDuration => rolling ? activeRollDuration : Mathf.Max(0.1f, rollDuration);
        public float RollProgress => rolling ? Mathf.Clamp01((Time.time - rollStartedAt) / activeRollDuration) : 0f;
        public event Action RollFinished;

        private void Awake() => InitializeRig();

        private void InitializeRig()
        {
            if (initialized) return;
            restScale = transform.localScale;
            restPosition = transform.localPosition;
            restRotation = transform.localRotation;
            initialized = true;

            if (GetComponent<Rigidbody>() != null)
            {
                Debug.LogError("Place PirateEightDirectionRig on the visual child, not the Rigidbody root.", this);
                enabled = false;
                return;
            }

            if (motionSource == null) motionSource = GetComponentInParent<Rigidbody>();
            if (navMeshMotionSource == null) navMeshMotionSource = GetComponentInParent<NavMeshAgent>();
            BuildIfNeeded();
            SetFacing(initialFacing);
            ApplyWalkPose(0f, 0f);
        }

        private void OnEnable()
        {
            InitializeRig();
            if (!built) return;
            CancelRoll();
        }

        // Vector2의 x/y는 월드 속도의 x/z를 의미합니다.
        public void SetMotion(Vector2 motion)
        {
            externalMotion = motion;
            useExternalMotion = true;
        }

        public void UseRigidbodyMotion() => useExternalMotion = false;

        public bool PlayRoll(Vector2 direction)
        {
            if (!isActiveAndEnabled || rolling || !built) return false;
            if (direction.sqrMagnitude < 0.0001f) direction = FacingToVector(facing);
            direction.Normalize();
            SetFacing(direction);

            for (int i = 0; i < bones.Length; i++)
            {
                rollStartPositions[i] = bones[i].localPosition;
                rollStartScales[i] = bones[i].localScale;
                rollStartRotations[i] = bones[i].localRotation;
            }

            // 부모 X 회전이 90도여도, 시각 리그는 로컬 Z축으로 회전합니다. 음수 스케일은 사용하지 않습니다.
            rollSpinSign = Mathf.Abs(direction.x) > 0.1f ? -Mathf.Sign(direction.x) : (direction.y >= 0f ? -1f : 1f);
            activeRollDuration = Mathf.Max(0.1f, rollDuration);
            rollStartedAt = Time.time;
            walkPhase = 0f;
            walkWeight = 0f;
            rolling = true;
            return true;
        }

        // Mover에서 호출: 구르기 애니메이션과 같은 시간 기준으로 이동 속도를 조절합니다.
        public float GetRollSpeedMultiplier()
        {
            if (!IsRolling) return 0f;
            float t = RollProgress;
            if (t < 0.12f) return Mathf.Lerp(0.30f, 1.35f, Smooth01(t / 0.12f));
            if (t < 0.65f) return Mathf.Lerp(1.35f, 1f, Smooth01((t - 0.12f) / 0.53f));
            return 1f - Smooth01((t - 0.65f) / 0.35f);
        }

        public void CancelRoll()
        {
            rolling = false;
            walkWeight = 0f;
            walkPhase = 0f;
            gaitSpeed = 1f;
            if (!initialized) return;
            RestoreVisualTransform();
            if (built) ApplyWalkPose(0f, 0f);
        }

        private void LateUpdate()
        {
            if (!built) return;
            idleTime += Time.deltaTime;
            Motion = ReadMotion();

            if (rolling)
            {
                UpdateRoll();
                return;
            }

            float speed = Motion.magnitude;
            bool walking = speed > walkingThreshold;
            if (walking) SetFacing(Motion);
            float targetWeight = walking ? Mathf.Clamp01(speed / Mathf.Max(referenceWalkSpeed, 0.1f)) : 0f;
            walkWeight = Mathf.MoveTowards(walkWeight, targetWeight, Time.deltaTime / Mathf.Max(walkBlendTime, 0.02f));
            float targetGait = Mathf.Clamp(speed / Mathf.Max(referenceWalkSpeed, 0.1f), 0.65f, 1.5f);
            gaitSpeed = Mathf.Lerp(gaitSpeed, targetGait, 1f - Mathf.Exp(-12f * Time.deltaTime));

            if (walkWeight > 0.0001f) walkPhase = Mathf.Repeat(walkPhase + Time.deltaTime * walkCyclesPerSecond * gaitSpeed * Mathf.PI * 2f, Mathf.PI * 2f);
            else
                walkPhase = 0f;

            ApplyWalkPose(walkPhase, walkWeight);
        }

        private Vector2 ReadMotion()
        {
            if (useExternalMotion) return externalMotion;
            if (navMeshMotionSource != null && navMeshMotionSource.enabled && navMeshMotionSource.isOnNavMesh) return new Vector2(navMeshMotionSource.velocity.x, navMeshMotionSource.velocity.z);
            if (motionSource != null) return new Vector2(motionSource.velocity.x, motionSource.velocity.z);
            return Vector2.zero;
        }

        private Vector2 RestArmAngles()
        {
            Vector2 extra = directionArmOffsets != null && directionArmOffsets.Length > (int)facing ? directionArmOffsets[(int)facing] : Vector2.zero;
            return new Vector2(backArmRestAngle, frontArmRestAngle) + extra;
        }

        private void ApplyWalkPose(float phase, float weight)
        {
            Vector2 direction = FacingToVector(facing);
            Vector2 arms = RestArmAngles();
            float side = Mathf.Abs(direction.x);
            float wave = Mathf.Sin(phase);
            float stride = Mathf.Cos(phase) * strideLength * weight;
            float backLift = Mathf.Max(0f, wave) * weight;
            float frontLift = Mathf.Max(0f, -wave) * weight;
            float breathe = Mathf.Sin(idleTime * 2.2f) * idleBreathing * (1f - weight);
            float bob = (1f - Mathf.Cos(phase * 2f)) * 0.5f * bodyBob * weight;
            float lean = -direction.x * forwardLeanDegrees * weight;
            float armSwing = wave * armSwingDegrees * weight * Mathf.Lerp(0.45f, 1f, side);
            float legSwing = wave * legSwingDegrees * weight * side;
            Vector3 chestOffset = new Vector3(direction.x * weight * 0.01f, bob + breathe * 0.3f, 0f);

            SetPart(0, chestOffset, arms.x + armSwing + lean, Vector3.one);
            SetPart(4, chestOffset, arms.y - armSwing + lean, Vector3.one);
            SetPart(1, new Vector3(-direction.x * stride, backLift * footLift - direction.y * stride * 0.35f, 0f), -legSwing, new Vector3(1f, 1f - backLift * 0.035f, 1f));
            SetPart(3, new Vector3(direction.x * stride, frontLift * footLift + direction.y * stride * 0.35f, 0f), legSwing, new Vector3(1f, 1f - frontLift * 0.035f, 1f));
            SetPart(2, chestOffset, lean * 0.6f, new Vector3(1f - breathe * 0.25f, 1f + breathe, 1f));
            SetPart(5, chestOffset + Vector3.up * (bob * 0.15f), lean * 0.3f - wave * 0.35f * weight, Vector3.one);
        }

        private void UpdateRoll()
        {
            float t = RollProgress;
            if (t >= 1f)
            {
                CancelRoll();
                RollFinished?.Invoke();
                return;
            }

            float tuckIn = Smooth01(t / 0.18f);
            float recover = Smooth01((t - 0.74f) / 0.26f);
            float tuck = tuckIn * (1f - recover) * rollTuckStrength;
            float spinT = Mathf.Clamp01((t - 0.12f) / 0.66f);
            float spin = Smooth01(spinT) * 360f * rollSpinSign;
            float prepare = Pulse(t, 0f, 0.18f);
            float landing = Pulse(t, 0.78f, 1f);
            float squash = prepare * 0.12f + landing * 0.16f;
            float hop = Mathf.Sin(spinT * Mathf.PI) * rollHopHeight;

            // 편안한 복귀 자세를 만든 뒤, 시작 자세와 웅크린 자세 사이를 보간합니다.
            ApplyWalkPose(0f, 0f);
            Vector2 arms = RestArmAngles();
            BlendRollPart(0, new Vector3(0.055f, -0.06f, 0f), arms.x + 68f, new Vector3(1f, 0.88f, 1f), tuckIn, recover);
            BlendRollPart(4, new Vector3(-0.055f, -0.06f, 0f), arms.y - 68f, new Vector3(1f, 0.88f, 1f), tuckIn, recover);
            BlendRollPart(1, new Vector3(0.09f, 0.22f, 0f), 22f, new Vector3(1f, 0.62f, 1f), tuckIn, recover);
            BlendRollPart(3, new Vector3(-0.09f, 0.22f, 0f), -22f, new Vector3(1f, 0.62f, 1f), tuckIn, recover);
            BlendRollPart(2, new Vector3(0f, 0.02f, 0f), 0f, new Vector3(1.06f, 0.88f, 1f), tuckIn, recover);
            BlendRollPart(5, new Vector3(0f, -0.34f, 0f), 8f * rollSpinSign, new Vector3(0.96f, 0.96f, 1f), tuckIn, recover);

            Quaternion rotation = Quaternion.Euler(0f, 0f, spin);
            Vector3 scale = new Vector3(1f + tuck * 0.08f + squash, 1f - tuck * 0.24f - squash, 1f);
            Vector3 pivot = new Vector3(rollPivot.x, rollPivot.y, 0f);
            Vector3 newScale = Vector3.Scale(restScale, scale);
            // 물리 루트는 회전시키지 않고, 시각 리그만 몸 중심으로 회전시킵니다.
            Vector3 pivotCorrection = Vector3.Scale(restScale, pivot) - rotation * Vector3.Scale(newScale, pivot);
            transform.localRotation = restRotation * rotation;
            transform.localScale = newScale;
            transform.localPosition = restPosition + restRotation * (pivotCorrection + Vector3.Scale(restScale, Vector3.up * hop));
        }

        private void BlendRollPart(int index, Vector3 offset, float angle, Vector3 scale, float tuckIn, float recover)
        {
            Transform bone = bones[index];
            Vector3 relaxedPosition = bone.localPosition;
            Quaternion relaxedRotation = bone.localRotation;
            Vector3 relaxedScale = bone.localScale;
            Vector3 targetPosition = Vector3.Lerp(relaxedPosition, PixelToLocal(JointPixels[index]) + offset, rollTuckStrength);
            Quaternion targetRotation = Quaternion.Slerp(relaxedRotation, Quaternion.Euler(0f, 0f, angle), rollTuckStrength);
            Vector3 targetScale = Vector3.Lerp(relaxedScale, scale, rollTuckStrength);

            bone.localPosition = Vector3.Lerp(Vector3.Lerp(rollStartPositions[index], targetPosition, tuckIn), relaxedPosition, recover);
            bone.localRotation = Quaternion.Slerp(Quaternion.Slerp(rollStartRotations[index], targetRotation, tuckIn), relaxedRotation, recover);
            bone.localScale = Vector3.Lerp(Vector3.Lerp(rollStartScales[index], targetScale, tuckIn), relaxedScale, recover);
        }

        private void SetPart(int index, Vector3 offset, float angle, Vector3 scale)
        {
            bones[index].localPosition = PixelToLocal(JointPixels[index]) + offset;
            bones[index].localRotation = Quaternion.Euler(0f, 0f, angle);
            bones[index].localScale = scale;
        }

        private void RestoreVisualTransform()
        {
            transform.localPosition = restPosition;
            transform.localRotation = restRotation;
            transform.localScale = restScale;
        }

        private void SetFacing(Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.0001f) return;
            int octant = Mathf.RoundToInt(Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg / 45f);
            // Enum order: S=0, SW=1, W=2, NW=3, N=4, NE=5, E=6, SE=7.
            Facing next = (Facing)((6 - octant + 8) % 8);
            if (next == facing && renderers[0].sprite != null) return;
            facing = next;
            ApplySprites();
        }

        private static Vector2 FacingToVector(Facing value)
        {
            float angle = (270f - (int)value * 45f) * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        private void BuildIfNeeded()
        {
            if (built) return;
            CacheSprites();
            for (int i = 0; i < PartNames.Length; i++)
            {
                Transform bone = transform.Find(PartNames[i] + " Bone");
                if (bone == null)
                {
                    bone = new GameObject(PartNames[i] + " Bone").transform;
                    bone.SetParent(transform, false);
                }
                bones[i] = bone;
                Transform art = bone.Find(PartNames[i] + " Art");
                if (art == null)
                {
                    art = new GameObject(PartNames[i] + " Art").transform;
                    art.SetParent(bone, false);
                }
                art.localPosition = -PixelToLocal(JointPixels[i]);
                art.localRotation = Quaternion.identity;
                art.localScale = Vector3.one;
                SpriteRenderer renderer = art.GetComponent<SpriteRenderer>();
                if (renderer == null) renderer = art.gameObject.AddComponent<SpriteRenderer>();
                renderer.sortingLayerName = sortingLayerName;
                renderer.sortingOrder = sortingOrder + i;
                renderer.color = tint;
                renderers[i] = renderer;
            }
            built = true;
            ApplySprites();
        }

        private void CacheSprites()
        {
            if (spritesCached) return;
            int missing = 0;
            string firstMissing = null;
            for (int d = 0; d < DirectionNames.Length; d++)
            for (int p = 0; p < PartNames.Length; p++)
            {
                string path = $"PirateRig/{DirectionNames[d]}-{PartNames[p]}";
                sprites[d, p] = Resources.Load<Sprite>(path);
                if (sprites[d, p] != null) continue;
                missing++;
                if (firstMissing == null) firstMissing = path;
            }
            spritesCached = true;
            if (missing > 0) Debug.LogWarning($"Pirate rig: {missing} sprite(s) missing. First: Resources/{firstMissing}. Check Sprite import and resource names.", this);
        }

        private void ApplySprites()
        {
            for (int i = 0; i < PartNames.Length; i++)
                renderers[i].sprite = sprites[(int)facing, i];
        }

        private static Vector3 PixelToLocal(Vector2 pixel)
        {
            // 기존 이미지의 캔버스 좌표와 PPU 100을 유지합니다.
            return new Vector3((pixel.x - 192f) / 100f, (256f - pixel.y) / 100f, 0f);
        }

        private static float Smooth01(float value)
        {
            float t = Mathf.Clamp01(value);
            return t * t * (3f - 2f * t);
        }

        private static float Pulse(float t, float start, float end)
        {
            if (t <= start || t >= end) return 0f;
            float wave = Mathf.Sin((t - start) / (end - start) * Mathf.PI);
            return wave * wave;
        }

        [ContextMenu("Apply XZ Motion Preset")]
        private void ApplyXZMotionPreset()
        {
            walkingThreshold = 0.08f;
            referenceWalkSpeed = 2.5f;
            walkCyclesPerSecond = 2.1f;
            walkBlendTime = 0.10f;
            backArmRestAngle = 22f;
            frontArmRestAngle = -22f;
            directionArmOffsets = new Vector2[8];
            armSwingDegrees = 4f;
            legSwingDegrees = 7f;
            footLift = 0.055f;
            strideLength = 0.055f;
            bodyBob = 0.009f;
            forwardLeanDegrees = 2.5f;
            idleBreathing = 0.008f;
            rollDuration = 0.32f;
            rollHopHeight = 0.08f;
            rollTuckStrength = 0.8f;
            rollPivot = new Vector2(0f, -0.15f);
        }

        private void OnDisable()
        {
            CancelRoll();
            externalMotion = Vector2.zero;
            Motion = Vector2.zero;
        }
    }
}
