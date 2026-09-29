using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    public static bool IsLoadingSavedGame = false;
    private bool shouldLoadPosition = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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

    // 1. LƯU TIẾN TRÌNH
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

            Debug.Log($"<color=green>[SaveManager]</color> Đã lưu vị trí: {pos} tại Scene: {SceneManager.GetActiveScene().name}");
        }
        else
        {
            Debug.LogError("[SaveManager] Không tìm thấy đối tượng có Tag 'Player'!");
        }
    }

    // 2. KÍCH HOẠT DỊCH VỊ TRÍ TỪ MAIN MENU
    public static void TriggerPositionLoadOnNextScene()
    {
        if (Instance != null)
        {
            Instance.shouldLoadPosition = true;
        }
    }

    // 3. XỬ LÝ KHI SCENE VỪA LOAD XONG
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (shouldLoadPosition || IsLoadingSavedGame)
        {
            shouldLoadPosition = false;
            StartCoroutine(ApplySavedPositionRoutine());
        }
    }

    private IEnumerator ApplySavedPositionRoutine()
    {
        // Chờ 2 frames để đảm bảo map và player đã khởi tạo hoàn toàn
        yield return null;
        yield return new WaitForEndOfFrame();

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null && PlayerPrefs.HasKey("PlayerX"))
        {
            float x = PlayerPrefs.GetFloat("PlayerX");
            float y = PlayerPrefs.GetFloat("PlayerY");
            float z = PlayerPrefs.GetFloat("PlayerZ");
            Vector3 targetPos = new Vector3(x, y, z);

            // Tắt tạm thời các component vật lý để đặt tọa độ không bị cản
            CharacterController cc = player.GetComponent<CharacterController>();
            Rigidbody rb = player.GetComponent<Rigidbody>();

            if (cc != null) cc.enabled = false;
            if (rb != null) rb.isKinematic = true;

            player.transform.position = targetPos;

            yield return null; // Chờ 1 frame cập nhật vị trí mới

            if (cc != null) cc.enabled = true;
            if (rb != null) rb.isKinematic = false;

            Debug.Log($"<color=cyan>[SaveManager]</color> Đã dịch chuyển Player đến vị trí lưu: {targetPos}");
        }

        // Tắt cờ trạng thái sau khi đã định vị xong
        yield return new WaitForSeconds(0.2f);
        IsLoadingSavedGame = false;
    }
}