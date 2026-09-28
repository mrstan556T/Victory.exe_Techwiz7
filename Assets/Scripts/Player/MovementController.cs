using UnityEngine;
using System.Collections;

public class MovementController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 0.7f;
    [SerializeField] private float sprintSpeed = 1.8f;
    [SerializeField] private float rotationSpeed = 720f;

    [Header("Jumping")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private CameraController cameraController; // Kéo Camera vào đây hoặc để trống script tự tìm
    [SerializeField] private Animator animator;

    private Rigidbody rb;
    private float horizontalInput;
    private float verticalInput;
    private bool isSprinting;
    private bool isJumping;
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Tự động tìm CameraController nếu chưa gán trên Inspector
        if (cameraController == null && cameraTransform != null)
        {
            cameraController = cameraTransform.GetComponent<CameraController>();
        }
        else if (cameraController == null && Camera.main != null)
        {
            cameraController = Camera.main.GetComponent<CameraController>();
            if (cameraTransform == null)
            {
                cameraTransform = Camera.main.transform;
            }
        }
    }

    private void Update()
    {
        // Chặn di chuyển khi đang mở hội thoại
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
        {
            horizontalInput = 0f;
            verticalInput = 0f;
            UpdateAnimation();
            return;
        }

        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        HandleRotation();
        GroundCheck();
        HandleJump();
        HandleSprint();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (cameraTransform == null) return;

        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 moveDirection = cameraForward * verticalInput + cameraRight * horizontalInput;
        moveDirection.Normalize();

        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;
        Vector3 targetVelocity = moveDirection * currentSpeed;

        rb.linearVelocity = new Vector3(
            targetVelocity.x,
            rb.linearVelocity.y,
            targetVelocity.z
        );
    }

    private void HandleRotation()
    {
        if (cameraTransform == null) return;

        // Nếu đang ở góc nhìn thứ nhất (FPP): Thân người luôn quay thẳng theo hướng nhìn ngang của camera
        if (cameraController != null && cameraController.IsFirstPerson)
        {
            Vector3 camForward = cameraTransform.forward;
            camForward.y = 0f;
            if (camForward.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(camForward);
            }
            return;
        }

        // Nếu ở góc nhìn thứ ba (TPP): Xoay nhân vật theo hướng bấm nút di chuyển
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 moveDirection = cameraForward * verticalInput + cameraRight * horizontalInput;

        if (moveDirection.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void HandleSprint()
    {
        bool isMoving = horizontalInput != 0f || verticalInput != 0f;
        isSprinting = Input.GetKey(KeyCode.LeftShift) && isMoving;
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isJumping)
        {
            StartCoroutine(JumpRoutine());
        }
    }

    private IEnumerator JumpRoutine()
    {
        isJumping = true;
        if (animator != null)
        {
            animator.SetTrigger("jumping");
        }

        yield return new WaitForSeconds(0.62f);

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;

        bool isMoving = horizontalInput != 0f || verticalInput != 0f;
        animator.SetBool("isWalking", isMoving);
        animator.SetBool("isSprinting", isSprinting);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            isJumping = false;
        }
    }

    private void GroundCheck()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, groundCheckDistance, groundLayer);
        }
    }
}