using UnityEngine;
using UnityEngine.AI;

namespace MineTest
{
    [DisallowMultipleComponent]
    public sealed class MineTestPlayerController : MonoBehaviour
    {
        private const string MiningStateName = "Logging";

        [SerializeField] private SpriteRenderer[] facingRenderers;
        [SerializeField, Min(0)] private int miningAnimatorIndex = 6;
        [SerializeField] private RuntimeAnimatorController miningAnimatorController;

        private Character character;
        private PlayerMove playerMove;
        private NavMeshAgent agent;
        private Animator animator;
        private RuntimeAnimatorController defaultAnimatorController;

        private bool movementLocked;
        private bool ownsCharacterLock;
        private bool previousControl;
        private bool previousFlip;
        private bool previousAgentEnabled;

        public bool MovementLocked
        {
            get => movementLocked;
            set
            {
                if (movementLocked == value)
                {
                    return;
                }

                movementLocked = value;

                if (value)
                {
                    AcquireCharacterLock();
                }
                else
                {
                    ReleaseCharacterLock();
                }
            }
        }

        private void Awake()
        {
            character = GetComponent<Character>();
            playerMove = GetComponent<PlayerMove>();
            agent = GetComponent<NavMeshAgent>();

            animator = character != null
                ? character.anim
                : GetComponentInChildren<Animator>(true);

            if (animator != null)
            {
                defaultAnimatorController = animator.runtimeAnimatorController;
            }

            if (facingRenderers == null || facingRenderers.Length == 0)
            {
                facingRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            }
        }

        // 채광 자동 접근에서 사용한다.
        public void Move(Vector3 displacement)
        {
            // 자동화 중에는 NavMeshAgent를 잠그므로 Transform으로 이동한다.
            transform.position += displacement;

            if (animator == null)
            {
                return;
            }

            bool moving = displacement.sqrMagnitude > 0.000001f;

            if (HasParameter("isRun"))
            {
                animator.SetBool("isRun", moving);
            }

            if (moving && HasParameter("moveSpeed"))
            {
                animator.SetFloat(
                    "moveSpeed",
                    character != null ? character.MovementAnimationSpeed : 1f
                );
            }
        }

        public void StopMovementVisual()
        {
            if (animator == null)
            {
                return;
            }

            if (HasParameter("isRun"))
            {
                animator.SetBool("isRun", false);
            }
        }

        // 채광용 Animator Controller로 전환한다.
        public void StartMiningAnimation()
        {
            if (animator == null)
            {
                return;
            }

            if (miningAnimatorController == null)
            {
                Debug.LogWarning(
                    "[MineTest] Mining Animator Controller가 할당되지 않았습니다.",
                    this
                );

                return;
            }

            animator.runtimeAnimatorController = miningAnimatorController;

            if (HasParameter("isRun"))
            {
                animator.SetBool("isRun", false);
            }

            if (HasParameter("isLogging"))
            {
                animator.SetBool("isLogging", true);
            }
            else
            {
                Debug.LogWarning(
                    "[MineTest] Mining Animator에 isLogging 파라미터가 없습니다.",
                    this
                );
            }

            animator.Update(0f);
        }

        // 채광 애니메이션이 지정된 횟수만큼 재생되었는지 확인한다.
        public bool HasCompletedMiningLoops(int loopCount)
        {
            if (animator == null)
            {
                return true;
            }

            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

            return state.IsName(MiningStateName) &&
                   state.normalizedTime >= loopCount;
        }

        // 기본 Animator Controller로 복구한다.
        public void RestoreDefaultAnimation()
        {
            if (animator == null)
            {
                return;
            }

            if (HasParameter("isLogging"))
            {
                animator.SetBool("isLogging", false);
            }

            RuntimeAnimatorController controllerToRestore = character != null
                ? character.GetAnimationController(0)
                : defaultAnimatorController;

            if (controllerToRestore != null)
            {
                animator.runtimeAnimatorController = controllerToRestore;
                animator.Update(0f);
            }
        }

        // 광물이 있는 방향을 바라본다.
        public void SetFacingFromWorldX(float worldX)
        {
            if (Mathf.Abs(worldX) <= 0.001f)
            {
                return;
            }

            SetFacingLeft(worldX > 0f);
        }

        private void SetFacingLeft(bool faceLeft)
        {
            if (character != null)
            {
                character.SetFacingLeft(faceLeft);
                return;
            }

            if (facingRenderers == null)
            {
                return;
            }

            foreach (SpriteRenderer renderer in facingRenderers)
            {
                if (renderer != null)
                {
                    renderer.flipX = faceLeft;
                }
            }
        }

        private void AcquireCharacterLock()
        {
            if (ownsCharacterLock)
            {
                return;
            }

            // PlayerMove의 수동 이동을 막는다.
            if (playerMove != null)
            {
                playerMove.MovementLocked = true;
            }

            if (character != null)
            {
                previousControl = character.isCanControll;
                previousFlip = character.canFlip;

                character.isCanControll = false;
                character.canFlip = false;
                character.SetExternalMovementVisualControl(true);
            }

            // 자동 접근 중에는 Transform 이동을 사용하기 때문에
            // NavMeshAgent의 자체 제어를 잠시 비활성화한다.
            previousAgentEnabled = agent != null && agent.enabled;

            if (agent != null && agent.enabled)
            {
                agent.enabled = false;
            }

            ownsCharacterLock = true;
        }

        private void ReleaseCharacterLock()
        {
            StopMovementVisual();

            if (!ownsCharacterLock)
            {
                return;
            }

            if (character != null)
            {
                character.SetExternalMovementVisualControl(false);
                character.isCanControll = previousControl;
                character.canFlip = previousFlip;
            }

            if (agent != null)
            {
                agent.enabled = previousAgentEnabled;
            }

            if (playerMove != null)
            {
                playerMove.MovementLocked = false;
            }

            ownsCharacterLock = false;
        }

        private bool HasParameter(string parameterName)
        {
            if (animator == null)
            {
                return false;
            }

            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.name == parameterName)
                {
                    return true;
                }
            }

            return false;
        }

        private void OnDisable()
        {
            movementLocked = false;

            RestoreDefaultAnimation();
            ReleaseCharacterLock();
        }
    }
}