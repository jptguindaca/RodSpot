using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// Controla movimento basico do jogador com CharacterController.
public class PlayerControl : NetworkBehaviour
{
    [SerializeField] private PlayerStats stats;
    [SerializeField] private Transform cameraTransform;

    private CharacterController controller;
    private Vector3 moveInput;
    private Vector3 cameraForward;
    private Vector3 velocity;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

    }
    [ServerRpc]
    public void JumpServerRpc()
    {
        if (controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(stats.jumpHeight * -2f * stats.gravity);

        }
    }
    [ServerRpc]
    private void MoveServerRpc(Vector2 input, Vector3 forward)
    {
        moveInput = input;

        cameraForward = forward;
    }
    [ServerRpc]
    private void SprintServerRpc(bool sprinting)
    {
        stats.moveSpeed = sprinting? stats.maxMoveSpeed : 3f;
    }
    

    private void Start()
    {
        // Cache do CharacterController.
        controller = GetComponent<CharacterController>();

        //Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        // Processa movimento e gravidade.

        if (!IsServer) { return; }

        HandleMovement();
        ApplyGravity();
      
      
       
    }
 
    public void Move(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        Vector2 input = context.ReadValue<Vector2>();

        Vector3 forward = cameraTransform.forward;

        MoveServerRpc(input, forward);
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (!IsOwner) { return; }

        if (context.performed)
        {
            JumpServerRpc();
        }
    }

    public void Sprint(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        if (context.performed)
            SprintServerRpc(true);

        if (context.canceled)
            SprintServerRpc(false);
    }

    private void HandleMovement()
    {
        // Move o jogador no plano da camera e roda para a direcao.
        Vector3 moveDirection = GetMoveDirection();
        controller.Move(moveDirection * stats.moveSpeed * Time.deltaTime);

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            RotatePlayer(moveDirection);
        }
    }

    private Vector3 GetMoveDirection()
    {
        // Converte input em direcao relativa a camera.
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        

        return forward * moveInput.y + right * moveInput.x;
        
    }

    private void RotatePlayer(Vector3 moveDirection)
    {
        // Alinha o jogador com a direcao de movimento.
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        // Aplica gravidade manual ao CharacterController.
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += stats.gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    public void StopMovement()
    {
        moveInput = Vector3.zero;
        velocity = Vector3.zero;
    }
}