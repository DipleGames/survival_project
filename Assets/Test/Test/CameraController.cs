using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour 
{
    //카메라가 Dog를 따라가도록 하기 위해서 필요한것 : Dog 위치, 카메라 위치
    [SerializeField] private GameObject player; //Dog 받아올 player 선언
    public float cameraSpeed = 5.0f;
    
    private void Start()
    {
        player = GameObject.Find("Dog"); //Dog 가져오기
    }

    private void Update()
    {
        Vector3 dir = player.transform.position - transform.position;     //목적지(Dog) 위치 - 내(카메라) 위치
        Vector3 moveVector = new Vector3(dir .x * cameraSpeed * Time.deltaTime , dir .y * cameraSpeed * Time.deltaTime, 0.0f);
        transform.Translate(moveVector);
    }



}
