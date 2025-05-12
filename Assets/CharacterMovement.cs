using UnityEngine;
using UnityEngine.InputSystem; // Required for new Input System

[RequireComponent(typeof(CharacterController))]
public class CharacterMovement : MonoBehaviour
{
    // Movement parameters
    [Header("Movement Settings")]
    public float moveSpeed = 5.0f;
    public float runSpeed = 8.0f;
    public float rotationSpeed = 10.0f;
    public float jumpForce = 5.0f;
    public float gravity = 20.0f;
    public float groundCheckDistance = 0.1f;
    public float mouseSensitivity = 2.0f;

    // Component references
    private CharacterController characterController;
    private Transform cameraTransform;
    private Animator animator;
    private Actions characterActions;

    // Movement state variables
    private Vector3 moveDirection = Vector3.zero;
    private bool isGrounded;
    private float verticalVelocity = 0f;
    public Transform cameraPivot; // Assign in Inspector
    private float pitch = 0f;

    private void Start()
    {
        // Get required components
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            characterController = gameObject.AddComponent<CharacterController>();
            characterController.height = 2.0f;
            characterController.radius = 0.5f;
            characterController.center = new Vector3(0, 1.0f, 0);
        }

        cameraTransform = Camera.main.transform;
        animator = GetComponentInChildren<Animator>();
        characterActions = GetComponent<Actions>();

        // Reset initial velocity to ensure we start grounded
        verticalVelocity = -0.5f;

        // Lock and hide the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Check if character is grounded
        CheckGroundStatus();
        
        // Handle mouse rotation
        HandleMouseRotation();
        
        // Handle movement input
        HandleMovement();
        
        // Handle jumping
        HandleJumping();
        
        // Apply gravity
        ApplyGravity();
        
        // Move the character
        characterController.Move(moveDirection * Time.deltaTime);
        
        // Update animations
        UpdateAnimationsWithActions();
    }

    private void CheckGroundStatus()
    {
        // First use the character controller's isGrounded
        isGrounded = characterController.isGrounded;
        
        // Double-check with a raycast for more accuracy
        if (!isGrounded)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out hit, 
                               characterController.height / 2 + groundCheckDistance))
            {
                isGrounded = true;
            }
        }
        
        // Force-reset vertical velocity if grounded to prevent floating
        if (isGrounded && verticalVelocity > 0)
        {
            verticalVelocity = -0.5f;
        }
    }

    private void HandleMouseRotation()
    {
        float mouseX = Mouse.current.delta.ReadValue().x * mouseSensitivity;
        float mouseY = Mouse.current.delta.ReadValue().y * mouseSensitivity;

        // Horizontal: rotate the player (yaw)
        transform.Rotate(Vector3.up * mouseX);

        // Vertical: rotate the camera pivot (pitch)
        if (cameraPivot != null)
        {
            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, -80f, 80f);
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }

    private void HandleMovement()
    {
        // Get forward movement input
        bool isMovingForward = Keyboard.current != null && Keyboard.current.wKey.isPressed;
        
        // Check if running (shift key or gamepad button)
        bool isRunning = false;
        if (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed)
            isRunning = true;
        if (Gamepad.current != null && Gamepad.current.buttonEast.isPressed)
            isRunning = true;
            
        float currentSpeed = isRunning ? runSpeed : moveSpeed;
        
        if (isMovingForward)
        {
            // Move in the direction the character is facing
            moveDirection = transform.forward * currentSpeed;
        }
        else
        {
            // Not moving horizontally
            moveDirection.x = 0;
            moveDirection.z = 0;
        }
    }

    private void HandleJumping()
    {
        if (isGrounded)
        {
            // Reset vertical velocity when grounded
            if (verticalVelocity > 0)
                verticalVelocity = -0.5f;
            
            // Jump when space is pressed
            bool jumpPressed = false;
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                jumpPressed = true;
            if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
                jumpPressed = true;
                
            if (jumpPressed)
            {
                verticalVelocity = jumpForce;
                
                // Use Actions script for animation if available
                if (characterActions != null)
                {
                    characterActions.Jump();
                }
                else if (animator != null)
                {
                    animator.SetTrigger("Jump");
                }
            }
        }
        
        // Apply vertical velocity to movement
        moveDirection.y = verticalVelocity;
    }

    private void ApplyGravity()
    {
        if (!isGrounded)
        {
            // Apply gravity when in the air
            verticalVelocity -= gravity * Time.deltaTime;
            
            // Add a terminal velocity cap to prevent excessive speed
            if (verticalVelocity < -20.0f)
                verticalVelocity = -20.0f;
        }
    }

    private void UpdateAnimationsWithActions()
    {
        // Use the Actions script if available
        if (characterActions != null)
        {
            float movementMagnitude = new Vector2(moveDirection.x, moveDirection.z).magnitude;
            
            if (movementMagnitude > 0.1f)
            {
                if (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed)
                    characterActions.Run();
                else
                    characterActions.Walk();
            }
            else
            {
                characterActions.Stay();
            }
        }
        // Fallback to direct animator control
        else if (animator != null)
        {
            float movementMagnitude = new Vector2(moveDirection.x, moveDirection.z).magnitude / runSpeed;
            
            // Only set the Speed parameter which should exist in your animator
            animator.SetFloat("Speed", movementMagnitude);
        }
    }
}