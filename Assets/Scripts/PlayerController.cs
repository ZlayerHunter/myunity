using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed = 2.0f;
    private float defaultSpeed;
    public float rotationSpeed = 120.0f;
    public float jumpForce = 5.0f;
    public float crouchSpeedMultiplier = 0.5f;

    private Rigidbody rb;
    private Animator animator;
    private PlayerInputActions inputActions;
    private Vector2 movementInput;
    private bool isCrouch = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        defaultSpeed = speed;

        inputActions = new PlayerInputActions();

        // Движение
        inputActions.Player.Move.performed += OnMovePerformed;
        inputActions.Player.Move.canceled += OnMoveCanceled;

        // Приседание
        inputActions.Player.Crouch.performed += OnCrouchPerformed;
        inputActions.Player.Crouch.canceled += OnCrouchCanceled;

        // Прыжок
        inputActions.Player.Jump.performed += OnJumpPerformed;

        // Способ ходьбы (Sprint и Walk)
        inputActions.Player.Sprint.performed += ctx => {
            animator.SetBool("isRun", true);
            speed = defaultSpeed * 5;
        };
        inputActions.Player.Sprint.canceled += ctx => {
            animator.SetBool("isRun", false);
            ResetSpeed();
        };

        inputActions.Player.Walk.performed += ctx => animator.SetBool("isSlowWalking", true);
        inputActions.Player.Walk.canceled += ctx => animator.SetBool("isSlowWalking", false);
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        movementInput = Vector2.zero;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        animator.SetTrigger("isJump");
        rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
    }

    private void OnCrouchPerformed(InputAction.CallbackContext context)
    {
        animator.SetBool("isCrouch", true);
        isCrouch = true;
        speed = defaultSpeed * crouchSpeedMultiplier;
    }

    private void OnCrouchCanceled(InputAction.CallbackContext context)
    {
        animator.SetBool("isCrouch", false);
        isCrouch = false;
        ResetSpeed();
    }

    private void ResetSpeed()
    {
        // Если мы больше не бежим и не сидим — возвращаем стандартную скорость
        if (!inputActions.Player.Sprint.inProgress && !isCrouch)
        {
            speed = defaultSpeed;
        }
        else if (isCrouch)
        {
            speed = defaultSpeed * crouchSpeedMultiplier;
        }
    }

    private void Update()
    {
        bool isMoving = movementInput.magnitude > 0.1f;
        if (animator.GetBool("isWalk") != isMoving)
            animator.SetBool("isWalk", isMoving);
    }

    private void FixedUpdate()
    {
        Vector3 movement = transform.forward * movementInput.y * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);

        float turn = movementInput.x * rotationSpeed * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);
    }
}
