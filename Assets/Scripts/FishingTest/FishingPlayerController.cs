using UnityEngine;
using UnityEngine.AI;

namespace FishingTest
{
    [DisallowMultipleComponent]
    public sealed class FishingPlayerController : MonoBehaviour
    {
        [Header("Character Visual")]
        [SerializeField] private SpriteRenderer[] facingRenderers;

        private Player player;
        private PlayerMove playerMove;
        private NavMeshAgent agent;
        private Animator animator;

        private bool movementLocked;
        private bool ownsLock;
        private bool previousMovementLocked;
        private bool previousAgentEnabled;

        public bool MovementLocked
        {
            get => movementLocked;
            set
            {
                if (movementLocked == value)
                    return;

                movementLocked = value;

                if (value)
                    AcquireLock();
                else
                    ReleaseLock();
            }
        }

        public bool AutoMovingVisual { get; set; }
        public bool HasMoveInput => ReadMoveInput().sqrMagnitude > 0.01f;

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
        /// 낚시 자동 접근 중 플레이어를 이동시킨다.
        /// </summary>
        public void Move(Vector3 displacement)
        {
            transform.position += displacement;
        }

        /// <summary>
        /// 자동 이동 상태에 따라 이동 애니메이션을 갱신한다.
        /// </summary>
        private void LateUpdate()
        {
            if (animator == null || !MovementLocked)
                return;

            if (HasParameter("isRun"))
                animator.SetBool("isRun", AutoMovingVisual);

            if (AutoMovingVisual && HasParameter("moveSpeed"))
            {
                float moveSpeed = player != null && player.Status != null
                    ? player.Status.GetStat(StatType.MoveSpeed)
                    : 1f;

                animator.SetFloat("moveSpeed", moveSpeed);
            }
        }

        /// <summary>
        /// 플레이어의 좌우 바라보는 방향을 설정한다.
        /// </summary>
        public void SetFacingLeft(bool faceLeft)
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
        /// 낚시 자동화 중 수동 이동과 NavMeshAgent를 잠근다.
        /// </summary>
        private void AcquireLock()
        {
            if (ownsLock)
                return;

            // 기존 이동 잠금 상태 저장
            if (playerMove != null)
            {
                previousMovementLocked = playerMove.MovementLocked;
                playerMove.MovementLocked = true;
            }

            // 자동 이동 중 Transform을 직접 사용하므로 Agent 비활성화
            previousAgentEnabled = agent != null && agent.enabled;

            if (agent != null && agent.enabled)
                agent.enabled = false;

            ownsLock = true;
        }

        /// <summary>
        /// 낚시 종료 후 이동 상태를 이전 상태로 복원한다.
        /// </summary>
        private void ReleaseLock()
        {
            if (!ownsLock)
                return;

            AutoMovingVisual = false;

            if (animator != null && HasParameter("isRun"))
                animator.SetBool("isRun", false);

            if (agent != null)
                agent.enabled = previousAgentEnabled;

            if (playerMove != null)
                playerMove.MovementLocked = previousMovementLocked;

            ownsLock = false;
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
        /// 현재 플레이어의 이동 입력값을 반환한다.
        /// 사용자 지정 키가 없으면 기본 Horizontal/Vertical 입력을 사용한다.
        /// </summary>
        private static Vector2 ReadMoveInput()
        {
            KeyCode left = (KeyCode)PlayerPrefs.GetInt("Key_Left", (int)KeyCode.None);
            KeyCode right = (KeyCode)PlayerPrefs.GetInt("Key_Right", (int)KeyCode.None);
            KeyCode down = (KeyCode)PlayerPrefs.GetInt("Key_Down", (int)KeyCode.None);
            KeyCode up = (KeyCode)PlayerPrefs.GetInt("Key_Up", (int)KeyCode.None);

            if (left == KeyCode.None && right == KeyCode.None &&
                down == KeyCode.None && up == KeyCode.None)
            {
                Vector2 input = new Vector2(
                    Input.GetAxisRaw("Horizontal"),
                    Input.GetAxisRaw("Vertical")
                );

                return Vector2.ClampMagnitude(input, 1f);
            }

            float x = (Input.GetKey(right) ? 1f : 0f) - (Input.GetKey(left) ? 1f : 0f);
            float y = (Input.GetKey(up) ? 1f : 0f) - (Input.GetKey(down) ? 1f : 0f);

            return Vector2.ClampMagnitude(new Vector2(x, y), 1f);
        }

        /// <summary>
        /// 컴포넌트 비활성화 시 이동 잠금을 해제한다.
        /// </summary>
        private void OnDisable()
        {
            MovementLocked = false;
            AutoMovingVisual = false;
        }
    }
}