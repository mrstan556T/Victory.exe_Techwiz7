using UnityEngine;
using TMPro;

public class NPCInteraction : MonoBehaviour
{
    [Header("NPC")]
    [SerializeField] private string npcId;

    [Header("Interaction Prompt")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private TMP_Text promptText;

    private bool playerInRange;

    private void Start()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    private void Update()
    {
        if (!playerInRange)
            return;

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

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    private void UpdatePrompt()
    {
        if (StoryManager.Instance == null)
            return;

        ConversationData conversation =
            StoryManager.Instance.GetAvailableConversation(npcId);

        if (conversation == null)
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }

            return;
        }

        if (promptText != null)
        {
            promptText.text =
                $"[E] {conversation.prompt.text}";
        }
    }

    private void StartConversation()
    {
        if (StoryManager.Instance == null)
            return;

        ConversationData conversation =
            StoryManager.Instance.GetAvailableConversation(npcId);

        if (conversation == null)
            return;

        if (DialogueManager.Instance == null)
            return;

        DialogueManager.Instance.StartDialogue(
            conversation
        );

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }
}