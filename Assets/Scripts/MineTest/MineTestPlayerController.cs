using UnityEngine;

namespace MineTest
{
    [DisallowMultipleComponent]
    public sealed class MineTestPlayerController : MonoBehaviour
    {
        private const string MiningStateName = "Logging";

        [SerializeField, Min(0.1f)] private float moveSpeed = 3f;
        [SerializeField] private SpriteRenderer[] facingRenderers;
        [SerializeField] private RuntimeAnimatorController miningAnimatorController;

        private Character character;
        private Animator animator;
        private RuntimeAnimatorController defaultAnimatorController;
        private bool movementLocked;
        private bool ownsCharacterLock;
        private bool previousControl;
        private bool previousFlip;

        public bool MovementLocked
        {
            get => movementLocked;
            set
            {
                if (movementLocked == value) return;
                movementLocked = value;
                if (value) AcquireCharacterLock();
                else ReleaseCharacterLock();
            }
        }

        public void Move(Vector3 displacement)
        {
            transform.position += displacement;
            if (animator == null) return;
            bool moving = displacement.sqrMagnitude > 0.000001f;
            animator.SetBool("isRun", moving);
            if (moving)
                animator.SetFloat("moveSpeed", character != null ? character.MovementAnimationSpeed : 1f);
        }

        public void StopMovementVisual()
        {
            if (animator != null) animator.SetBool("isRun", false);
        }

        public void StartMiningAnimation()
        {
            if (animator == null || miningAnimatorController == null) return;
            animator.runtimeAnimatorController = miningAnimatorController;

            animator.SetBool("isRun", false);
            animator.SetBool("isLogging", true);
            animator.Update(0f);
        }

        public bool HasCompletedMiningLoops(int loopCount)
        {
            if (animator == null) return true;
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            return state.IsName(MiningStateName) && state.normalizedTime >= loopCount;
        }

        public void RestoreDefaultAnimation()
        {
            if (animator == null) return;
            if (HasParameter("isLogging")) animator.SetBool("isLogging", false);
            if (defaultAnimatorController != null)
                animator.runtimeAnimatorController = defaultAnimatorController;
        }

        public void SetFacingFromWorldX(float worldX)
        {
            if (Mathf.Abs(worldX) > 0.001f)
                SetFacingLeft(worldX > 0f);
        }

        private void Awake()
        {
            character = GetComponent<Character>();
            animator = character != null ? character.anim : GetComponentInChildren<Animator>(true);
            if (animator != null) defaultAnimatorController = animator.runtimeAnimatorController;
            if (facingRenderers == null || facingRenderers.Length == 0)
                facingRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        }

        private bool HasParameter(string parameterName)
        {
            if (animator == null) return false;
            foreach (AnimatorControllerParameter parameter in animator.parameters)
                if (parameter.name == parameterName) return true;
            return false;
        }

        private void Update()
        {
            if (MovementLocked || character != null) return;

            Vector2 input = ReadMoveInput();
            if (input.sqrMagnitude <= 0.01f)
            {
                StopMovementVisual();
                return;
            }

            Move(new Vector3(input.x, 0f, input.y) * (moveSpeed * Time.deltaTime));
            SetFacingLeft(input.x > 0f);
        }

        private void OnDisable()
        {
            movementLocked = false;
            RestoreDefaultAnimation();
            ReleaseCharacterLock();
        }

        private void SetFacingLeft(bool faceLeft)
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

        private void AcquireCharacterLock()
        {
            if (character == null || ownsCharacterLock) return;
            previousControl = character.isCanControll;
            previousFlip = character.canFlip;
            character.isCanControll = false;
            character.canFlip = false;
            character.SetExternalMovementVisualControl(true);
            ownsCharacterLock = true;
        }

        private void ReleaseCharacterLock()
        {
            StopMovementVisual();
            if (!ownsCharacterLock) return;
            character.SetExternalMovementVisualControl(false);
            character.isCanControll = previousControl;
            character.canFlip = previousFlip;
            ownsCharacterLock = false;
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
