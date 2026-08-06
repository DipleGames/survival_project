using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMove : MonoBehaviour // MonoBehaviour를 상속받는 TestMove 클래스를 선언
{
    [SerializeField] private float moveSpeed = 3f; //moveSpeed라는 float 변수를 private으로 선언하되, [SerializeField]를 붙여서 인스펙터 창에는 노출되도록 한다. 초기값은 3.
    private SpriteRenderer sr; //SpriteRenderer 타입의 변수 sr을 private으로 선언해서 선언한 클래스 내부에서만 접근이 가능하도록 함. 인스펙터 창에는 노출되지 않음.
    public Sprite sprite; //Sprite 타입의 변수 sprite를 public으로 선언, 모든 곳에서 해당 변수나 메소드에 접근 가능. 인스펙터 창에 노출됨.

    private void Awake() //가장 먼저 호출되는 함수. Start보다 우선순위로 진행됨
    {
        sr = GetComponent<SpriteRenderer>(); //sr이라는 값에 <SpriteRenderer>을 넣음
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
    }

}
