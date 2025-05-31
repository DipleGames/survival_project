using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics.Internal;
using UnityEngine;
using UnityEngine.AI;

public class TempMove : MonoBehaviour
{
    public float speed = 5f;

    NavMeshAgent agent;

    Vector3 dir;
    float x;
    float z;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = false;
    }

    void FixedUpdate()
    {
        if (agent.enabled)
            Move();
    }

    bool isDownUp = false;
    bool isLeftRight = false;
    void Move()
    {
        bool xInput = (Input.GetKey((KeyCode)PlayerPrefs.GetInt("Key_Left")))
            || (Input.GetKey((KeyCode)PlayerPrefs.GetInt("Key_Right")));
        bool zInput = (Input.GetKey((KeyCode)PlayerPrefs.GetInt("Key_Up"))) || (Input.GetKey((KeyCode)PlayerPrefs.GetInt("Key_Down")));

        if (!xInput)
            x = 0;

        else if (xInput)
        {
            if (Input.GetKey((KeyCode)PlayerPrefs.GetInt("Key_Left")))
            {
                x = -1;

                if (!isLeftRight && Input.GetKey((KeyCode)PlayerPrefs.GetInt("Key_Right")))
                {
                    x = 1;
                }
            }

            else if (Input.GetKey((KeyCode)PlayerPrefs.GetInt("Key_Right")))
            {
                x = 1;
                isLeftRight = true;
            }

            if (Input.GetKeyUp((KeyCode)PlayerPrefs.GetInt("Key_Right")))
                isLeftRight = false;
        }

        if (!zInput)
            z = 0;

        else if (zInput)
        {
            if (Input.GetKey((KeyCode)PlayerPrefs.GetInt("Key_Up")))
            {
                z = 1;

                if (!isDownUp && Input.GetKey((KeyCode)PlayerPrefs.GetInt("Key_Down")))
                    z = -1;
            }

            else if (Input.GetKey((KeyCode)PlayerPrefs.GetInt("Key_Down")))
            {
                z = -1;
                isDownUp = true;
            }

            if (Input.GetKeyUp((KeyCode)PlayerPrefs.GetInt("Key_Down")))
                isDownUp = false;
        }


        dir = (Vector3.right * x + Vector3.forward * z).normalized;

        // Y축 기준으로 좌측(반시계 방향)으로 30도 회전
        
       /* Quaternion rotation = Quaternion.AngleAxis(multiDir, Vector3.up);
        dir = rotation * dir;*/

        agent.speed = speed;
        agent.Move(dir * speed * Time.deltaTime);
    }
    //public int multiDir = 45;
}
