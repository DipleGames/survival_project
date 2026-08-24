using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour 
{
    private int maxhp = 100; //maxhp 선언, 초기값 100
    public int currentHp; //currentHp 선언, 인스펙터에서 보임

    private void Start()
    {
        currentHp = maxhp; //currentHp에 maxhp를 할당
    }

    private void OnTriggerEnter2D(Collider2D collision) //is Trigger = true일때 작동하는 매서드 //객체 = 뼈, 오늘 확인한 내용 : 충돌한 두 물체가 둘 다 is trigger 상태여야 정상적으로 작동함
    {
        if (collision.name == "Bullet(Clone)") //충돌한 오브젝트 이름이 Bullet(Clone)이면 <- 왜 Bullet이 아니라 Bullet(Clone)? 게임플레이중에,ㄴ 좌클릭했을때 생성되는 오브젝트의 이름이 Bullet(Clone)이기 때문에 
        {
            currentHp -= 10; //10 차감
            if (currentHp <= 0) //currentHp가 0 이하가 되면
            {
                Destroy(gameObject); //오브젝트 자체를 없앰
            }
        }
    }

}
