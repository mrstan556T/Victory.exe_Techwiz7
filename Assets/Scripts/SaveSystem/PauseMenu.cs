using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private GameObject pauseMenuUI; // Kéo Panel Pause Menu vào đây

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // Điền đúng tên Scene Menu chính

    public static bool isPaused = false;

    void Start()
    {
        // Đảm bảo ban đầu tắt menu và thời gian game chạy bình thường
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    void Update()
    {
        // Bắt phím Escape bằng New Input System
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    // 1. Nút CONTINUE (Tiếp tục)
    public void Resume()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        Time.timeScale = 1f; // Tiếp tục thời gian game
        isPaused = false;

        // Khóa lại con trỏ chuột cho góc nhìn gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Tạm dừng game và hiện menu
    public void Pause()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // Đóng băng mọi chuyển động vật lý/gameplay
        isPaused = true;

        // Mở khóa chuột để click nút UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // 2. Nút SAVE GAME
    public void SaveGameButton()
    {
        SaveManager.SavePlayerGame();
        Debug.Log("Đã lưu tiến trình từ Pause Menu!");
    }

    // 3. Nút MAIN MENU
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f; // Bắt buộc trả timeScale về 1 trước khi đổi cảnh
        SceneManager.LoadScene(mainMenuSceneName);
    }
}