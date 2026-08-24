using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private Transform enemyTr; //bullet이 향하는데 필요한것 : 적 위치를 저장할 enemyTr 선언
    [SerializeField] private float speed = 5f; //bullet 자신의 속도 = speed 선언, 초기값 5f


    public void InitBullet(Transform enemyTr) //불릿 초기화
    {
        this.enemyTr = enemyTr; //전달받은 적의 위치를 총알 자신의 변수 = enemyTr에 전달받아 저장
    }//위에서, 적 위치를 저장하는 용도인 enemyTr이랑 지금 클래스 내에서 사용하는 enemyTr이 이름이 똑같아 this를 사용해 enemyTr 변수를 구분함



    private void Update()
    {
        Vector3 dir = (enemyTr.position - transform.position).normalized; //bullet이 적까지 날아가는데 필요한 백터 계산 : 적(enemyTr) 위치 - 내위치
        transform.position += dir * speed * Time.deltaTime; //사양에 상관없이 항상 똑같은 속도로 bullet이 날아가도록 속도 정규화

    }

    private void OnTriggerEnter2D(Collider2D collision) //is Trigger = true일때 작동하는 매서드 //객체 = 뼈
    {
        if (collision.name == "Enemy") //부딪힌 대상의 이름이 "Enemy"이면
        {
            Destroy(gameObject); //총알 자신(gameObject)을 파괴함
        }

    }

}
