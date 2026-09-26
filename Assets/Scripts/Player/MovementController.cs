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
    }
    private void Update()
    {
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
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 moveDirection =
            cameraForward * verticalInput +
            cameraRight * horizontalInput;

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
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 moveDirection =
            cameraForward * verticalInput +
            cameraRight * horizontalInput;

        if (moveDirection.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(moveDirection);

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
        animator.SetTrigger("jumping");

        yield return new WaitForSeconds(0.62f);

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        isGrounded = false;
    }

    private void UpdateAnimation()
    {
        bool isWalking = horizontalInput != 0f || verticalInput != 0f;
        animator.SetBool("isWalking", isWalking);

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
        isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, groundCheckDistance, groundLayer);
    }
}
