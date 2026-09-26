using UnityEngine;
using UnityEngine.UI;

public class NPCInteraction : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private Button talkButton;

    [Header("Dialogue")]
    [SerializeField] private DialogueData dialogueData;

    private bool playerInRange;

    private void Awake()
    {
        interactionPrompt.SetActive(false);

        if (talkButton != null) talkButton.onClick.AddListener(StartConversation);
    }

    private void Update()
    {
        if(playerInRange)
        {
            if(Input.GetKeyDown(KeyCode.E))
            {
                StartConversation();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;

        // Không hiện Talk nếu đang dialogue
        if (!DialogueManager.Instance.IsDialogueActive)
        {
            interactionPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;

        interactionPrompt.SetActive(false);
    }

    private void StartConversation()
    {
        if (!playerInRange)
            return;

        if (DialogueManager.Instance.IsDialogueActive)
            return;

        interactionPrompt.SetActive(false);

        DialogueManager.Instance.StartDialogue(dialogueData);
    }
}