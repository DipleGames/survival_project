using UnityEngine;
using UnityEngine.AI;

namespace MineTest
{
    [DisallowMultipleComponent]
    public sealed class MineTestPlayerController : MonoBehaviour
    {
        private const string MiningStateName = "Logging";

        [SerializeField, Min(0.1f)] private float moveSpeed = 3f;
        [SerializeField] private SpriteRenderer[] facingRenderers;
        [SerializeField, Min(0)] private int miningAnimatorIndex = 6;
        [SerializeField] private RuntimeAnimatorController standaloneMiningController;

        private Character character;
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

        // 자동 접근 중 NavMeshAgent 또는 Transform으로 캐릭터를 이동시키고 달리기 연출을 갱신한다.
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

            if (animator == null) 
            {
                return;
            }

            bool moving = displacement.sqrMagnitude > 0.000001f;
            animator.SetBool("isRun", moving);

            if (moving)
            {
                animator.SetFloat("moveSpeed", character != null ? character.MovementAnimationSpeed : 1f);
            }
        }

        // 자동 이동이 끝났을 때 달리기 애니메이션 표시를 중지한다.
        public void StopMovementVisual()
        {
            if (animator != null) 
            {
                animator.SetBool("isRun", false);
            }
        }

        // Character의 채광용 컨트롤러로 교체하고 Logging 상태 재생을 시작한다.
        public void StartMiningAnimation()
        {
            if (animator == null) 
            {
                return;
            }

            RuntimeAnimatorController miningController = character != null
                ? character.GetAnimationController(miningAnimatorIndex)
                : standaloneMiningController;

            if (miningController == null) 
            {
                return;
            }

            animator.runtimeAnimatorController = miningController;

            animator.SetBool("isRun", false);
            animator.SetBool("isLogging", true);
            animator.Update(0f);
        }

        // Logging 상태가 요청된 반복 횟수만큼 재생되었는지 확인한다.
        public bool HasCompletedMiningLoops(int loopCount)
        {
            if (animator == null) 
            {
                return true;
            }

            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

            return state.IsName(MiningStateName) && state.normalizedTime >= loopCount;
        }

        // 채광 애니메이션을 종료하고 Character의 0번째 기본 컨트롤러로 복구한다.
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
            }
        }

        // 대상과의 월드 X축 방향을 기준으로 캐릭터가 바라볼 방향을 정한다.
        public void SetFacingFromWorldX(float worldX)
        {
            if (Mathf.Abs(worldX) > 0.001f)
            {
                SetFacingLeft(worldX > 0f);
            }
        }

        // Character, NavMeshAgent, Animator와 방향 전환용 렌더러 참조를 초기화한다.
        private void Awake()
        {
            character = GetComponent<Character>();
            agent = GetComponent<NavMeshAgent>();
            animator = character != null ? character.anim : GetComponentInChildren<Animator>(true);

            if (animator != null) 
            {
                defaultAnimatorController = animator.runtimeAnimatorController;
            }

            if (facingRenderers == null || facingRenderers.Length == 0)
            {
                facingRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            }
        }

        // 현재 Animator Controller에 지정된 파라미터가 존재하는지 검사한다.
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

        // Character가 없는 독립 테스트 플레이어에 한해 수동 이동 입력을 처리한다.
        private void Update()
        {
            if (MovementLocked || character != null) 
            {
                return;
            }

            Vector2 input = ReadMoveInput();

            if (input.sqrMagnitude <= 0.01f)
            {
                StopMovementVisual();
                return;
            }

            Move(new Vector3(input.x, 0f, input.y) * (moveSpeed * Time.deltaTime));
            SetFacingLeft(input.x > 0f);
        }

        // 비활성화 시 채광 애니메이션과 이동 잠금을 해제하고 기본 상태로 되돌린다.
        private void OnDisable()
        {
            movementLocked = false;
            RestoreDefaultAnimation();
            ReleaseCharacterLock();
        }

        // Character 또는 하위 SpriteRenderer가 왼쪽을 바라보도록 설정한다.
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

        // 자동 접근과 채광 중 Character의 일반 이동 및 방향 전환 제어권을 잠근다.
        private void AcquireCharacterLock()
        {
            if (character == null || ownsCharacterLock) 
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

            ownsCharacterLock = true;
        }

        // 채광 전에 저장한 Character와 NavMeshAgent 상태를 복구한다.
        private void ReleaseCharacterLock()
        {
            StopMovementVisual();

            if (!ownsCharacterLock) 
            {
                return;
            }

            character.SetExternalMovementVisualControl(false);
            character.isCanControll = previousControl;
            character.canFlip = previousFlip;

            if (agent != null) 
            {
                agent.enabled = previousAgentEnabled;
            }

            ownsCharacterLock = false;
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
