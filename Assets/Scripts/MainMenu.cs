using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Tên Scene Màn Chơi")]
    [SerializeField] private string gameplaySceneName = "URP_Rooftop_Market";

    [Header("Hiệu Ứng Phản Hồi")]
    [SerializeField] private GameObject loadingText; // Biến cũ của bạn (kéo LoadingPanel hoặc LoadingText vào)
    [SerializeField] private Slider loadingSlider;    // Biến mới: Kéo LoadingSlider vào đây
    [SerializeField] private TextMeshProUGUI loadingPercentageText; // Biến mới (tùy chọn): Kéo LoadingText (nếu dùng TMP) vào để hiện số %

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
        operation.allowSceneActivation = false; // Ngăn chuyển cảnh đột ngột để hiển thị tiến trình mượt mà

        while (!operation.isDone)
        {
            // Unity nạp ngầm từ 0.0f đến 0.9f
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            // Cập nhật giá trị vào thanh Slider (từ 0 đến 1)
            if (loadingSlider != null)
            {
                loadingSlider.value = progress;
            }

            // Cập nhật chữ phần trăm nếu có gán
            if (loadingPercentageText != null)
            {
                loadingPercentageText.text = $"Loading... {(progress * 100f):F0}%";
            }

            // Khi tài nguyên map đã tải xong hoàn toàn ngầm
            if (operation.progress >= 0.9f)
            {
                if (loadingSlider != null) loadingSlider.value = 1f;
                if (loadingPercentageText != null) loadingPercentageText.text = "Loading... 100%";

                yield return new WaitForSeconds(0.4f); // Giữ lại 0.4s để người chơi thấy thanh đã đầy 100%
                operation.allowSceneActivation = true; // Kích hoạt vào Scene mới
            }

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