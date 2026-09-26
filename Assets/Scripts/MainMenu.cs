using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Tên Scene Màn Chơi")]
    [SerializeField] private string gameplaySceneName = "URP_Rooftop_Market";

    [Header("Hiệu Ứng Phản Hồi")]
    [SerializeField] private GameObject loadingText;

    [Header("Quản Lý Giao Diện")]
    [SerializeField] private GameObject mainMenuGroup; // Kéo MainMenuGroup vào đây
    [SerializeField] private GameObject settingsPanel; // Kéo SettingsPanel vào đây

    private bool isStarting = false;

    public void ContinueGame() => StartGame();
    public void PlayGame() => StartGame();

    private void StartGame()
    {
        if (isStarting) return;
        isStarting = true;

        if (mainMenuGroup != null) mainMenuGroup.SetActive(false);
        if (loadingText != null) loadingText.SetActive(true);

        StartCoroutine(LoadSceneAsyncProcess());
    }

    private IEnumerator LoadSceneAsyncProcess()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(gameplaySceneName);
        while (!operation.isDone)
        {
            yield return null;
        }
    }

    // 1. Khi bấm Setting: TẮT MENU CHÍNH - BẬT BẢNG SETTING
    public void OpenSettings()
    {
        if (mainMenuGroup != null) mainMenuGroup.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    // 2. Khi bấm Quay lại / Đóng ở Setting: TẮT BẢNG SETTING - BẬT LẠI MENU CHÍNH
    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mainMenuGroup != null) mainMenuGroup.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}