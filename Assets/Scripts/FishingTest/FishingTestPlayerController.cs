using UnityEngine;
using UnityEngine.AI;

namespace FishingTest
{
    [DisallowMultipleComponent]
    public sealed class FishingTestPlayerController : MonoBehaviour
    {
        [Header("Character Visual")]
        [SerializeField] private SpriteRenderer[] facingRenderers;

        private Character character;
        private PlayerMove playerMove;
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

                if (value) AcquireLock();
                else ReleaseLock();
            }
        }

        public bool AutoMovingVisual { get; set; }
        public bool HasMoveInput => ReadMoveInput().sqrMagnitude > 0.01f;

        private void Awake()
        {
            character = GetComponent<Character>();
            playerMove = GetComponent<PlayerMove>();
            agent = GetComponent<NavMeshAgent>();
            animator = character != null ? character.anim : GetComponentInChildren<Animator>(true);

            if (facingRenderers == null || facingRenderers.Length == 0)
                facingRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        }

        public void Move(Vector3 displacement)
        {
            transform.position += displacement;
        }

        private void LateUpdate()
        {
            if (animator == null || !MovementLocked) return;

            animator.SetBool("isRun", AutoMovingVisual);

            if (AutoMovingVisual)
                animator.SetFloat("moveSpeed", character != null ? character.MovementAnimationSpeed : 1f);
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
            {
                if (renderer != null) renderer.flipX = faceLeft;
            }
        }

        private void AcquireLock()
        {
            if (ownsLock) return;

            if (playerMove != null) playerMove.MovementLocked = true;

            if (character != null)
            {
                previousControl = character.isCanControll;
                previousFlip = character.canFlip;

                character.isCanControll = false;
                character.canFlip = false;
                character.SetExternalMovementVisualControl(true);
            }

            previousAgentEnabled = agent != null && agent.enabled;

            if (agent != null && agent.enabled) agent.enabled = false;

            ownsLock = true;
        }

        private void ReleaseLock()
        {
            if (!ownsLock) return;

            if (animator != null) animator.SetBool("isRun", false);

            if (character != null)
            {
                character.SetExternalMovementVisualControl(false);
                character.isCanControll = previousControl;
                character.canFlip = previousFlip;
            }

            if (agent != null) agent.enabled = previousAgentEnabled;
            if (playerMove != null) playerMove.MovementLocked = false;

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

        private void OnDisable()
        {
            movementLocked = false;
            AutoMovingVisual = false;
            ReleaseLock();
        }
    }
}