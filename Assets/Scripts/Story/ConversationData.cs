using System;

[Serializable]
public class ConversationData
{
    public string id;
    public string npcId;
    public DialogueLine[] lines;

    public DialogueLine prompt;

    public DialogueCondition condition;
    public ConversationCompletion onComplete;
}