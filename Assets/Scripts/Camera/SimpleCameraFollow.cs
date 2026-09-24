using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleCameraFollow : MonoBehaviour
{
    [Header("Mục tiêu theo dõi")]
    public Transform target;                  // Kéo nhân vật vào đây

    [Header("Khoảng cách & Chiều cao")]
    public Vector3 targetOffset = new Vector3(0f, 1.4f, 0f); // Điểm nhìn (ngang ngực/đầu)
    public float distance = 4.0f;             // Khoảng cách từ camera tới nhân vật
    public float minDistance = 1.5f;
    public float maxDistance = 7.0f;

    [Header("Độ nhạy & Góc xoay chuột")]
    public float mouseSensitivityX = 1.5f;
    public float mouseSensitivityY = 1.0f;
    public float minYAngle = -10f;            // Không cho camera chúc quá sâu từ dưới lên
    public float maxYAngle = 60f;             // Giới hạn góc nhìn từ trên xuống

    [Header("Độ mượt")]
    public float smoothSpeed = 15f;

    private float currentX = 0f;
    private float currentY = 15f;             // Góc nhìn ban đầu hơi chếch nhẹ xuống

    void Start()
    {
        // Khóa và ẩn con trỏ chuột vào giữa màn hình khi bắt đầu chơi
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Nếu target đã có sẵn, lấy góc quay ban đầu theo lưng nhân vật
        if (target != null)
        {
            currentX = target.eulerAngles.y;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Nhận tín hiệu rê chuột (New Input System)
        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            currentX += mouseDelta.x * mouseSensitivityX * 0.1f;
            currentY -= mouseDelta.y * mouseSensitivityY * 0.1f;

            // Khóa góc ngước lên / nhìn xuống trong phạm vi an toàn
            currentY = Mathf.Clamp(currentY, minYAngle, maxYAngle);

            // Zoom bằng con lăn chuột
            float scroll = Mouse.current.scroll.ReadValue().y;
            distance = Mathf.Clamp(distance - scroll * 0.005f, minDistance, maxDistance);
        }

        // 2. Tính toán vị trí xoay theo góc cầu quanh nhân vật
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0f);
        Vector3 focusPoint = target.position + targetOffset;
        Vector3 desiredPosition = focusPoint - (rotation * Vector3.forward * distance);

        // 3. Cập nhật vị trí và hướng nhìn
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.LookAt(focusPoint);

        // 4. Mở khóa con trỏ chuột khi bấm phím Escape
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}