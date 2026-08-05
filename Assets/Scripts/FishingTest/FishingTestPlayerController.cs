using UnityEngine;
using UnityEngine.AI;

namespace FishingTest
{
    [DisallowMultipleComponent]
    public sealed class FishingTestPlayerController : MonoBehaviour
    {
        [Header("Test scene fallback")]
        [SerializeField, Min(0.1f)] private float moveSpeed = 3f;
        [Header("Character visual")]
        [SerializeField] private SpriteRenderer[] facingRenderers;

        private Character character;
        private NavMeshAgent agent;
        private Animator animator;
        private bool movementLocked;
        private bool ownsLock;
        private bool previousControl;
        private bool previousFlip;
        private bool previousAgentEnabled;

        public bool MovementLocked
        {
            get => movementLocked;
            set
            {
                if (movementLocked == value) return;
                movementLocked = value;
                if (value) AcquireLock(); else ReleaseLock();
            }
        }

        public bool AutoMovingVisual { get; set; }
        public Vector2 MoveInput { get; private set; }
        public bool HasMoveInput => MoveInput.sqrMagnitude > 0.01f;

        private void Awake()
        {
            character = GetComponent<Character>();
            agent = GetComponent<NavMeshAgent>();
            animator = character != null ? character.anim : GetComponentInChildren<Animator>(true);
            if (character == null && agent != null)
            {
                agent.speed = moveSpeed;
                agent.enabled = true;
            }
            if (facingRenderers == null || facingRenderers.Length == 0)
                facingRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        }

        private void Update()
        {
            MoveInput = ReadMoveInput();
            if (character == null && !MovementLocked && HasMoveInput)
            {
                SetFacingLeft(MoveInput.x > 0f);
                Move(new Vector3(MoveInput.x, 0f, MoveInput.y) * moveSpeed * Time.deltaTime);
            }
        }

        public void Move(Vector3 displacement)
        {
            if (agent != null && agent.enabled && agent.isOnNavMesh)
                agent.Move(displacement);
            else
                transform.position += displacement;
        }

        private void LateUpdate()
        {
            if (animator == null) return;

            // Character owns its normal movement animation. Only write animator
            // parameters while this component is actually controlling movement.
            if (character != null && !MovementLocked) return;

            bool moving = AutoMovingVisual || (character == null && !MovementLocked && HasMoveInput);
            animator.SetBool("isRun", moving);
            if (moving)
                animator.SetFloat("moveSpeed", character != null ? character.MovementAnimationSpeed : 1f);
        }

        private void OnDisable()
        {
            movementLocked = false;
            AutoMovingVisual = false;
            ReleaseLock();
        }

        public void SetFacingLeft(bool faceLeft)
        {
            if (character != null)
            {
                character.SetFacingLeft(faceLeft);
                return;
            }

            if (facingRenderers == null) return;
            foreach (SpriteRenderer renderer in facingRenderers)
                if (renderer != null) renderer.flipX = faceLeft;
        }

        private void AcquireLock()
        {
            if (character == null || ownsLock) return;
            previousControl = character.isCanControll;
            previousFlip = character.canFlip;
            previousAgentEnabled = agent != null && agent.enabled;
            character.isCanControll = false;
            character.canFlip = false;
            character.SetExternalMovementVisualControl(true);
            if (agent != null && agent.enabled) agent.enabled = false;
            ownsLock = true;
        }

        private void ReleaseLock()
        {
            if (!ownsLock) return;
            if (animator != null) animator.SetBool("isRun", false);
            character.SetExternalMovementVisualControl(false);
            character.isCanControll = previousControl;
            character.canFlip = previousFlip;
            if (agent != null) agent.enabled = previousAgentEnabled;
            ownsLock = false;
        }

        private static Vector2 ReadMoveInput()
        {
            KeyCode left = (KeyCode)PlayerPrefs.GetInt("Key_Left");
            KeyCode right = (KeyCode)PlayerPrefs.GetInt("Key_Right");
            KeyCode down = (KeyCode)PlayerPrefs.GetInt("Key_Down");
            KeyCode up = (KeyCode)PlayerPrefs.GetInt("Key_Up");
            if (left == KeyCode.None && right == KeyCode.None && down == KeyCode.None && up == KeyCode.None)
                return Vector2.ClampMagnitude(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")), 1f);

            float x = (Input.GetKey(right) ? 1f : 0f) - (Input.GetKey(left) ? 1f : 0f);
            float y = (Input.GetKey(up) ? 1f : 0f) - (Input.GetKey(down) ? 1f : 0f);
            return Vector2.ClampMagnitude(new Vector2(x, y), 1f);
        }
    }
}
