using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private Renderer[] characterRenderers; // Kéo các SkinnedMeshRenderer của nhân vật vào đây để ẩn khi vào FPP

    [Header("Switch Perspective Key")]
    [SerializeField] private KeyCode toggleViewKey = KeyCode.V;

    [Header("Third Person (TPP) Settings")]
    [SerializeField] private float tppDistance = 2.8f;
    [SerializeField] private float tppHeight = 0.5f;

    [Header("First Person (FPP) Settings")]
    [SerializeField] private float fppDistance = -0.1f;    // Độ lệch trước/sau
    [SerializeField] private float fppHeight = 0.5f;       // Tầm mắt nhân vật
    [SerializeField] private float fppWallPadding = 0.15f; // Khoảng đệm chống xuyên tường ở FPP

    [Header("Wall Collision Settings")]
    [SerializeField] private LayerMask obstacleLayer;      // Layer của tường / môi trường (loại trừ layer của Player)
    [SerializeField] private float cameraRadius = 0.15f;    // Bán kính hình cầu để quét va chạm
    [SerializeField] private float minCollisionDist = 0.2f; // Khoảng cách tối thiểu ở TPP
    [SerializeField] private float smoothSpeed = 15f;       // Tốc độ co dãn mượt của camera

    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity = 2f;

    [Header("Vertical Rotation")]
    [SerializeField] private float minVerticalAngle = -45f;
    [SerializeField] private float maxVerticalAngle = 45f;

    public bool IsFirstPerson { get; private set; } = false;

    private Camera cam;
    private float rotationX;
    private float rotationY;
    private float currentDistance;
    private bool isGameplayMode = true;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam != null)
        {
            // Giảm mặt phẳng cắt gần để chống lẹm tường khi dí sát mặt ở FPP
            cam.nearClipPlane = 0.01f;
        }

        Vector3 currentRotation = transform.eulerAngles;
        rotationX = currentRotation.x;
        rotationY = currentRotation.y;

        currentDistance = tppDistance;

        EnableGameplayInput();
        UpdateCharacterVisibility();
    }

    private void Update()
    {
        // Chỉ cho phép bấm đổi góc nhìn khi đang ở chế độ Gameplay
        if (isGameplayMode && Input.GetKeyDown(toggleViewKey))
        {
            IsFirstPerson = !IsFirstPerson;
            UpdateCharacterVisibility();
        }
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        if (isGameplayMode)
        {
            HandleMouseRotation();
        }

        FollowTarget();
    }

    private void HandleMouseRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Xoay ngang 360 độ
        rotationY += mouseX;

        // Xoay dọc nhưng bị giới hạn
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(
            rotationX,
            minVerticalAngle,
            maxVerticalAngle
        );
    }

    private void FollowTarget()
    {
        Quaternion rotation = Quaternion.Euler(rotationX, rotationY, 0f);

        if (IsFirstPerson)
        {
            // === XỬ LÝ GÓC NHÌN THỨ NHẤT (FPP) ===
            Vector3 eyeOrigin = target.position + Vector3.up * fppHeight;
            Vector3 desiredPos = eyeOrigin + rotation * new Vector3(0f, 0f, -fppDistance);
            Vector3 castDirection = desiredPos - eyeOrigin;
            float castDist = castDirection.magnitude;

            // Kiểm tra vật cản nếu offset ra trước
            if (castDist > 0.001f)
            {
                if (Physics.SphereCast(eyeOrigin, cameraRadius, castDirection.normalized, out RaycastHit hit, castDist, obstacleLayer))
                {
                    desiredPos = eyeOrigin + castDirection.normalized * Mathf.Max(0f, hit.distance - fppWallPadding);
                }
            }

            // Quét kiểm tra tường ngay phía trước ống kính
            Vector3 forwardDir = rotation * Vector3.forward;
            if (Physics.SphereCast(desiredPos, cameraRadius, forwardDir, out RaycastHit wallHit, fppWallPadding, obstacleLayer))
            {
                desiredPos -= forwardDir * (fppWallPadding - wallHit.distance);
            }

            transform.position = desiredPos;
            currentDistance = fppDistance;
        }
        else
        {
            // === XỬ LÝ GÓC NHÌN THỨ BA (TPP) ===
            Vector3 targetPosition = target.position + Vector3.up * tppHeight;
            float desiredDistance = tppDistance;
            Vector3 backDirection = rotation * Vector3.back;

            // Quét cản tường phía sau camera
            if (Physics.SphereCast(targetPosition, cameraRadius, backDirection, out RaycastHit hit, tppDistance, obstacleLayer))
            {
                desiredDistance = Mathf.Clamp(hit.distance, minCollisionDist, tppDistance);
            }

            currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * smoothSpeed);

            Vector3 offset = backDirection * currentDistance;
            transform.position = targetPosition + offset;
        }

        transform.rotation = rotation;
    }

    private void UpdateCharacterVisibility()
    {
        // Ẩn model nhân vật ở góc nhìn thứ nhất để không che camera
        if (characterRenderers != null)
        {
            foreach (var rend in characterRenderers)
            {
                if (rend != null)
                    rend.enabled = !IsFirstPerson;
            }
        }
    }

    public void EnableGameplayInput()
    {
        isGameplayMode = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void EnableUIInput()
    {
        isGameplayMode = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}