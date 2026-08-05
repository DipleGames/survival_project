using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveFog : MonoBehaviour
{
    [SerializeField] float moveSpeed;

    Renderer rend;
    Vector3 offset;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        offset = new Vector2(Time.time * moveSpeed, 0);
        rend.material.mainTextureOffset = offset;
    }
}
