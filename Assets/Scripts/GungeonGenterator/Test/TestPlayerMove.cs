using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlayerMove : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody rb;
    private Vector2 moveInput;
    private bool isMoving;

    public void SetMoving(bool moving){ isMoving = moving; if (!moving) moveInput = Vector2.zero; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        isMoving = true;
    }

    private void Update()
    {
        if (!isMoving) return;
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        moveInput = new Vector2(x, y).normalized;
    }

    private void FixedUpdate()
    {
        if (!isMoving) return;
        rb.MovePosition(rb.position + new Vector3(moveInput.x, moveInput.y, 0) * moveSpeed * Time.fixedDeltaTime);
    }
}
