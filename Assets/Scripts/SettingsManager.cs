using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TMP_Dropdown fpsDropdown;
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    private void Start()
    {
        LoadSettings();
    }

    // 1. Chỉnh âm lượng (0.0 đến 1.0)
    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    // 2. Chỉnh giới hạn FPS (Index: 0 = 30, 1 = 60, 2 = 120, 3 = Không giới hạn)
    public void SetFPS(int index)
    {
        int targetFPS = -1;
        switch (index)
        {
            case 0: targetFPS = 30; break;
            case 1: targetFPS = 60; break;
            case 2: targetFPS = 120; break;
            case 3: targetFPS = -1; break; // Không giới hạn
        }

        Application.targetFrameRate = targetFPS;
        PlayerPrefs.SetInt("FPSIndex", index);
    }

    // 3. Chỉnh chất lượng đồ họa
    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("QualitySetting", qualityIndex);
    }

    // 4. Chỉnh toàn màn hình / Cửa sổ
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }

    // Tải lại cài đặt người chơi đã lưu từ lần chơi trước
    private void LoadSettings()
    {
        // Tải âm lượng
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        if (volumeSlider != null) volumeSlider.value = savedVolume;
        AudioListener.volume = savedVolume;

        // Tải FPS
        int savedFPSIndex = PlayerPrefs.GetInt("FPSIndex", 1); // Mặc định 60 FPS
        if (fpsDropdown != null) fpsDropdown.value = savedFPSIndex;
        SetFPS(savedFPSIndex);

        // Tải Quality
        int savedQuality = PlayerPrefs.GetInt("QualitySetting", QualitySettings.GetQualityLevel());
        if (qualityDropdown != null) qualityDropdown.value = savedQuality;
        QualitySettings.SetQualityLevel(savedQuality);

        // Tải Fullscreen
        bool savedFullscreen = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;
        if (fullscreenToggle != null) fullscreenToggle.isOn = savedFullscreen;
        Screen.fullScreen = savedFullscreen;
    }
}