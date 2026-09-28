using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ColorConnectPuzzle : MonoBehaviour
{
    [System.Serializable]
    public class PuzzleCell
    {
        public Button button;
        public Image background;
        public int colorId;      // 0: Trống, 1..N: ID màu tương ứng
        public bool isEndpoint;  // Điểm gốc cố định
    }

    [Header("Bảng Giao Diện")]
    [SerializeField] private GameObject puzzlePanel;
    [SerializeField] private List<PuzzleCell> cells = new List<PuzzleCell>();

    [Header("Xử Lý Xung Đột Hệ Thống")]
    [SerializeField] private GameObject dialogueUICanvas;         // Kéo DialogueUI vào đây để tự tắt khi giải đố
    [SerializeField] private MonoBehaviour playerCameraController; // Kéo script điều khiển camera/Player vào đây

    [Header("Bảng Màu (Color Palette)")]
    [SerializeField] private Color emptyCellColor = new Color(0.12f, 0.14f, 0.17f, 1f);
    [SerializeField] private List<Color> colorPalette = new List<Color>()
    {
        Color.clear,                                            // ID 0: Trống
        new Color(1.0f, 0.95f, 0.0f, 1.0f),                    // ID 1: Vàng chanh
        new Color(1.0f, 0.35f, 0.35f, 1.0f),                   // ID 2: Đỏ san hô
        new Color(0.0f, 0.85f, 0.2f, 1.0f),                    // ID 3: Xanh lá
        new Color(1.0f, 0.55f, 0.0f, 1.0f),                    // ID 4: Cam
        new Color(0.15f, 0.5f, 1.0f, 1.0f),                    // ID 5: Xanh lam
        new Color(0.68f, 0.26f, 0.8f, 1.0f),                   // ID 6: Tím
        new Color(0.0f, 0.9f, 1.0f, 1.0f),                     // ID 7: Xanh lơ
        new Color(0.6f, 0.1f, 0.1f, 1.0f),                     // ID 8: Nâu đỏ
        new Color(0.95f, 0.95f, 0.95f, 1.0f)                   // ID 9: Trắng
    };

    [Header("Phản Hồi Ngoài Map Khi Giải Xong")]
    [SerializeField] private Light targetLight;
    [SerializeField] private float lightIntensityOn = 4f;
    [SerializeField] private Color lightTurnOnColor = Color.cyan;

    private const int GRID_SIZE = 7;
    private int activeColor = 0;
    private bool isDragging = false;
    private bool isSolved = false;

    private void Start()
    {
        for (int i = 0; i < cells.Count; i++)
        {
            int index = i;
            if (cells[index].button != null)
            {
                CellEventBridge bridge = cells[index].button.gameObject.GetComponent<CellEventBridge>();
                if (bridge == null)
                {
                    bridge = cells[index].button.gameObject.AddComponent<CellEventBridge>();
                }
                bridge.Init(index, OnCellPointerDown, OnCellPointerEnter, OnCellPointerUp);
            }
        }

        InitBoardVisuals();
    }

    public void OpenPuzzle()
    {
        if (isSolved) return;
        if (puzzlePanel != null) puzzlePanel.SetActive(true);

        // 1. Tạm ẩn DialogueUI để tránh xung đột cản tia chuột Raycast
        if (dialogueUICanvas != null) dialogueUICanvas.SetActive(false);

        // 2. Tắt script điều khiển camera và mở chuột tự do
        if (playerCameraController != null) playerCameraController.enabled = false;
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ClosePuzzle()
    {
        if (puzzlePanel != null) puzzlePanel.SetActive(false);

        // 1. Phục hồi DialogueUI cho game
        if (dialogueUICanvas != null) dialogueUICanvas.SetActive(true);

        // 2. Phục hồi thời gian và góc nhìn nhân vật
        Time.timeScale = 1f;
        if (playerCameraController != null) playerCameraController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnCellPointerDown(int index)
    {
        if (isSolved || index < 0 || index >= cells.Count) return;

        PuzzleCell cell = cells[index];

        if (cell.isEndpoint || cell.colorId != 0)
        {
            activeColor = cell.colorId;
            isDragging = true;
        }
    }

    public void OnCellPointerEnter(int index)
    {
        if (isSolved || !isDragging || activeColor == 0 || index < 0 || index >= cells.Count) return;

        PuzzleCell cell = cells[index];

        // Không tô đè lên điểm Endpoint của màu khác
        if (cell.isEndpoint && cell.colorId != activeColor) return;

        if (!cell.isEndpoint)
        {
            cell.colorId = activeColor;
            UpdateCellVisual(cell);
            CheckWinCondition();
        }
    }

    public void OnCellPointerUp(int index)
    {
        isDragging = false;
        activeColor = 0;
    }

    private void InitBoardVisuals()
    {
        foreach (var cell in cells)
        {
            if (cell.isEndpoint)
            {
                if (cell.background != null)
                {
                    cell.background.color = GetColorById(cell.colorId);
                }
            }
            else
            {
                cell.colorId = 0;
                if (cell.background != null)
                {
                    cell.background.color = emptyCellColor;
                }
            }
        }
    }

    private void UpdateCellVisual(PuzzleCell cell)
    {
        if (cell.background == null) return;

        if (cell.colorId == 0)
        {
            cell.background.color = emptyCellColor;
        }
        else
        {
            cell.background.color = GetColorById(cell.colorId);
        }
    }

    private Color GetColorById(int id)
    {
        if (id >= 0 && id < colorPalette.Count)
        {
            return colorPalette[id];
        }
        return emptyCellColor;
    }

    private void CheckWinCondition()
    {
        if (cells.Count != GRID_SIZE * GRID_SIZE) return;

        // Kiểm tra xem tất cả các ô đã được phủ kín màu chưa
        for (int i = 0; i < cells.Count; i++)
        {
            if (cells[i].colorId == 0) return;
        }

        // Lọc danh sách điểm gốc
        Dictionary<int, List<int>> colorEndpoints = new Dictionary<int, List<int>>();
        for (int i = 0; i < cells.Count; i++)
        {
            if (cells[i].isEndpoint)
            {
                int cId = cells[i].colorId;
                if (!colorEndpoints.ContainsKey(cId))
                {
                    colorEndpoints[cId] = new List<int>();
                }
                colorEndpoints[cId].Add(i);
            }
        }

        // BFS duyệt đường nối thông suốt từng cặp màu
        foreach (var pair in colorEndpoints)
        {
            if (pair.Value.Count != 2) return;

            if (!IsPathConnected(pair.Value[0], pair.Value[1], pair.Key))
            {
                return;
            }
        }

        OnPuzzleSolved();
    }

    private bool IsPathConnected(int start, int target, int colorId)
    {
        Queue<int> queue = new Queue<int>();
        HashSet<int> visited = new HashSet<int>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            int current = queue.Dequeue();
            if (current == target) return true;

            int row = current / GRID_SIZE;
            int col = current % GRID_SIZE;

            int[] dRow = { -1, 1, 0, 0 };
            int[] dCol = { 0, 0, -1, 1 };

            for (int i = 0; i < 4; i++)
            {
                int nRow = row + dRow[i];
                int nCol = col + dCol[i];

                if (nRow >= 0 && nRow < GRID_SIZE && nCol >= 0 && nCol < GRID_SIZE)
                {
                    int neighborIndex = nRow * GRID_SIZE + nCol;

                    if (!visited.Contains(neighborIndex) && cells[neighborIndex].colorId == colorId)
                    {
                        visited.Add(neighborIndex);
                        queue.Enqueue(neighborIndex);
                    }
                }
            }
        }

        return false;
    }

    private void OnPuzzleSolved()
    {
        isSolved = true;

        if (targetLight != null)
        {
            targetLight.color = lightTurnOnColor;
            targetLight.intensity = lightIntensityOn;
        }

        StartCoroutine(AutoCloseRoutine());
    }

    private System.Collections.IEnumerator AutoCloseRoutine()
    {
        yield return new WaitForSecondsRealtime(0.8f);
        ClosePuzzle();
    }
}

public class CellEventBridge : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerUpHandler
{
    private int cellIndex;
    private System.Action<int> onDown;
    private System.Action<int> onEnter;
    private System.Action<int> onUp;

    public void Init(int index, System.Action<int> downAction, System.Action<int> enterAction, System.Action<int> upAction)
    {
        cellIndex = index;
        onDown = downAction;
        onEnter = enterAction;
        onUp = upAction;
    }

    public void OnPointerDown(PointerEventData eventData) => onDown?.Invoke(cellIndex);
    public void OnPointerEnter(PointerEventData eventData) => onEnter?.Invoke(cellIndex);
    public void OnPointerUp(PointerEventData eventData) => onUp?.Invoke(cellIndex);
}