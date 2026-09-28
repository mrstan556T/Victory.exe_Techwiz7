using UnityEngine;

public class ElectricBoxTrigger : MonoBehaviour
{
    [Header("Giao Diện Minigame")]
    [SerializeField] private ColorConnectPuzzle puzzleController; // Kéo PuzzleController vào
    [SerializeField] private GameObject interactPromptUI;         // Kéo Text nhắc phím [E] vào

    private bool isPlayerNearby = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<CharacterController>() != null)
        {
            isPlayerNearby = true;
            if (interactPromptUI != null) interactPromptUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<CharacterController>() != null)
        {
            isPlayerNearby = false;
            if (interactPromptUI != null) interactPromptUI.SetActive(false);
            if (puzzleController != null) puzzleController.ClosePuzzle();
        }
    }

    private void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            if (puzzleController != null)
            {
                puzzleController.OpenPuzzle();
                if (interactPromptUI != null) interactPromptUI.SetActive(false);
            }
        }
    }
}