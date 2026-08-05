using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TamingPetMovement : MonoBehaviour
{
    [SerializeField] Transform spawnPos;
    [SerializeField] Transform arrivePos;
    [SerializeField] float moveSpeed;

    GamesceneManager gamesceneManager;  

    void Start()
    {
        gamesceneManager = GamesceneManager.Instance;
        transform.position = spawnPos.position;
    }

    void Update()
    {
        if(gamesceneManager.isNight)
            gameObject.SetActive(false);

        transform.position = Vector3.MoveTowards(transform.position,arrivePos.position, Time.deltaTime * moveSpeed);
    }

    private void OnEnable()
    {
        transform.position = spawnPos.position;
    }
}
