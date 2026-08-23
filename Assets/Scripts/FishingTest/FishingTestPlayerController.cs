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
                if (movementLocked == value) 
                {
                    return;
                }

                movementLocked = value;

                if (value) 
                {
                    AcquireLock();
                }
                else
                {
                    ReleaseLock();
                }
            }
        }

        public bool AutoMovingVisual { get; set; }
        public Vector2 MoveInput { get; private set; }
        public bool HasMoveInput => MoveInput.sqrMagnitude > 0.01f;

        // 캐릭터, NavMeshAgent, Animator와 방향 전환용 렌더러 참조를 초기화한다.
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
            {
                facingRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            }
        }

        // 매 프레임 이동 입력을 읽고 테스트 씬의 독립 플레이어를 직접 이동시킨다.
        private void Update()
        {
            MoveInput = ReadMoveInput();

            if (character == null && !MovementLocked && HasMoveInput)
            {
                SetFacingLeft(MoveInput.x > 0f);
                Move(new Vector3(MoveInput.x, 0f, MoveInput.y) * moveSpeed * Time.deltaTime);
            }
        }

        // NavMeshAgent 또는 Transform을 이용해 지정된 월드 변위만큼 캐릭터를 이동시킨다.
        public void Move(Vector3 displacement)
        {
            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                agent.Move(displacement);
            }
            else
            {
                transform.position += displacement;
            }
        }

        // 이 컴포넌트가 이동을 담당하는 동안 달리기 애니메이션 파라미터를 갱신한다.
        private void LateUpdate()
        {
            if (animator == null) 
            {
                return;
            }

            // Animator Controller가 캐릭터의 Animator에 지정되어 있지 않으면 기본 컨트롤러를 적용한다.
            // parameters while this component is actually controlling movement.
            if (character != null && !MovementLocked) 
            {
                return;
            }

            bool moving = AutoMovingVisual || (character == null && !MovementLocked && HasMoveInput);
            animator.SetBool("isRun", moving);

            if (moving)
            {
                animator.SetFloat("moveSpeed", character != null ? character.MovementAnimationSpeed : 1f);
            }
        }

        // 비활성화될 때 자동 이동 상태와 캐릭터 제어 잠금을 안전하게 해제한다.
        private void OnDisable()
        {
            movementLocked = false;
            AutoMovingVisual = false;
            ReleaseLock();
        }

        // 캐릭터 또는 하위 SpriteRenderer가 왼쪽을 바라보도록 설정한다.
        public void SetFacingLeft(bool faceLeft)
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

        // 낚시 이동 중 Character의 입력, 방향 전환, NavMeshAgent 제어권을 잠근다.
        private void AcquireLock()
        {
            if (character == null || ownsLock) 
            {
                return;
            }

            previousControl = character.isCanControll;
            previousFlip = character.canFlip;
            previousAgentEnabled = agent != null && agent.enabled;

            character.isCanControll = false;
            character.canFlip = false;
            character.SetExternalMovementVisualControl(true);

            if (agent != null && agent.enabled) 
            {
                agent.enabled = false;
            }

            ownsLock = true;
        }

        // 잠금 전 상태를 복원하여 Character에 일반 이동 제어권을 돌려준다.
        private void ReleaseLock()
        {
            if (!ownsLock) 
            {
                return;
            }

            if (animator != null) 
            {
                animator.SetBool("isRun", false);
            }

            character.SetExternalMovementVisualControl(false);
            character.isCanControll = previousControl;
            character.canFlip = previousFlip;

            if (agent != null) 
            {
                agent.enabled = previousAgentEnabled;
            }

            ownsLock = false;
        }

        // 사용자 지정 키 설정 또는 기본 축에서 정규화된 이동 입력을 읽는다.
        private static Vector2 ReadMoveInput()
        {
            KeyCode left = (KeyCode)PlayerPrefs.GetInt("Key_Left");
            KeyCode right = (KeyCode)PlayerPrefs.GetInt("Key_Right");
            KeyCode down = (KeyCode)PlayerPrefs.GetInt("Key_Down");
            KeyCode up = (KeyCode)PlayerPrefs.GetInt("Key_Up");

            if (left == KeyCode.None && right == KeyCode.None && down == KeyCode.None && up == KeyCode.None)
            {
                return Vector2.ClampMagnitude(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")), 1f);
            }

            float x = (Input.GetKey(right) ? 1f : 0f) - (Input.GetKey(left) ? 1f : 0f);
            float y = (Input.GetKey(up) ? 1f : 0f) - (Input.GetKey(down) ? 1f : 0f);

            return Vector2.ClampMagnitude(new Vector2(x, y), 1f);
        }
    }
}
