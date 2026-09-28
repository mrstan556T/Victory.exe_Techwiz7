using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Tên Scene Màn Chơi Mặc Định")]
    [SerializeField] private string defaultGameplayScene = "URP_Rooftop_Market";

    [Header("Hiệu Ứng Phản Hồi")]
    [SerializeField] private GameObject loadingText;
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private TextMeshProUGUI loadingPercentageText;

    [Header("Quản Lý Giao Diện")]
    [SerializeField] private GameObject mainMenuGroup;
    [SerializeField] private GameObject settingsPanel;

    private bool isStarting = false;

    // 1. NÚT CONTINUE (TIẾP TỤC TIẾN TRÌNH CŨ)
    public void ContinueGame()
    {
        if (isStarting) return;

        // Nếu có dữ liệu lưu thì tiếp tục, ngược lại chạy game mới
        if (PlayerPrefs.HasKey("SavedScene") && PlayerPrefs.HasKey("PlayerX"))
        {
            SaveManager.IsLoadingSavedGame = true; // Bật cờ chặn màn hình tối Intro
            string targetScene = PlayerPrefs.GetString("SavedScene");
            StartGameProcess(targetScene, true);
        }
        else
        {
            // Chưa có file save thì chạy lượt chơi mới
            PlayGame();
        }
    }

    // 2. NÚT PLAY / NEW GAME (CHƠI MỚI TỪ ĐẦU)
    public void PlayGame()
    {
        if (isStarting) return;

        // Xóa sạch vị trí cũ để không bị nhận nhầm
        PlayerPrefs.DeleteKey("PlayerX");
        PlayerPrefs.DeleteKey("PlayerY");
        PlayerPrefs.DeleteKey("PlayerZ");
        PlayerPrefs.DeleteKey("SavedScene");
        PlayerPrefs.Save();

        SaveManager.IsLoadingSavedGame = false; // Tắt cờ để Intro diễn ra bình thường

        StartGameProcess(defaultGameplayScene, false);
    }

    private void StartGameProcess(string sceneName, bool shouldLoadPos)
    {
        isStarting = true;

        if (mainMenuGroup != null) mainMenuGroup.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (loadingText != null) loadingText.SetActive(true);

        StartCoroutine(LoadSceneAsyncProcess(sceneName, shouldLoadPos));
    }

    private IEnumerator LoadSceneAsyncProcess(string sceneName, bool shouldLoadPos)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            if (loadingSlider != null)
            {
                loadingSlider.value = progress;
            }

            if (loadingPercentageText != null)
            {
                loadingPercentageText.text = $"Loading... {(progress * 100f):F0}%";
            }

            if (operation.progress >= 0.9f)
            {
                if (loadingSlider != null) loadingSlider.value = 1f;
                if (loadingPercentageText != null) loadingPercentageText.text = "Loading... 100%";

                yield return new WaitForSeconds(0.4f);

                // Nếu là Continue, báo cho SaveManager biết cần dịch chuyển nhân vật
                if (shouldLoadPos)
                {
                    SaveManager.TriggerPositionLoadOnNextScene();
                }

                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    public void OpenSettings()
    {
        if (mainMenuGroup != null) mainMenuGroup.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

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