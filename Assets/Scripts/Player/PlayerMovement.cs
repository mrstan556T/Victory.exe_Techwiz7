using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3.0f;
    public float runSpeed = 7.0f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    private CharacterController controller;
    private Animator animator;
    private Transform cameraTransform;
    private Vector3 verticalVelocity;
    private float jumpCooldown = 0f; // Bộ đếm ngăn isGrounded kích hoạt quá sớm

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // Lấy Transform của Camera chính
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        // 1. Quản lý thời gian chờ sau khi nhảy
        if (jumpCooldown > 0f)
        {
            jumpCooldown -= Time.deltaTime;
        }

        // Chỉ xác nhận tiếp đất khi không trong giai đoạn vừa bấm bật nhảy
        bool isGrounded = controller.isGrounded && jumpCooldown <= 0f;

        if (animator != null)
        {
            animator.SetBool("IsGrounded", isGrounded);
        }

        // Giữ lực tì nhẹ xuống sàn khi đã tiếp đất
        if (isGrounded && verticalVelocity.y < 0)
        {
            verticalVelocity.y = -2f;
        }

        // 2. Đọc phím bấm (New Input System)
        float horizontal = 0f;
        float vertical = 0f;
        bool isRunning = false;
        bool jumpPressed = false;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontal -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontal += 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) vertical += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) vertical -= 1f;

            isRunning = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
            jumpPressed = Keyboard.current.spaceKey.wasPressedThisFrame;
        }

        // 3. Xử lý nhảy
        if (jumpPressed && controller.isGrounded)
        {
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpCooldown = 0.2f; // Tạm ngắt isGrounded trong 0.2s để trigger Jump kịp kích hoạt

            if (animator != null)
            {
                animator.ResetTrigger("Jump");
                animator.SetTrigger("Jump");
            }
        }

        // 4. Di chuyển ngang bám theo góc nhìn Camera
        Vector3 inputDir = new Vector3(horizontal, 0f, vertical).normalized;

        if (inputDir.magnitude >= 0.1f)
        {
            Vector3 moveDirection;

            if (cameraTransform != null)
            {
                // Lấy hướng trước/phải của camera trên mặt phẳng ngang (bỏ qua độ nghiêng Y)
                Vector3 camForward = cameraTransform.forward;
                Vector3 camRight = cameraTransform.right;
                camForward.y = 0f;
                camRight.y = 0f;
                camForward.Normalize();
                camRight.Normalize();

                moveDirection = (camForward * inputDir.z + camRight * inputDir.x).normalized;
            }
            else
            {
                moveDirection = inputDir;
            }

            // Xoay nhân vật theo hướng di chuyển
            transform.forward = moveDirection;

            float currentSpeed = isRunning ? runSpeed : walkSpeed;
            float animSpeedValue = isRunning ? 2.0f : 1.0f;

            controller.Move(moveDirection * currentSpeed * Time.deltaTime);

            if (animator != null)
                animator.SetFloat("Speed", animSpeedValue);
        }
        else
        {
            if (animator != null)
                animator.SetFloat("Speed", 0f);
        }

        // 5. Áp dụng trọng lực rơi tự do
        verticalVelocity.y += gravity * Time.deltaTime;
        controller.Move(verticalVelocity * Time.deltaTime);
    }
}