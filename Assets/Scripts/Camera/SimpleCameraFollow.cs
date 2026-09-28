using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleCameraFollow : MonoBehaviour
{
    [Header("Mục tiêu theo dõi")]
    public Transform target;                  // Kéo nhân vật vào đây
    public Renderer characterRenderer;       // Kéo Mesh của nhân vật vào đây để ẩn khi ở góc nhìn 1 (tránh bị che tầm nhìn)

    [Header("Góc nhìn thứ 3 (TP)")]
    public Vector3 tpOffset = new Vector3(0f, 1.4f, 0f);
    public float tpDistance = 4.0f;
    public float minDistance = 1.2f;
    public float maxDistance = 7.0f;

    [Header("Góc nhìn thứ nhất (FP)")]
    public Vector3 fpOffset = new Vector3(0f, 1.6f, 0.2f); // Ngay tầm mắt nhân vật

    [Header("Độ nhạy & Giới hạn góc xoay")]
    public float mouseSensitivityX = 1.5f;
    public float mouseSensitivityY = 1.0f;
    public float minYAngle = -40f;            // Cho phép cúi nhìn chân ở góc nhìn 1
    public float maxYAngle = 70f;

    [Header("Chế độ hiện tại")]
    public bool isFirstPerson = false;

    private float currentX = 0f;
    private float currentY = 15f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (target != null)
        {
            currentX = target.eulerAngles.y;
        }

        UpdateMeshVisibility();
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Nhận phím V để chuyển đổi góc nhìn
        if (Keyboard.current != null && Keyboard.current.vKey.wasPressedThisFrame)
        {
            ToggleViewMode();
        }

        // 2. Nhận tín hiệu chuột
        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            currentX += mouseDelta.x * mouseSensitivityX * 0.1f;
            currentY -= mouseDelta.y * mouseSensitivityY * 0.1f;
            currentY = Mathf.Clamp(currentY, minYAngle, maxYAngle);

            // Zoom lăn chuột
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                if (!isFirstPerson)
                {
                    tpDistance = Mathf.Clamp(tpDistance - scroll * 0.005f, minDistance, maxDistance);
                    // Lăn chuột sát mức tối thiểu sẽ tự chuyển sang góc nhìn thứ nhất
                    if (tpDistance <= minDistance && scroll > 0)
                    {
                        isFirstPerson = true;
                        UpdateMeshVisibility();
                    }
                }
                else if (scroll < 0) // Lăn lùi ra xa thì chuyển về góc nhìn thứ 3
                {
                    isFirstPerson = false;
                    tpDistance = minDistance + 0.5f;
                    UpdateMeshVisibility();
                }
            }
        }

        // 3. Tính toán vị trí Camera dựa theo chế độ
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0f);

        if (isFirstPerson)
        {
            // Camera đặt ngay tại mắt nhân vật
            Vector3 eyePos = target.position + target.rotation * fpOffset;
            transform.position = eyePos;
            transform.rotation = rotation;

            // Ở góc nhìn thứ nhất, thân người sẽ xoay ngay theo hướng ngang của camera
            target.rotation = Quaternion.Euler(0f, currentX, 0f);
        }
        else
        {
            // Camera góc nhìn thứ 3 quay quanh nhân vật
            Vector3 focusPoint = target.position + tpOffset;
            Vector3 desiredPosition = focusPoint - (rotation * Vector3.forward * tpDistance);

            transform.position = desiredPosition;
            transform.LookAt(focusPoint);
        }

        // Mở khóa chuột khi bấm Escape
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void ToggleViewMode()
    {
        isFirstPerson = !isFirstPerson;
        UpdateMeshVisibility();
    }

    private void UpdateMeshVisibility()
    {
        // Ẩn thân nhân vật ở góc nhìn thứ nhất để không bị mắt/đầu che màn hình
        if (characterRenderer != null)
        {
            characterRenderer.enabled = !isFirstPerson;
        }
    }
}