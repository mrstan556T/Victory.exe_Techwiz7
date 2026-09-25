using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Camera Settings")]
    [SerializeField] private float distance = 2f;
    [SerializeField] private float height = 0.5f;

    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity = 2f;

    [Header("Vertical Rotation")]
    [SerializeField] private float minVerticalAngle = -45f;
    [SerializeField] private float maxVerticalAngle = 45f;

    private float rotationX;
    private float rotationY;

    private void Start()
    {
        // Khóa chuột vào giữa màn hình
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Lấy rotation ban đầu của camera
        Vector3 currentRotation = transform.eulerAngles;

        rotationX = currentRotation.x;
        rotationY = currentRotation.y;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        HandleMouseRotation();
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

        Vector3 offset = rotation * new Vector3(0f, 0f, -distance);

        Vector3 targetPosition = target.position + Vector3.up * height;

        transform.position = targetPosition + offset;
        transform.rotation = rotation;
    }
}
