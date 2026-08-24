using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAttack : MonoBehaviour
{
    [SerializeField] private GameObject bullet; //Bullet(대상 프리팹)을 받아올 bullet 선언
    [SerializeField] private Transform enemyTr; //Enemy의 위치값을 받아올 enemyTr 선언

    void Start() 
    {
        enemyTr = GameObject.Find("Enemy").GetComponent<Transform>(); //enemyTr에 뼈=Enemy라는 오브젝트를 찾고, 그 안에 들어있는 Transform을 가져와 할당한다
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) //0은 좌클릭을 의미함
        {
            if (enemyTr == null) //enemyTr이 null이면 == hp 0이되어 이미 파괴된 상태라면
            {
                return;
            }
            GameObject go =  Instantiate(bullet, transform.position, Quaternion.identity); //오브젝트 생성:Instantiate-bullet, 생성위치-플레이어 위치, 회전값-없음(기본값) 그걸 go안에 저장함 => bullet 생성!
            Bullet bulletScript = go.GetComponent<Bullet>(); //Bullet(=뼈)을 상속받는? bulletScript 선언, go 속 컴포넌트(=살) bullet을 가져와 할당함
            bulletScript.InitBullet(enemyTr);  //생성된 총알(bulletScript)에 타겟인 적의 위치(enemyTr)를 할당 ... Bullet에서 적의 위치 저장한 enemyTr을 참조해옴

        }
        

    }
}
