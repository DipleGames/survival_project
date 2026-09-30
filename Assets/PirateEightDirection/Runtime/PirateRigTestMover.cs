using UnityEngine;

namespace PirateEightDirection
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PirateRigTestMover : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float speed = 2.5f;

        [Header("Roll")]
        [SerializeField, Min(0.1f)] private float rollSpeed = 15f;
        [SerializeField, Min(0f)] private float rollCooldown = 0.25f;

        private Rigidbody2D body;
        private PirateEightDirectionRig rig;
        private Vector2 input;
        private Vector2 lastDirection = Vector2.down;
        private Vector2 rollDirection;
        private float nextRollTime;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            rig = GetComponentInChildren<PirateEightDirectionRig>();
        }

        private void Update()
        {
            float horizontal = 0f;
            float vertical = 0f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) horizontal -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontal += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) vertical -= 1f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) vertical += 1f;
            input = new Vector2(horizontal, vertical).normalized;
            if (input != Vector2.zero) lastDirection = input;

            if (Input.GetKeyDown(KeyCode.Space) && rig != null && !rig.IsRolling && Time.time >= nextRollTime)
            {
                rollDirection = lastDirection;
                if (rig.PlayRoll(rollDirection))
                    nextRollTime = Time.time + rig.RollDuration + rollCooldown;
            }
        }

        private void FixedUpdate()
        {
            body.velocity = rig != null && rig.IsRolling ? rollDirection * rollSpeed : input * speed;
        }

        private void OnDisable()
        {
            input = Vector2.zero;
            if (body != null) body.velocity = Vector2.zero;
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(16, 16, 330, 90), "Pirate Eight Direction Test");
            GUI.Label(new Rect(32, 43, 300, 24), "Move: WASD or Arrow Keys");
            GUI.Label(new Rect(32, 63, 300, 24), "Roll: Space (toward move direction)");
            GUI.Label(new Rect(32, 83, 300, 24), "Release keys to preview idle breathing");
        }
    }
}
