using UnityEngine;
using TMPro;

public class NPCInteraction : MonoBehaviour
{
    [Header("Interaction Prompt")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private TMP_Text promptText;

    private NPCIdentity npcIdentity;
    private bool playerInRange;

    private void Awake()
    {
        npcIdentity = GetComponent<NPCIdentity>();

        if (npcIdentity == null)
        {
            Debug.LogError(
                $"NPCIdentity missing on {gameObject.name}"
            );
        }
    }

    private void Start()
    {
        HidePrompt();
    }

    private void Update()
    {
        if (!playerInRange)
            return;

        // Nếu đang dialogue thì không hiện prompt
        if (DialogueManager.Instance != null &&
            DialogueManager.Instance.IsDialogueActive)
        {
            HidePrompt();
            return;
        }

        UpdatePrompt();

        if (Input.GetKeyDown(KeyCode.E))
        {
            StartConversation();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;

        UpdatePrompt();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;

        HidePrompt();
    }

    private void UpdatePrompt()
    {
        if (StoryManager.Instance == null)
        {
            HidePrompt();
            return;
        }

        if (npcIdentity == null)
        {
            HidePrompt();
            return;
        }

        ConversationData conversation =
            StoryManager.Instance.GetAvailableConversation(
                npcIdentity.NpcId
            );

        Debug.Log(
            $"NPC: {npcIdentity.NpcId} | " +
            $"Conversation: " +
            (conversation != null
                ? conversation.id
                : "NULL")
        );

        // Không có conversation khả dụng
        if (conversation == null)
        {
            HidePrompt();
            return;
        }

        // Có conversation nhưng không có prompt
        if (conversation.prompt == null)
        {
            if (promptText != null)
            {
                promptText.text = "[ E ] Talk";
            }

            ShowPrompt();
            return;
        }

        // Hiển thị câu prompt của conversation
        if (promptText != null)
        {
            string prompt = conversation.prompt.text;

            if (string.IsNullOrWhiteSpace(prompt))
            {
                promptText.text = "[ E ] Talk";
            }
            else
            {
                promptText.text = $"[ E ] {prompt}";
            }
        }

        ShowPrompt();
    }

    private void StartConversation()
    {
        if (StoryManager.Instance == null)
            return;

        if (npcIdentity == null)
            return;

        ConversationData conversation =
            StoryManager.Instance.GetAvailableConversation(
                npcIdentity.NpcId
            );

        if (conversation == null)
            return;

        if (DialogueManager.Instance == null)
            return;

        // Ẩn prompt trước khi bắt đầu dialogue
        HidePrompt();

        DialogueManager.Instance.StartDialogue(
            conversation
        );
    }

    private void ShowPrompt()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(true);
        }
    }

    private void HidePrompt()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }
}