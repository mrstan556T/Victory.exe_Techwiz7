using System;
using UnityEngine;

public class PatrolDialogueTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NPCIdentity npcIdentity;
    [SerializeField] private PatrolResponse patrolResponse;

    [Header("Dialogue")]
    [SerializeField] private TextAsset dialogueFile;

    private void Awake()
    {
        Debug.Log(
            "PatrolDialogueTrigger: Awake()"
        );

        if (npcIdentity == null)
        {
            npcIdentity = GetComponent<NPCIdentity>();
        }

        if (patrolResponse == null)
        {
            patrolResponse = GetComponent<PatrolResponse>();
        }

        Debug.Log(
            "PatrolDialogueTrigger: NPC Identity = " +
            (npcIdentity != null ? npcIdentity.NpcId : "NULL")
        );

        Debug.Log(
            "PatrolDialogueTrigger: Patrol Response = " +
            (patrolResponse != null ? "FOUND" : "NULL")
        );

        Debug.Log(
            "PatrolDialogueTrigger: Dialogue File = " +
            (dialogueFile != null ? dialogueFile.name : "NULL")
        );
    }

    public void StartPatrolDialogue()
    {
        Debug.Log(
            "PatrolDialogueTrigger: StartPatrolDialogue() called."
        );

        if (dialogueFile == null)
        {
            Debug.LogError(
                "PatrolDialogueTrigger: Dialogue file is missing."
            );

            return;
        }

        Debug.Log(
            "PatrolDialogueTrigger: Dialogue file found: " +
            dialogueFile.name
        );

        if (DialogueManager.Instance == null)
        {
            Debug.LogError(
                "PatrolDialogueTrigger: DialogueManager.Instance is null."
            );

            return;
        }

        Debug.Log(
            "PatrolDialogueTrigger: DialogueManager found."
        );

        if (patrolResponse == null)
        {
            Debug.LogError(
                "PatrolDialogueTrigger: PatrolResponse is missing."
            );

            return;
        }

        ConversationData conversation;

        try
        {
            Debug.Log(
                "PatrolDialogueTrigger: Parsing JSON..."
            );

            conversation =
                JsonUtility.FromJson<ConversationData>(
                    dialogueFile.text
                );
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "PatrolDialogueTrigger: Failed to load dialogue. " +
                exception.Message
            );

            return;
        }

        if (conversation == null)
        {
            Debug.LogError(
                "PatrolDialogueTrigger: Dialogue data is invalid."
            );

            return;
        }

        Debug.Log(
            "PatrolDialogueTrigger: JSON parsed."
        );

        Debug.Log(
            "PatrolDialogueTrigger: Conversation ID = " +
            conversation.id
        );

        Debug.Log(
            "PatrolDialogueTrigger: NPC ID = " +
            conversation.npcId
        );

        if (conversation.lines == null ||
            conversation.lines.Length == 0)
        {
            Debug.LogError(
                "PatrolDialogueTrigger: Dialogue has no lines."
            );

            return;
        }

        Debug.Log(
            "PatrolDialogueTrigger: Dialogue lines = " +
            conversation.lines.Length
        );

        if (npcIdentity != null &&
            conversation.npcId != npcIdentity.NpcId)
        {
            Debug.LogError(
                "PatrolDialogueTrigger: NPC ID mismatch. " +
                "NPC = " +
                npcIdentity.NpcId +
                " | Dialogue = " +
                conversation.npcId
            );

            return;
        }

        Debug.Log(
            "PatrolDialogueTrigger: NPC ID matched."
        );

        Debug.Log(
            "PatrolDialogueTrigger: Calling DialogueManager.StartPatrolDialogue()."
        );

        DialogueManager.Instance.StartPatrolDialogue(
            conversation,
            patrolResponse.OnDialogueCompleted
        );

        Debug.Log(
            "PatrolDialogueTrigger: StartPatrolDialogue() completed."
        );
    }
}