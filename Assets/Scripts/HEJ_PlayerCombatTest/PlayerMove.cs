using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public class PlayerMove : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float moveSpeed = 3f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField, Min(0f)] private float movementAnimationSpeed = 1f;

    [Header("Facing")]
    [SerializeField] private SpriteRenderer[] facingRenderers;
    [SerializeField] private bool defaultFacingLeft = true;

    private NavMeshAgent agent;

    private static readonly int IsRunHash = Animator.StringToHash("isRun");
    private static readonly int MoveSpeedHash = Animator.StringToHash("moveSpeed");

    public bool MovementLocked { get; set; }

    public bool HasMoveInput => ReadMoveInput().sqrMagnitude > 0.01f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>(true);
        }

        if (facingRenderers == null || facingRenderers.Length == 0)
        {
            facingRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        }
    }

    private void Update()
    {
        if (MovementLocked)
        {
            return;
        }

        Vector2 input = ReadMoveInput();
        bool isMoving = input.sqrMagnitude > 0.01f;

        UpdateAnimation(isMoving);

        if (!isMoving)
        {
            return;
        }

        Move(input);
        UpdateFacing(input.x);
    }

    private void Move(Vector2 input)
    {
        Vector3 displacement = new Vector3(input.x, 0f, input.y) * (moveSpeed * Time.deltaTime);

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.Move(displacement);
        }
        else
        {
            transform.position += displacement;
        }
    }

    private void UpdateFacing(float horizontal)
    {
        if (Mathf.Abs(horizontal) <= 0.001f || facingRenderers == null)
        {
            return;
        }
       
        bool flipX = defaultFacingLeft ? horizontal > 0f : horizontal < 0f;
        foreach (SpriteRenderer facingRenderer in facingRenderers)
        {
            if (facingRenderer != null)
            {
                facingRenderer.flipX = flipX;
            }
        }
    }
    // 외부에서 월드 방향을 전달해 바라보는 방향을 변경
    public void FaceDirection(Vector3 direction)
    {
        UpdateFacing(direction.x);
    }

    private void UpdateAnimation(bool isMoving)
    {
        if (animator == null)
        {
            return;
        }

        animator.SetBool(IsRunHash, isMoving);

        if (isMoving)
        {
            animator.SetFloat(MoveSpeedHash, movementAnimationSpeed);
        }
    }

    private void OnDisable()
    {
        UpdateAnimation(false);
    }

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