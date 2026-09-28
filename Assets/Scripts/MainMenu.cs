using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Tên Scene Màn Chơi")]
    [SerializeField] private string gameplaySceneName = "URP_Rooftop_Market";

    [Header("Quản Lý Giao Diện")]
    [SerializeField] private GameObject mainMenuGroup; // Kéo MainMenuGroup vào đây
    [SerializeField] private GameObject settingsPanel; // Kéo SettingsPanel vào đây

    [Header("Hiệu Ứng Phản Hồi (Loading)")]
    [SerializeField] private GameObject loadingText; // Kéo LoadingPanel hoặc LoadingText vào
    [SerializeField] private Slider loadingSlider;    // Kéo LoadingSlider vào đây
    [SerializeField] private TextMeshProUGUI loadingPercentageText; // Kéo Text % vào đây

    [Header("Màn Hình Dẫn Chuyện (Intro)")]
    [SerializeField] private GameObject introPanel;              // Kéo IntroPanel vào đây
    [SerializeField] private TextMeshProUGUI storyText;          // Kéo StoryText vào đây
    [SerializeField] private GameObject continueToGameButton;    // Kéo nút ContinueToGameButton vào đây
    [TextArea(4, 10)]
    [SerializeField] private string storyContent = "Năm 2088. Thành phố mưa không dứt dưới những vệt đèn Neon nhòe nhoẹt...\n\nMột cuộc gọi khẩn cấp từ chợ tầng thượng Rooftop Market báo cáo về một xác chết bất thường.\n\nHank, đã đến lúc anh phải tìm ra sự thật đằng sau vụ án này.";
    [SerializeField] private float typingSpeed = 0.04f;          // Tốc độ hiện chữ từng ký tự

    private bool isStarting = false;
    private Coroutine typingCoroutine;

    // Khi người chơi bấm nút Play hoặc Continue
    public void ContinueGame() => StartIntroOrGame();
    public void PlayGame() => StartIntroOrGame();

    private void StartIntroOrGame()
    {
        if (isStarting) return;

        // 1. Tắt Menu chính
        if (mainMenuGroup != null) mainMenuGroup.SetActive(false);

        // 2. Kiểm tra nếu có màn hình dẫn chuyện thì bật lên
        if (introPanel != null)
        {
            introPanel.SetActive(true);

            // Tạm ẩn nút tiếp tục cho đến khi chữ gõ xong
            if (continueToGameButton != null) 
                continueToGameButton.SetActive(false);

            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeStoryRoutine());
        }
        else
        {
            // Nếu không gắn IntroPanel thì chuyển thẳng sang nạp cảnh
            ProceedToLoadingScene();
        }
    }

    // Hiệu ứng chữ chạy từng ký tự kiểu máy đánh chữ/terminal
    private IEnumerator TypeStoryRoutine()
    {
        if (storyText != null)
        {
            storyText.text = "";
            foreach (char letter in storyContent.ToCharArray())
            {
                storyText.text += letter;
                yield return new WaitForSeconds(typingSpeed);
            }
        }

        // Hiện chữ xong thì kích hoạt nút bấm tiếp tục
        if (continueToGameButton != null)
            continueToGameButton.SetActive(true);
    }

    // Hàm gọi khi bấm nút Tiếp tục ở IntroPanel (PROCEED TO SCENE)
    public void OnClickContinueToGame()
    {
        if (introPanel != null) introPanel.SetActive(false);
        ProceedToLoadingScene();
    }

    // Bắt đầu quá trình nạp map ngầm (Loading logic cũ)
    private void ProceedToLoadingScene()
    {
        isStarting = true;

        if (loadingText != null) loadingText.SetActive(true);

        StartCoroutine(LoadSceneAsyncProcess());
    }

    private IEnumerator LoadSceneAsyncProcess()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(gameplaySceneName);
        operation.allowSceneActivation = false; // Ngăn chuyển cảnh đột ngột

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

            // Khi tài nguyên map đã nạp xong hoàn toàn trong nền
            if (operation.progress >= 0.9f)
            {
                if (loadingSlider != null) loadingSlider.value = 1f;
                if (loadingPercentageText != null) loadingPercentageText.text = "Loading... 100%";

                yield return new WaitForSeconds(0.4f); // Giữ lại 0.4s để thấy 100%
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