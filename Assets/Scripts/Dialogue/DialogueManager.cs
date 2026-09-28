using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [SerializeField] private DialogueUI dialogueUI;

    private ConversationData currentConversation;
    private int currentLineIndex;
    private bool isDialogueActive;

    public bool IsDialogueActive => isDialogueActive;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void StartDialogue(ConversationData conversation)
    {
        if (conversation == null)
            return;

        if (conversation.lines == null ||
            conversation.lines.Length == 0)
            return;

        currentConversation = conversation;
        currentLineIndex = 0;
        isDialogueActive = true;

        dialogueUI.ShowDialogue(
            conversation.lines[currentLineIndex]
        );
    }

    public void NextLine()
    {
        if (!isDialogueActive)
            return;

        currentLineIndex++;

        if (currentLineIndex >= currentConversation.lines.Length)
        {
            EndDialogue();
            return;
        }

        dialogueUI.ShowDialogue(
            currentConversation.lines[currentLineIndex]
        );
    }

    public void EndDialogue()
    {
        isDialogueActive = false;

        if (currentConversation != null)
        {
            StoryManager.Instance.CompleteConversation(
                currentConversation.id
            );
        }

        currentConversation = null;
        currentLineIndex = 0;

        dialogueUI.HideDialogue();
    }
}