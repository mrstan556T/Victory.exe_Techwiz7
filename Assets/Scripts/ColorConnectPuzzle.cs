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
        public bool isEndpoint;  // Điểm gốc ban đầu
    }

    [Header("Bảng Giao Diện")]
    [SerializeField] private GameObject puzzlePanel;
    [SerializeField] private List<PuzzleCell> cells = new List<PuzzleCell>();

    [Header("Xử Lý Xung Đột Hệ Thống")]
    [SerializeField] private GameObject dialogueUICanvas;         // Kéo đối tượng DialogueUI vào đây
    [SerializeField] private MonoBehaviour playerCameraController; // Kéo script xoay camera của nhân vật vào đây

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
    [SerializeField] private float lightIntensityOn = 15f;
    [SerializeField] private Color lightTurnOnColor = Color.cyan;

    private const int GRID_SIZE = 7;
    private int activeColor = 0;
    private bool isDragging = false;
    private bool isSolved = false;

    // Lưu trữ đường đi của từng màu: colorId -> danh sách index các ô theo thứ tự
    private Dictionary<int, List<int>> colorPaths = new Dictionary<int, List<int>>();

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

        if (dialogueUICanvas != null) dialogueUICanvas.SetActive(false);
        if (playerCameraController != null) playerCameraController.enabled = false;
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ClosePuzzle()
    {
        if (puzzlePanel != null) puzzlePanel.SetActive(false);

        if (dialogueUICanvas != null) dialogueUICanvas.SetActive(true);
        Time.timeScale = 1f;
        if (playerCameraController != null) playerCameraController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnCellPointerDown(int index)
    {
        if (isSolved || index < 0 || index >= cells.Count) return;

        PuzzleCell cell = cells[index];

        // 1. Nhấn vào điểm gốc Endpoint
        if (cell.isEndpoint)
        {
            activeColor = cell.colorId;
            isDragging = true;

            if (!colorPaths.ContainsKey(activeColor))
            {
                colorPaths[activeColor] = new List<int>();
            }

            ResetColorPath(activeColor);
            colorPaths[activeColor].Add(index);
            return;
        }

        // 2. Nhấn vào một ô đã có dây nối trước đó
        if (cell.colorId != 0)
        {
            activeColor = cell.colorId;
            isDragging = true;

            if (colorPaths.ContainsKey(activeColor))
            {
                TrimPathTo(activeColor, index);
            }
        }
    }

    public void OnCellPointerEnter(int index)
    {
        if (isSolved || !isDragging || activeColor == 0 || index < 0 || index >= cells.Count) return;

        if (!colorPaths.ContainsKey(activeColor) || colorPaths[activeColor].Count == 0) return;

        List<int> currentPath = colorPaths[activeColor];
        int lastIndex = currentPath[currentPath.Count - 1];

        if (index == lastIndex) return;

        // Cơ chế tua lùi nếu lướt chuột ngược lại các ô trước đó trên cùng dây
        int existingIndexInPath = currentPath.IndexOf(index);
        if (existingIndexInPath != -1)
        {
            TrimPathTo(activeColor, index);
            return;
        }

        // Chặn hoàn toàn đi chéo (chỉ cho phép đi 4 hướng liền kề)
        if (!IsAdjacent(lastIndex, index)) return;

        // Nếu đường dây hiện tại đã chạm tới Endpoint đích thì dừng kéo tiếp
        if (currentPath.Count > 1 && cells[lastIndex].isEndpoint) return;

        PuzzleCell targetCell = cells[index];

        // Không cho phép đi vào Endpoint của màu khác
        if (targetCell.isEndpoint && targetCell.colorId != activeColor) return;

        // Nếu đè lên dây của màu khác -> Reset toàn bộ dây của màu bị đè
        if (targetCell.colorId != 0 && targetCell.colorId != activeColor && !targetCell.isEndpoint)
        {
            ResetColorPath(targetCell.colorId);
        }

        // Thêm ô vào đường dây và hiển thị màu
        currentPath.Add(index);
        targetCell.colorId = activeColor;
        UpdateCellVisual(targetCell);

        CheckWinCondition();
    }

    public void OnCellPointerUp(int index)
    {
        isDragging = false;
        activeColor = 0;
    }

    private bool IsAdjacent(int idxA, int idxB)
    {
        int rowA = idxA / GRID_SIZE;
        int colA = idxA % GRID_SIZE;
        int rowB = idxB / GRID_SIZE;
        int colB = idxB % GRID_SIZE;

        int rowDiff = Mathf.Abs(rowA - rowB);
        int colDiff = Mathf.Abs(colA - colB);

        return (rowDiff + colDiff) == 1;
    }

    private void TrimPathTo(int cId, int targetIndex)
    {
        if (!colorPaths.ContainsKey(cId)) return;

        List<int> path = colorPaths[cId];
        int targetPos = path.IndexOf(targetIndex);
        if (targetPos == -1) return;

        for (int i = path.Count - 1; i > targetPos; i--)
        {
            int cellIdx = path[i];
            if (!cells[cellIdx].isEndpoint)
            {
                cells[cellIdx].colorId = 0;
                UpdateCellVisual(cells[cellIdx]);
            }
            path.RemoveAt(i);
        }
    }

    private void ResetColorPath(int cId)
    {
        if (!colorPaths.ContainsKey(cId)) return;

        List<int> path = colorPaths[cId];
        for (int i = path.Count - 1; i >= 0; i--)
        {
            int cellIdx = path[i];
            if (!cells[cellIdx].isEndpoint)
            {
                cells[cellIdx].colorId = 0;
                UpdateCellVisual(cells[cellIdx]);
            }
        }
        path.Clear();
    }

    private void InitBoardVisuals()
    {
        colorPaths.Clear();

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

        // 1. Toàn bộ 49 ô phải được phủ kín, không còn ô trống
        for (int i = 0; i < cells.Count; i++)
        {
            if (cells[i].colorId == 0) return;
        }

        // 2. Gom nhóm các điểm gốc (Endpoints) theo từng cặp màu
        Dictionary<int, List<int>> endpointsByColor = new Dictionary<int, List<int>>();
        for (int i = 0; i < cells.Count; i++)
        {
            if (cells[i].isEndpoint)
            {
                int cId = cells[i].colorId;
                if (!endpointsByColor.ContainsKey(cId))
                {
                    endpointsByColor[cId] = new List<int>();
                }
                endpointsByColor[cId].Add(i);
            }
        }

        // 3. Quét BFS đảm bảo 2 Endpoint của tất cả các màu đều được nối thông mạch
        foreach (var pair in endpointsByColor)
        {
            int colorId = pair.Key;
            List<int> endpoints = pair.Value;

            if (endpoints.Count != 2) return;

            int startPoint = endpoints[0];
            int endPoint = endpoints[1];

            if (!IsPathConnectedBFS(startPoint, endPoint, colorId))
            {
                return; // Có màu chưa nối thông đến đích
            }
        }

        // Đã thỏa mãn tất cả điều kiện -> Kích hoạt thắng
        OnPuzzleSolved();
    }

    private bool IsPathConnectedBFS(int start, int target, int colorId)
    {
        Queue<int> queue = new Queue<int>();
        HashSet<int> visited = new HashSet<int>();

        queue.Enqueue(start);
        visited.Add(start);

        int[] dRow = { -1, 1, 0, 0 };
        int[] dCol = { 0, 0, -1, 1 };

        while (queue.Count > 0)
        {
            int curr = queue.Dequeue();
            if (curr == target) return true;

            int row = curr / GRID_SIZE;
            int col = curr % GRID_SIZE;

            for (int i = 0; i < 4; i++)
            {
                int nRow = row + dRow[i];
                int nCol = col + dCol[i];

                if (nRow >= 0 && nRow < GRID_SIZE && nCol >= 0 && nCol < GRID_SIZE)
                {
                    int neighbor = nRow * GRID_SIZE + nCol;

                    if (!visited.Contains(neighbor) && cells[neighbor].colorId == colorId)
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        return false;
    }

    private void OnPuzzleSolved()
    {
        isSolved = true;
        Debug.Log("<color=cyan>======> [CHÚC MỪNG] ĐÃ GIẢI THÀNH CÔNG! ĐANG KÍCH HOẠT BẬT ĐÈN... <======</color>");

        // Bật sáng bóng đèn ngoài map
        if (targetLight != null)
        {
            targetLight.gameObject.SetActive(true);
            targetLight.enabled = true;
            targetLight.color = lightTurnOnColor;
            targetLight.intensity = lightIntensityOn;
        }
        else
        {
            Debug.LogError("[Puzzle] Chưa kéo Target Light vào Inspector của PuzzleController!");
        }

        StartCoroutine(AutoCloseRoutine());
    }

    private System.Collections.IEnumerator AutoCloseRoutine()
    {
        yield return new WaitForSecondsRealtime(0.8f);
        ClosePuzzle();
    }
}

// Lắng nghe sự kiện chuột mượt mà tương thích 100% với New Input System
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