using UnityEngine;
using UnityEngine.AI;

namespace MineTest
{
    [DisallowMultipleComponent]
    public sealed class MinePlayerController : MonoBehaviour
    {
        private const string MiningStateName = "Logging";

        [Header("Character Visual")]
        [SerializeField] private SpriteRenderer[] facingRenderers;
        [SerializeField] private RuntimeAnimatorController miningAnimatorController;

        private Player player;
        private PlayerMove playerMove;
        private NavMeshAgent agent;
        private Animator animator;

        private RuntimeAnimatorController previousAnimatorController;

        private bool movementLocked;
        private bool ownsMovementLock;
        private bool previousMovementLocked;
        private bool previousAgentEnabled;
        private bool usingMiningController;

        public bool MovementLocked
        {
            get => movementLocked;
            set
            {
                if (movementLocked == value)
                    return;

                movementLocked = value;

                if (value)
                    AcquireMovementLock();
                else
                    ReleaseMovementLock();
            }
        }

        private void Awake()
        {
            player = GetComponent<Player>();
            playerMove = GetComponent<PlayerMove>();
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponentInChildren<Animator>(true);

            if (facingRenderers == null || facingRenderers.Length == 0)
                facingRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        }

        /// <summary>
        /// 채광 자동 접근 중 플레이어를 이동시키고 이동 애니메이션을 갱신한다.
        /// </summary>
        public void Move(Vector3 displacement)
        {
            transform.position += displacement;

            if (animator == null)
                return;

            bool moving = displacement.sqrMagnitude > 0.000001f;

            if (HasParameter("isRun"))
                animator.SetBool("isRun", moving);

            if (moving && HasParameter("moveSpeed"))
            {
                float moveSpeed = player != null && player.Status != null
                    ? player.Status.GetStat(StatType.MoveSpeed)
                    : 1f;

                animator.SetFloat("moveSpeed", moveSpeed);
            }
        }

        /// <summary>
        /// 이동 애니메이션을 정지한다.
        /// </summary>
        public void StopMovementVisual()
        {
            if (animator == null)
                return;

            if (HasParameter("isRun"))
                animator.SetBool("isRun", false);
        }

        /// <summary>
        /// 채광용 Animator Controller로 전환하고 채광 애니메이션을 시작한다.
        /// </summary>
        public void StartMiningAnimation()
        {
            if (animator == null)
                return;

            if (miningAnimatorController == null)
            {
                Debug.LogWarning("[MineTest] Mining Animator Controller가 할당되지 않았습니다.", this);
                return;
            }

            if (!usingMiningController)
            {
                previousAnimatorController = animator.runtimeAnimatorController;
                usingMiningController = true;
            }

            animator.runtimeAnimatorController = miningAnimatorController;

            if (HasParameter("isRun"))
                animator.SetBool("isRun", false);

            if (HasParameter("isLogging"))
                animator.SetBool("isLogging", true);
            else
                Debug.LogWarning("[MineTest] Mining Animator에 isLogging 파라미터가 없습니다.", this);

            animator.Update(0f);
        }

        /// <summary>
        /// 채광 애니메이션이 지정된 횟수만큼 반복되었는지 확인한다.
        /// </summary>
        public bool HasCompletedMiningLoops(int loopCount)
        {
            if (animator == null)
                return true;

            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

            return state.IsName(MiningStateName) && state.normalizedTime >= loopCount;
        }

        /// <summary>
        /// 채광 종료 후 이전 Animator Controller로 복구한다.
        /// </summary>
        public void RestoreDefaultAnimation()
        {
            if (animator == null || !usingMiningController)
                return;

            if (animator.runtimeAnimatorController == miningAnimatorController)
            {
                if (HasParameter("isLogging"))
                    animator.SetBool("isLogging", false);

                animator.runtimeAnimatorController = previousAnimatorController;
                animator.Update(0f);
            }

            previousAnimatorController = null;
            usingMiningController = false;
        }

        /// <summary>
        /// 광물의 X 방향에 따라 플레이어의 바라보는 방향을 설정한다.
        /// </summary>
        public void SetFacingFromWorldX(float worldX)
        {
            if (Mathf.Abs(worldX) <= 0.001f)
                return;

            SetFacingLeft(worldX > 0f);
        }

        /// <summary>
        /// SpriteRenderer의 좌우 반전 상태를 변경한다.
        /// </summary>
        private void SetFacingLeft(bool faceLeft)
        {
            if (facingRenderers == null)
                return;

            foreach (SpriteRenderer renderer in facingRenderers)
            {
                if (renderer != null)
                    renderer.flipX = faceLeft;
            }
        }

        /// <summary>
        /// 채광 자동화 중 플레이어의 수동 이동과 NavMeshAgent 이동을 잠근다.
        /// </summary>
        private void AcquireMovementLock()
        {
            if (ownsMovementLock)
                return;

            // 기존 이동 잠금 상태 저장 후 수동 이동 차단
            if (playerMove != null)
            {
                previousMovementLocked = playerMove.MovementLocked;
                playerMove.MovementLocked = true;
            }

            // 자동 접근에서 Transform 이동을 사용하므로 Agent 비활성화
            previousAgentEnabled = agent != null && agent.enabled;

            if (agent != null && agent.enabled)
                agent.enabled = false;

            ownsMovementLock = true;
        }

        /// <summary>
        /// 채광 종료 후 플레이어의 이동 상태를 복원한다.
        /// </summary>
        private void ReleaseMovementLock()
        {
            StopMovementVisual();

            if (!ownsMovementLock)
                return;

            if (agent != null)
                agent.enabled = previousAgentEnabled;

            if (playerMove != null)
                playerMove.MovementLocked = previousMovementLocked;

            ownsMovementLock = false;
        }

        /// <summary>
        /// Animator에 지정된 파라미터가 존재하는지 확인한다.
        /// </summary>
        private bool HasParameter(string parameterName)
        {
            if (animator == null)
                return false;

            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.name == parameterName)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 비활성화될 때 채광 애니메이션과 이동 잠금을 정리한다.
        /// </summary>
        private void OnDisable()
        {
            movementLocked = false;

            RestoreDefaultAnimation();
            ReleaseMovementLock();
        }
    }
}