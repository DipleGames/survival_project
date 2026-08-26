using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestMove : MonoBehaviour // MonoBehaviour를 상속받는 TestMove 클래스를 선언
{
    [SerializeField] private float moveSpeed = 3f; //moveSpeed라는 float 변수를 private으로 선언하되, [SerializeField]를 붙여서 인스펙터 창에는 노출되도록 한다. 초기값은 3.
    [SerializeField] private SpriteRenderer sr; //SpriteRenderer 타입의 변수 sr을 private으로 선언해서 선언한 클래스 내부에서만 접근이 가능하도록 함. 인스펙터 창에 보이도록 함.
    [SerializeField] private Camera cam;
    public Sprite sprite; //Sprite 타입의 변수 sprite를 public으로 선언, 모든 곳에서 해당 변수나 메소드에 접근 가능. 인스펙터 창에 노출됨.
    Animator Anim;

    private float idileTimer = 0f;

    private void Awake() //가장 먼저 호출되는 함수. Start보다 우선순위로 진행됨
    {
        sr = GetComponent<SpriteRenderer>(); 
        Anim = GetComponent<Animator>();
        cam = GetComponent<Camera>();
    }
    private void Start() //Awake보다 후순위로 진행, 스크립트가 활성화된 상태일 때 딱 한 만 호출됨.
    {
       
    }
    private void Update() //매 프레임마다 자동으로 반복 호출되는 함수 (Awake, Start 실행 이후부터 시작됨)
    {
        float v = Input.GetAxisRaw("Vertical"); //세로방향 입력값을 받아 v에 저장
        float h = Input.GetAxisRaw("Horizontal"); //가로방향 입력값을 받아 h에 저장
        Vector3 dir = new Vector3(h, v, 0).normalized; //vector 3 환경에서, h,v, 0의 값을 정규화(nomalized=길이 1로만듦), dir이라는 방향 백터로 저장.
        transform.position += dir * moveSpeed * Time.deltaTime; //현재 오브젝트의 위치=transform.position(인스펙터에 보이는 그 값)에 방향*이동속도*델타타임(프레임레이트와 무관하게 일정한 속도로 움직이도록 함)만큼 더해서 이동시킴.

        if (Input.GetKeyDown(KeyCode.Space)) //스페이스바 누르면 코루틴 시작
        {
            co = StartCoroutine(SpeedBoostCoroutine());
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (co != null)
            {
                StopCoroutine(co);
                co = null; //코루틴 강제종료
            } 
        }

        Anim.SetFloat("MoveX", h); //Animator Blend Tree로 입력값 (x,y) 전달
        Anim.SetFloat("MoveY", v);

        if (h != 0) //좌우 입력값에 따라 반전되도록 함
        {
            sr.flipX = (h < 0);
        }

        if (h == 0 && v == 0) //입력이 완전히 0일때
        {
            idileTimer += Time.deltaTime; //타이머 증가
            
            if (idileTimer >= 3f) //idleTimer가 3초가 되면
            {
                Anim.SetBool("isSpecialIdle", true); //Trigger 실행

                idileTimer = -7f; //다시 트리거 실행할때까지 10초 유예시간 설정  (3초 - 10초 = -7초)
            }
        }
        else
        {
            idileTimer = 0f; //입력 들어오면 타이머 초기화, 모션 초기화
            Anim.SetBool("isSpecialIdle", false);
        }
    }

    private void OnEnable() //활성화될때마다 호출 Awake와 Start 사이에 호출됨 
    {
        
    }

    private void OnDisable() //비활성화할때마다 호출 
    {
        
    }

    private void FixedUpdate() //모든 컴퓨터에서 항상 일정한 주기로 호출되는 함수 => 이동, 충돌, 힘 등 물리연산용
    {
        
    }

    private void LateUpdate() //모든 update 호출된 이후에 호출되는 함수 => 카메라 추적 
    {

    }

    private Coroutine co;


    private IEnumerator SpeedBoostCoroutine()
    {
        float originalSpped = moveSpeed;
        moveSpeed = moveSpeed * 2f; //속도 2배 증가
       
        yield return new WaitForSeconds(3f); //3초동안 대기

        moveSpeed = originalSpped; //원래 속도로 복구 -> 3초 지나면 그 사이에 스페이스바 몇번을 눌렀던지 무관하게 초기값으로 돌아감.

    }



}
