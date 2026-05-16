using UnityEngine;
using UnityEngine.InputSystem;

// Controla movimento basico do jogador com CharacterController.
public class PlayerControl : MonoBehaviour
{
    [SerializeField] private PlayerStats stats;
    [SerializeField] private Transform cameraTransform;

    private CharacterController controller;
    private Vector3 moveInput;
    private Vector3 velocity;

    private void Start()
    {
        // Cache do CharacterController.
        controller = GetComponent<CharacterController>();

        //Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        // Processa movimento e gravidade.
        HandleMovement();
        ApplyGravity();
    }

    public void Move(InputAction.CallbackContext context)
    {
        // Guarda o input de movimento no plano X/Y.
        moveInput = context.ReadValue<Vector2>();
        // Debug.Log($"Input de movimento: {moveInput}");
    }

    public void Jump(InputAction.CallbackContext context)
    {
        // Salta apenas se estiver no chao.
        if (context.performed && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(stats.jumpHeight * -2f * stats.gravity);
        }
    }

    public void Sprint(InputAction.CallbackContext context)
    {
        // Ajusta a velocidade enquanto o sprint esta ativo.
        if (context.performed)
        {
            stats.moveSpeed = stats.maxMoveSpeed;
        }
        else if (context.canceled)
        {
            stats.moveSpeed = 3f; // Volta a velocidade normal quando o sprint termina.
        }
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