using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [SerializeField] private DialogueUI dialogueUI;

    private DialogueLine[] currentDialogue;
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

    public void StartDialogue(DialogueData dialogueData)
    {
        if (dialogueData == null)
            return;

        if (dialogueData.dialogueLines == null ||
            dialogueData.dialogueLines.Length == 0)
            return;

        currentDialogue = dialogueData.dialogueLines;
        currentLineIndex = 0;

        isDialogueActive = true;

        dialogueUI.ShowDialogue(currentDialogue[currentLineIndex]);
    }

    public void NextLine()
    {
        if (!isDialogueActive)
            return;

        currentLineIndex++;

        if (currentLineIndex >= currentDialogue.Length)
        {
            EndDialogue();
            return;
        }

        dialogueUI.ShowDialogue(currentDialogue[currentLineIndex]);
    }

    public void EndDialogue()
    {
        isDialogueActive = false;

        currentDialogue = null;
        currentLineIndex = 0;

        dialogueUI.HideDialogue();
    }
}