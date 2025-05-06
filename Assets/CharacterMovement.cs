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

    // Component references
    private CharacterController characterController;
    private Transform cameraTransform;
    private Animator animator;
    private Actions characterActions;

    // Movement state variables
    private Vector3 moveDirection = Vector3.zero;
    private bool isGrounded;
    private float verticalVelocity = 0f;

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
    }

    private void Update()
    {
        // Check if character is grounded
        CheckGroundStatus();
        
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

    private void HandleMovement()
    {
        // Get input values using new Input System
        Vector2 inputVector = Vector2.zero;
        
        // Read keyboard input
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) inputVector.y += 1;
            if (Keyboard.current.sKey.isPressed) inputVector.y -= 1;
            if (Keyboard.current.aKey.isPressed) inputVector.x -= 1;
            if (Keyboard.current.dKey.isPressed) inputVector.x += 1;
        }
        
        // Read gamepad input if available
        if (Gamepad.current != null)
        {
            inputVector += Gamepad.current.leftStick.ReadValue();
        }
        
        // Check if running (shift key or gamepad button)
        bool isRunning = false;
        if (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed)
            isRunning = true;
        if (Gamepad.current != null && Gamepad.current.buttonEast.isPressed)
            isRunning = true;
            
        float currentSpeed = isRunning ? runSpeed : moveSpeed;
        
        // Calculate movement direction relative to camera
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        
        // Keep the vectors parallel to the ground
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();
        
        // Create the movement direction vector
        Vector3 desiredMoveDirection = forward * inputVector.y + right * inputVector.x;
        
        // If moving, update move direction and rotate character
        if (desiredMoveDirection.magnitude > 0.1f)
        {
            // Normalize for consistent movement speed in all directions
            desiredMoveDirection.Normalize();
            
            // Rotate character to face movement direction
            Quaternion targetRotation = Quaternion.LookRotation(desiredMoveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            
            // Set movement direction
            moveDirection.x = desiredMoveDirection.x * currentSpeed;
            moveDirection.z = desiredMoveDirection.z * currentSpeed;
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
            
            animator.SetFloat("Speed", movementMagnitude);
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetBool("IsRunning", movementMagnitude > 0.5f && 
                            (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed));
        }
    }
}