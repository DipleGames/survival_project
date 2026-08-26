using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempPlayer : MonoBehaviour
{
    public float moveSpeed = 5f;

    [Header("Cache")]
    [SerializeField]Rigidbody rb;
    [SerializeField] SpriteRenderer spriteRenderer;

    Animator anim;
    [SerializeField]bool isLookingRight = false;
    Vector3 dir;
    float x;
    float z;

    PlayerState state;
    Coroutine interactionRoutine;
    InteractionData currentInteraction;
    IGatherable currentGatheringTarget;
    public enum PlayerState
    {
        Idle,
        Move,
        AutoMove,
        GatherInteracting
    }

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }
    private void Start()
    {
    }
    void Update()
    {
        switch (state)
        {
            case PlayerState.Idle:
            case PlayerState.Move:

                ReadMovementInput();

                break;

            case PlayerState.AutoMove:
            case PlayerState.GatherInteracting:

                if (HasMovementInput())
                {
                    CancelInteraction();
                }

                break;
        }
    }


    private void ReadMovementInput()
    {
        x = 0;
        z = 0;

        if (Input.GetKey((KeyCode)PlayerPrefs.GetInt("Key_Left")))
        {
            x = -1;
            isLookingRight = false;
        }
        else if (Input.GetKey((KeyCode)PlayerPrefs.GetInt("Key_Right")))
        {
            x = 1;
            isLookingRight = true;
        }

        if (Input.GetKey((KeyCode)PlayerPrefs.GetInt("Key_Up")))
        {
            z = 1;
        }
        else if (Input.GetKey((KeyCode)PlayerPrefs.GetInt("Key_Down")))
        {
            z = -1;
        }

        dir = new Vector3(x, 0, z).normalized;

        state = dir.sqrMagnitude > 0f
            ? PlayerState.Move
            : PlayerState.Idle;
    }
    bool HasMovementInput()
    {
        return
            Input.GetKey((KeyCode)PlayerPrefs.GetInt("Key_Left")) ||
            Input.GetKey((KeyCode)PlayerPrefs.GetInt("Key_Right")) ||
            Input.GetKey((KeyCode)PlayerPrefs.GetInt("Key_Up")) ||
            Input.GetKey((KeyCode)PlayerPrefs.GetInt("Key_Down"));
    }

    void FixedUpdate()
    {
        if (state == PlayerState.Move ||
           state == PlayerState.Idle)
        {
            Move();
        }
    }
    public void StartInteraction(
    InteractionData data,
    IGatherable target)
    {
        if (interactionRoutine != null)
            StopCoroutine(interactionRoutine);

        currentInteraction = data;
        currentGatheringTarget = target;

        interactionRoutine =
            StartCoroutine(InteractionRoutine());
    }
    IEnumerator InteractionRoutine()
    {
        state = PlayerState.AutoMove;

        while (Vector3.Distance(
            rb.position,
            currentInteraction.MovePosition) > 0.05f)
        {
            Vector3 dir =
                (currentInteraction.MovePosition - rb.position)
                .normalized;

            rb.MovePosition(
                rb.position +
                dir * moveSpeed * Time.fixedDeltaTime);

            anim.SetBool("Moving", true);

            yield return new WaitForFixedUpdate();
        }

        anim.SetBool("Moving", false);

        state = PlayerState.GatherInteracting;

        if (!string.IsNullOrEmpty(currentInteraction.AnimationName))
        {
            anim.SetBool(currentInteraction.AnimationName, true);
        }

        yield return new WaitForSeconds(currentInteraction.InteractionTime);

        if (!string.IsNullOrEmpty(currentInteraction.AnimationName))
        {
            anim.SetBool(currentInteraction.AnimationName, false);
        }

        currentGatheringTarget.EndInteraction();

        currentGatheringTarget = null;

        interactionRoutine = null;

        state = PlayerState.Idle;
    }
    public void CancelInteraction()
    {
        if (interactionRoutine != null)
        {
            StopCoroutine(interactionRoutine);
            interactionRoutine = null;
        }

        anim.SetBool("Moving", false);
        if (!string.IsNullOrEmpty(currentInteraction.AnimationName))
        {
            anim.SetBool(currentInteraction.AnimationName, false);
        }

        currentGatheringTarget?.CancelGathering();
        currentGatheringTarget = null;

        state = PlayerState.Idle;
    }
    void Move()
    {
        anim.SetBool("Moving", dir.sqrMagnitude > 0);

        spriteRenderer.flipX = isLookingRight;

        rb.MovePosition(
            rb.position +
            dir * moveSpeed * Time.fixedDeltaTime);
    }
}