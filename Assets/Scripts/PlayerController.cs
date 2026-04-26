using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour
{
    [SerializeField] private PlayerStats stats;
    [SerializeField] private Transform cameraTransform;

    private CharacterController controller;
    private Vector3 moveInput;
    private Vector3 velocity;

    private void Start()
    {
        controller = GetComponent<CharacterController>();

        //Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        HandleMovement();
        ApplyGravity();
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        // Debug.Log($"Move input: {moveInput}");
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(stats.jumpHeight * -2f * stats.gravity);
        }
    }

    public void Sprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            stats.moveSpeed = stats.maxMoveSpeed;
        }
        else if (context.canceled)
        {
            stats.moveSpeed = 3f; // Reset to normal speed when sprinting stops
        }
    }

    private void HandleMovement()
    {
        Vector3 moveDirection = GetMoveDirection();
        controller.Move(moveDirection * stats.moveSpeed * Time.deltaTime);

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            RotatePlayer(moveDirection);
        }
    }

    private Vector3 GetMoveDirection()
    {
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
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += stats.gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}