using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    private static SaveManager instance;
    private bool shouldLoadPosition = false;

    // Biến cờ toàn cục để các script khác (như Intro) biết người chơi đang Continue hay New Game
    public static bool IsLoadingSavedGame = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // --- HÀM LƯU TIẾN TRÌNH ---
    public static void SavePlayerGame()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Vector3 pos = player.transform.position;
            PlayerPrefs.SetFloat("PlayerX", pos.x);
            PlayerPrefs.SetFloat("PlayerY", pos.y);
            PlayerPrefs.SetFloat("PlayerZ", pos.z);
            PlayerPrefs.SetString("SavedScene", SceneManager.GetActiveScene().name);
            PlayerPrefs.Save();

            Debug.Log($"[SaveManager] Đã lưu game thành công tại vị trí: {pos}");
        }
        else
        {
            Debug.LogWarning("[SaveManager] Không tìm thấy GameObject có Tag là 'Player' để lưu!");
        }
    }

    // --- HÀM TIẾP TỤC GAME (CONTINUE) ---
    public static void RequestContinueGame()
    {
        if (PlayerPrefs.HasKey("SavedScene") && PlayerPrefs.HasKey("PlayerX"))
        {
            IsLoadingSavedGame = true; // Bật cờ để chặn màn hình Intro
            string savedScene = PlayerPrefs.GetString("SavedScene");
            if (instance != null) instance.shouldLoadPosition = true;
            SceneManager.LoadScene(savedScene);
        }
        else
        {
            // Nếu chưa có file lưu, chạy màn chơi mới bình thường
            IsLoadingSavedGame = false;
            SceneManager.LoadScene("URP_Rooftop_Market");
        }
    }

    public static void TriggerPositionLoadOnNextScene()
    {
        if (instance != null)
        {
            instance.shouldLoadPosition = true;
        }
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (shouldLoadPosition)
        {
            shouldLoadPosition = false;
            StartCoroutine(DelayedLoadPosition());
        }
    }

    private IEnumerator DelayedLoadPosition()
    {
        // Chờ 1 frame đảm bảo các object trong Scene khởi tạo xong
        yield return null;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null && PlayerPrefs.HasKey("PlayerX"))
        {
            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false; // Tắt tạm để dịch chuyển mượt mà

            float x = PlayerPrefs.GetFloat("PlayerX");
            float y = PlayerPrefs.GetFloat("PlayerY");
            float z = PlayerPrefs.GetFloat("PlayerZ");

            player.transform.position = new Vector3(x, y, z);

            if (controller != null) controller.enabled = true; // Bật lại controller

            Debug.Log($"[SaveManager] Đã nạp thành công vị trí cũ của nhân vật: {player.transform.position}");
        }

        // Chờ thêm 1 khoảng ngắn sau khi định vị rồi trả cờ về false
        yield return new WaitForSeconds(0.2f);
        IsLoadingSavedGame = false;
    }
}