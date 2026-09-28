using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Tốc độ di chuyển")]
    public float walkSpeed = 3.0f;
    public float runSpeed = 7.0f;
    public float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;

    [Header("Nhảy & Trọng lực")]
    public float jumpHeight = 1.5f;
    public float gravity = -20f;
    public float groundCheckDistance = 0.2f;

    private CharacterController controller;
    private Animator animator;
    private Transform cameraTransform;
    private SimpleCameraFollow camScript;
    private float verticalVelocityY;
    private float jumpCooldown = 0f;

    // Biến lưu pháp tuyến bề mặt va chạm để trượt
    private Vector3 contactNormal = Vector3.up;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
            camScript = cameraTransform.GetComponent<SimpleCameraFollow>();
        }
        else
        {
            Camera cam = FindAnyObjectByType<Camera>();
            if (cam != null)
            {
                cameraTransform = cam.transform;
                camScript = cameraTransform.GetComponent<SimpleCameraFollow>();
            }
        }
    }

    void Update()
    {
        if (jumpCooldown > 0f) jumpCooldown -= Time.deltaTime;

        // 1. Kiểm tra tiếp đất kết hợp Raycast ở chân
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y + controller.radius, transform.position.z);
        bool physicallyGrounded = Physics.CheckSphere(spherePosition, controller.radius + groundCheckDistance, ~0, QueryTriggerInteraction.Ignore);
        
        bool isGrounded = (controller.isGrounded || physicallyGrounded) && jumpCooldown <= 0f;

        if (animator != null) animator.SetBool("IsGrounded", isGrounded);

        if (isGrounded && verticalVelocityY < 0)
        {
            verticalVelocityY = -5f; // Tì lực chắc chắn xuống sàn
        }

        // 2. Nhận tín hiệu phím
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

        // 3. Logic Nhảy
        if (jumpPressed && isGrounded)
        {
            verticalVelocityY = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpCooldown = 0.25f;

            if (animator != null)
            {
                animator.ResetTrigger("Jump");
                animator.SetTrigger("Jump");
            }
        }

        // 4. Tính toán hướng di chuyển
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        Vector3 horizontalMove = Vector3.zero;

        if (direction.magnitude >= 0.1f)
        {
            float camY = 0f;
            if (cameraTransform != null) camY = cameraTransform.eulerAngles.y;

            bool isFirstPerson = camScript != null && camScript.isFirstPerson;

            if (isFirstPerson && cameraTransform != null)
            {
                Vector3 forward = cameraTransform.forward;
                Vector3 right = cameraTransform.right;
                forward.y = 0f;
                right.y = 0f;
                horizontalMove = (forward.normalized * vertical + right.normalized * horizontal).normalized;
            }
            else
            {
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + camY;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                horizontalMove = (Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward).normalized;
            }

            // TRƯỢT TƯỜNG: Chiếu vector di chuyển lên mặt phẳng va chạm
            // Giúp triệt tiêu lực đâm thẳng xuyên vào vật thể và ép trượt men theo cạnh
            if (contactNormal != Vector3.up && Vector3.Dot(horizontalMove, contactNormal) < 0)
            {
                horizontalMove = Vector3.ProjectOnPlane(horizontalMove, contactNormal).normalized;
            }

            float currentSpeed = isRunning ? runSpeed : walkSpeed;
            horizontalMove *= currentSpeed;

            if (animator != null) animator.SetFloat("Speed", isRunning ? 2.0f : 1.0f);
        }
        else
        {
            if (animator != null) animator.SetFloat("Speed", 0f);
        }

        // 5. Trọng lực
        verticalVelocityY += gravity * Time.deltaTime;

        // 6. Gộp chuyển động và đảm bảo trục Y không bị lực ngang can thiệp
        Vector3 finalVelocity = new Vector3(horizontalMove.x, verticalVelocityY, horizontalMove.z);

        controller.Move(finalVelocity * Time.deltaTime);

        // Reset pháp tuyến sau mỗi frame
        contactNormal = Vector3.up;
    }

    // Bắt va chạm với bất kỳ bề mặt nào mà nhân vật tiếp xúc
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        contactNormal = hit.normal;

        // Nếu va chạm với vật cản đứng (tường, mép đồ vật)
        if (hit.normal.y < 0.5f)
        {
            // Ngăn chặn hoàn toàn việc nâng độ cao Y ngoài ý muốn khi ép sát vật thể
            if (verticalVelocityY > 0 && !controller.isGrounded && jumpCooldown <= 0f)
            {
                verticalVelocityY = 0f;
            }
        }
    }
}