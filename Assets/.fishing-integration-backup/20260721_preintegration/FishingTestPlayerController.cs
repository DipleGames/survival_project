using UnityEngine;

namespace FishingTest
{
    public sealed class FishingTestPlayerController : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float moveSpeed = 3f;

        private Animator characterAnimator;
        private SpriteRenderer[] spriteRenderers;

        public bool MovementLocked { get; set; }
        public bool AutoMovingVisual { get; set; }
        public Vector2 MoveInput { get; private set; }
        public bool HasMoveInput => MoveInput.sqrMagnitude > 0.01f;

        private void Awake()
        {
            characterAnimator = GetComponentInChildren<Animator>(true);
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            MakeCharacterOpaque();
        }

        private void Update()
        {
            MoveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            MoveInput = Vector2.ClampMagnitude(MoveInput, 1f);

            if (MovementLocked || !HasMoveInput)
                return;

            UpdateHorizontalFacing();
            Vector3 moveDirection = new Vector3(MoveInput.x, 0f, MoveInput.y);
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }

        private void LateUpdate()
        {
            bool moving = AutoMovingVisual || (!MovementLocked && HasMoveInput);
            if (characterAnimator != null)
            {
                characterAnimator.SetBool("isRun", moving);
                characterAnimator.SetFloat("moveSpeed", moving ? 1f : 0f);
            }
            MakeCharacterOpaque();
        }

        private void MakeCharacterOpaque()
        {
            if (spriteRenderers == null)
                return;
            foreach (SpriteRenderer renderer in spriteRenderers)
            {
                Color color = renderer.color;
                color.a = 1f;
                renderer.color = color;
            }
        }

        private void UpdateHorizontalFacing()
        {
            if (spriteRenderers == null || Mathf.Approximately(MoveInput.x, 0f))
                return;

            SetFacingLeft(MoveInput.x > 0f);
        }

        public void SetFacingLeft(bool faceLeft)
        {
            if (spriteRenderers == null)
                return;

            foreach (SpriteRenderer renderer in spriteRenderers)
                renderer.flipX = faceLeft;
        }
    }
}
