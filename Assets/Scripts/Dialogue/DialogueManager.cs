using System;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [SerializeField] private DialogueUI dialogueUI;

    private ConversationData currentConversation;

    private int currentLineIndex;

    private bool isDialogueActive;

    private Action onDialogueComplete;

    public bool IsDialogueActive => isDialogueActive;
    
    private bool isPatrolDialogue;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // =========================================================
    // NPC DIALOGUE
    // =========================================================

    public void StartDialogue(ConversationData conversation)
    {
        //patrol dialogue 
        isPatrolDialogue = false;
        
        if (conversation == null)
            return;

        if (conversation.lines == null ||
            conversation.lines.Length == 0)
            return;

        currentConversation = conversation;

        currentLineIndex = 0;

        isDialogueActive = true;

        onDialogueComplete = null;

        dialogueUI.ShowDialogue(
            conversation.lines[currentLineIndex]
        );
    }

    // =========================================================
    // PATROL DIALOGUE
    // =========================================================
    public void StartPatrolDialogue(
    ConversationData conversation,
    Action onComplete)
{
    if (conversation == null)
        return;

    if (conversation.lines == null ||
        conversation.lines.Length == 0)
        return;

    currentConversation = conversation;

    currentLineIndex = 0;

    isDialogueActive = true;

    isPatrolDialogue = true;

    onDialogueComplete = onComplete;

    dialogueUI.ShowDialogue(
        conversation.lines[currentLineIndex]
    );
}

    // =========================================================
    // STORY EVENT DIALOGUE
    // =========================================================

    public void StartStoryDialogue(
        string speaker,
        string text,
        Action onComplete)
    {
        //patrol dialogue 
        isPatrolDialogue = false;

        if (string.IsNullOrEmpty(text))
        {
            onComplete?.Invoke();
            return;
        }

        currentConversation = null;

        currentLineIndex = 0;

        isDialogueActive = true;

        onDialogueComplete = onComplete;

        DialogueLine line = new DialogueLine
        {
            speaker = speaker,
            text = text
        };

        dialogueUI.ShowDialogue(line);
    }

    // =========================================================
    // NEXT LINE
    // =========================================================

    public void NextLine()
    {
        if (!isDialogueActive)
            return;

        // Story dialogue only has one line.
        if (currentConversation == null)
        {
            EndDialogue();
            return;
        }

        currentLineIndex++;

        if (currentLineIndex >=
            currentConversation.lines.Length)
        {
            EndDialogue();
            return;
        }

        dialogueUI.ShowDialogue(
            currentConversation.lines[currentLineIndex]
        );
    }

    // =========================================================
    // END
    // =========================================================

    public void EndDialogue()
    {
        if (!isDialogueActive)
            return;

        isDialogueActive = false;

        Action callback = onDialogueComplete;

        onDialogueComplete = null;

        // NPC dialogue
        if (currentConversation != null && !isPatrolDialogue)
        {
            if (StoryManager.Instance != null)
            {
                StoryManager.Instance.CompleteConversation(
                    currentConversation.id
                );
            }
        }

        currentConversation = null;

        currentLineIndex = 0;

        dialogueUI.HideDialogue();

        isPatrolDialogue = false;

        // Story dialogue callback
        callback?.Invoke();
        
    }
}