using UnityEngine;

namespace PirateEightDirection
{
    [RequireComponent(typeof(Rigidbody))]
    [DisallowMultipleComponent]
    public sealed class PirateRigTestMover : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float speed = 2.5f;
        [Header("Roll")]
        [SerializeField, Min(0.1f)] private float rollSpeed = 15f;
        [SerializeField, Min(0f)] private float rollCooldown = 0.25f;
        [Header("Presentation")]
        [SerializeField] private bool interpolateMovement = true;
        [SerializeField] private bool showControls = true;

        private Rigidbody body;
        private PirateEightDirectionRig rig;
        private Vector3 input;
        private Vector3 lastDirection = Vector3.back;
        private Vector3 rollDirection;
        private float nextRollTime;
        private bool RigActive => rig != null && rig.isActiveAndEnabled;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            rig = GetComponentInChildren<PirateEightDirectionRig>();
            if (interpolateMovement) body.interpolation = RigidbodyInterpolation.Interpolate;
            if (rig == null) Debug.LogWarning("PirateRigTestMover: add PirateEightDirectionRig to the visual child.", this);
        }

        private void Update()
        {
            float horizontal = 0f;
            float vertical = 0f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) horizontal -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontal += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) vertical -= 1f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) vertical += 1f;
            input = new Vector3(horizontal, 0f, vertical).normalized;
            if (input.sqrMagnitude > 0f) lastDirection = input;

            if (!Input.GetKeyDown(KeyCode.Space) || !RigActive || rig.IsRolling || Time.time < nextRollTime) return;
            rollDirection = lastDirection;
            if (rig.PlayRoll(new Vector2(rollDirection.x, rollDirection.z))) nextRollTime = Time.time + rig.RollDuration + rollCooldown;
        }

        private void FixedUpdate()
        {
            Vector3 velocity = RigActive && rig.IsRolling ? rollDirection * (rollSpeed * rig.GetRollSpeedMultiplier()) : input * speed;
            body.velocity = new Vector3(velocity.x, body.velocity.y, velocity.z);
        }

        private void OnDisable()
        {
            input = Vector3.zero;
            if (body != null) body.velocity = Vector3.zero;
            if (rig != null) rig.CancelRoll();
        }

        [ContextMenu("Apply XZ Motion Preset")]
        private void ApplyXZMotionPreset()
        {
            speed = 2.5f;
            rollSpeed = 15f;
            rollCooldown = 0.25f;
            interpolateMovement = true;
        }

        private void OnGUI()
        {
            if (!showControls) return;
            GUI.Box(new Rect(16, 16, 340, 100), "Pirate Eight Direction Test");
            GUI.Label(new Rect(32, 43, 310, 24), "Move: WASD or Arrow Keys");
            GUI.Label(new Rect(32, 63, 310, 24), "Roll: Space (toward move direction)");
            GUI.Label(new Rect(32, 83, 310, 24), "Release keys to preview relaxed idle");
        }
    }
}
