using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController),typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    
    [SerializeField]
    private float playerSpeed = 2.0f;
    [SerializeField]
    private float jumpHeight = 1.0f;
    [SerializeField]
    private float gravityValue = -9.81f;
    [SerializeField] 
    private float rotationSpeed = 3f;


    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;
    private Transform cam;
    private Animator animator;

    private void Start()
    {
        controller = gameObject.GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        cam = Camera.main.transform;
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        animator.SetBool("isGrounded",true);
    }

    void Update()
    {
        groundedPlayer = controller.isGrounded;
        animator.SetBool("isGrounded",groundedPlayer);
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        move = move.x * cam.right.normalized + move.z * cam.forward.normalized;
        move.y = 0f;
        controller.Move(move * Time.deltaTime * playerSpeed);
    
        animator.SetFloat("VelocityX",moveInput.x,0.1f,Time.deltaTime);
        animator.SetFloat("VelocityZ",moveInput.y,0.1f,Time.deltaTime);

        // Changes the height position of the player..
        if (jumpAction.triggered && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
            animator.SetBool("isGrounded",false);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        Quaternion targetRot = Quaternion.Euler(0,cam.eulerAngles.y,0);
        transform.rotation = Quaternion.Lerp(transform.rotation,targetRot, rotationSpeed * Time.deltaTime);
    }
    
}