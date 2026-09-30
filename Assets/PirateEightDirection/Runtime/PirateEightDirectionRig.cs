using System;
using UnityEngine;
using UnityEngine.AI;

namespace PirateEightDirection
{
    [DisallowMultipleComponent]
    public sealed class PirateEightDirectionRig : MonoBehaviour
    {
        private enum Facing { South, SouthWest, West, NorthWest, North, NorthEast, East, SouthEast }

        [Header("Motion")]
        [SerializeField] private Rigidbody2D motionSource;
        [SerializeField] private NavMeshAgent navMeshMotionSource;
        [SerializeField, Min(0.01f)] private float walkingThreshold = 0.08f;
        [SerializeField, Min(0.1f)] private float walkCyclesPerSecond = 1.8f;
        [SerializeField, Range(0f, 20f)] private float limbSwingDegrees = 11f;
        [SerializeField, Range(0f, 0.1f)] private float idleBreathing = 0.025f;
        [SerializeField] private Vector2 initialFacing = Vector2.down;

        [Header("Roll")]
        [SerializeField, Min(0.1f)] private float rollDuration = 0.3f;
        [SerializeField, Range(0f, 0.5f)] private float rollHopHeight = 0.12f;

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
        private Facing facing = Facing.South;
        private Vector2 externalMotion;
        private bool useExternalMotion;
        private Vector3 restScale;
        private Vector3 restPosition;
        private Quaternion restRotation;
        private bool rolling;
        private float rollTime;
        private float rollSpinSign;
        private bool rollTumble;
        // Play 중 재컴파일 시 readonly 배열은 비워지지만 private 필드는 복원되므로 직렬화에서 제외한다.
        [NonSerialized] private bool built;

        public Vector2 Motion { get; private set; }
        public bool IsRolling => rolling;
        public float RollDuration => rollDuration;
        public event Action RollFinished;

        private void Awake()
        {
            restScale = transform.localScale;
            restPosition = transform.localPosition;
            restRotation = transform.localRotation;
            if (motionSource == null) motionSource = GetComponentInParent<Rigidbody2D>();
            if (navMeshMotionSource == null) navMeshMotionSource = GetComponentInParent<NavMeshAgent>();
            BuildIfNeeded();
            SetFacing(initialFacing);
        }

        private void OnEnable()
        {
            BuildIfNeeded();
        }

        public void SetMotion(Vector2 motion)
        {
            externalMotion = motion;
            useExternalMotion = true;
        }

        public void UseRigidbodyMotion()
        {
            useExternalMotion = false;
        }

        /// <summary>direction 방향으로 구르기를 시작한다. 이미 구르는 중이면 false.</summary>
        public bool PlayRoll(Vector2 direction)
        {
            if (rolling) return false;
            BuildIfNeeded();
            if (direction.sqrMagnitude < 0.0001f) direction = FacingToVector(facing);
            direction.Normalize();
            SetFacing(direction);

            // 좌우가 섞인 방향은 화면 평면에서 회전, 순수 상하 방향은 앞뒤로 텀블링하는 것처럼 보이게 한다.
            rollTumble = Mathf.Abs(direction.x) < 0.3f;
            rollSpinSign = direction.x > 0.01f ? -1f : direction.x < -0.01f ? 1f : -1f;
            rollTime = 0f;
            rolling = true;
            return true;
        }

        private void LateUpdate()
        {
            BuildIfNeeded();
            if (useExternalMotion)
                Motion = externalMotion;
            else if (motionSource != null)
                Motion = motionSource.velocity;
            else if (navMeshMotionSource != null && navMeshMotionSource.enabled && navMeshMotionSource.isOnNavMesh)
                Motion = new Vector2(navMeshMotionSource.velocity.x, navMeshMotionSource.velocity.z);
            else
                Motion = Vector2.zero;
            if (rolling)
            {
                UpdateRoll();
                return;
            }
            if (Motion.sqrMagnitude > walkingThreshold * walkingThreshold) SetFacing(Motion);

            float speed = Motion.magnitude;
            bool walking = speed > walkingThreshold;
            float phase = Time.time * walkCyclesPerSecond * Mathf.PI * 2f;
            float wave = Mathf.Sin(phase);
            float swing = walking ? wave * limbSwingDegrees : 0f;
            float breathe = walking ? Mathf.Abs(wave) * 0.018f : Mathf.Sin(Time.time * 2.2f) * idleBreathing;

            bones[0].localRotation = Quaternion.Euler(0, 0, swing);
            bones[1].localRotation = Quaternion.Euler(0, 0, -swing * 0.65f);
            bones[2].localScale = new Vector3(1f - breathe * 0.35f, 1f + breathe, 1f);
            bones[2].localPosition = PixelToLocal(JointPixels[2]) + Vector3.up * (walking ? Mathf.Abs(wave) * 0.025f : breathe * 0.35f);
            bones[3].localRotation = Quaternion.Euler(0, 0, swing * 0.65f);
            bones[4].localRotation = Quaternion.Euler(0, 0, -swing);
            bones[5].localRotation = Quaternion.Euler(0, 0, walking ? -wave * 1.5f : Mathf.Sin(Time.time * 1.4f) * 0.7f);
        }

        private void UpdateRoll()
        {
            rollTime += Time.deltaTime;
            float t = Mathf.Clamp01(rollTime / rollDuration);

            // 몸을 웅크리는 정도: 앞 25% 동안 접고, 마지막 25% 동안 편다.
            float tuck = Mathf.SmoothStep(0f, 1f, t / 0.25f) * Mathf.SmoothStep(0f, 1f, (1f - t) / 0.25f);
            // 도약 직전과 착지 직후에 짧게 찌그러진다.
            float squash = Pulse(t, 0.06f, 0.06f) + Pulse(t, 0.95f, 0.06f);
            float spinT = Mathf.Clamp01((t - 0.1f) / 0.8f);
            float spinT01 = spinT * spinT * (3f - 2f * spinT);
            float spinDegrees = spinT01 * 360f;
            float hop = Mathf.Sin(spinT * Mathf.PI) * rollHopHeight;

            var scale = new Vector3(1f + squash * 0.12f - tuck * 0.06f, 1f - squash * 0.2f - tuck * 0.1f, 1f);
            if (rollTumble)
            {
                float flip = Mathf.Cos(spinDegrees * Mathf.Deg2Rad);
                scale.y *= Mathf.Sign(flip) * Mathf.Max(Mathf.Abs(flip), 0.2f);
                transform.localRotation = restRotation;
            }
            else
            {
                transform.localRotation = restRotation * Quaternion.Euler(0f, 0f, spinDegrees * rollSpinSign);
            }
            transform.localScale = Vector3.Scale(restScale, scale);
            transform.localPosition = restPosition + Vector3.up * hop;

            bones[0].localRotation = Quaternion.Euler(0f, 0f, tuck * 70f);
            bones[4].localRotation = Quaternion.Euler(0f, 0f, -tuck * 70f);
            bones[1].localRotation = Quaternion.Euler(0f, 0f, tuck * 25f);
            bones[3].localRotation = Quaternion.Euler(0f, 0f, -tuck * 25f);
            bones[1].localScale = new Vector3(1f, 1f - tuck * 0.35f, 1f);
            bones[3].localScale = new Vector3(1f, 1f - tuck * 0.35f, 1f);
            bones[2].localScale = new Vector3(1f + tuck * 0.05f, 1f - tuck * 0.1f, 1f);
            bones[2].localPosition = PixelToLocal(JointPixels[2]);
            bones[5].localPosition = PixelToLocal(JointPixels[5]) + Vector3.down * (tuck * 0.25f);
            bones[5].localRotation = Quaternion.Euler(0f, 0f, tuck * 12f * rollSpinSign);

            if (t < 1f) return;
            rolling = false;
            ResetPose();
            RollFinished?.Invoke();
        }

        private static float Pulse(float t, float center, float width)
        {
            float x = (t - center) / width;
            return Mathf.Exp(-x * x);
        }

        private static Vector2 FacingToVector(Facing value)
        {
            float angle = value switch
            {
                Facing.East => 0f,
                Facing.NorthEast => 45f,
                Facing.North => 90f,
                Facing.NorthWest => 135f,
                Facing.West => 180f,
                Facing.SouthWest => 225f,
                Facing.SouthEast => 315f,
                _ => 270f
            };
            return new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        }

        private void ResetPose()
        {
            transform.localScale = restScale;
            transform.localPosition = restPosition;
            transform.localRotation = restRotation;
            if (!built) return;
            for (int i = 0; i < bones.Length; i++)
            {
                if (bones[i] == null) continue;
                bones[i].localRotation = Quaternion.identity;
                bones[i].localScale = Vector3.one;
                bones[i].localPosition = PixelToLocal(JointPixels[i]);
            }
        }

        private void SetFacing(Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.0001f) return;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            int octant = Mathf.RoundToInt(angle / 45f);
            Facing next;
            switch (octant)
            {
                case -2: next = Facing.South; break;
                case -3: next = Facing.SouthWest; break;
                case 4:
                case -4: next = Facing.West; break;
                case 3: next = Facing.NorthWest; break;
                case 2: next = Facing.North; break;
                case 1: next = Facing.NorthEast; break;
                case 0: next = Facing.East; break;
                case -1: next = Facing.SouthEast; break;
                default: next = Facing.South; break;
            }
            if (next == facing && renderers[0].sprite != null) return;
            facing = next;
            ApplySprites();
        }

        private void BuildIfNeeded()
        {
            if (built && bones[0] != null && renderers[0] != null) return;
            for (int i = 0; i < PartNames.Length; i++)
            {
                // 재컴파일 후에는 이전에 만든 자식이 남아 있으므로 새로 만들지 않고 재사용한다.
                Transform bone = transform.Find(PartNames[i] + " Bone");
                if (bone == null)
                {
                    bone = new GameObject(PartNames[i] + " Bone").transform;
                    bone.SetParent(transform, false);
                }
                bone.localPosition = PixelToLocal(JointPixels[i]);
                bones[i] = bone;

                Transform art = bone.Find(PartNames[i] + " Art");
                if (art == null)
                {
                    art = new GameObject(PartNames[i] + " Art").transform;
                    art.SetParent(bone, false);
                }
                art.localPosition = -PixelToLocal(JointPixels[i]);
                var spriteRenderer = art.GetComponent<SpriteRenderer>();
                if (spriteRenderer == null) spriteRenderer = art.gameObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sortingLayerName = sortingLayerName;
                spriteRenderer.sortingOrder = sortingOrder + i;
                spriteRenderer.color = tint;
                renderers[i] = spriteRenderer;
            }
            built = true;
            ApplySprites();
        }

        private void ApplySprites()
        {
            string direction = DirectionNames[(int)facing];
            for (int i = 0; i < PartNames.Length; i++)
                renderers[i].sprite = Resources.Load<Sprite>($"PirateRig/{direction}-{PartNames[i]}");
        }

        private static Vector3 PixelToLocal(Vector2 pixel)
        {
            const float pixelsPerUnit = 100f;
            return new Vector3((pixel.x - 192f) / pixelsPerUnit, (256f - pixel.y) / pixelsPerUnit, 0f);
        }

        private void OnDisable()
        {
            rolling = false;
            ResetPose();
        }
    }
}
