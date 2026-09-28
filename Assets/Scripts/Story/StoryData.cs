using System;

[Serializable]
public class StoryData
{
    public string chapterId;
    public string chapterTitle;

    // Conversation that must be completed
    // before this chapter can finish.
    public string completionConversationId;

    public NPCData[] npcs;
    public StoryEvent[] events;
    public ConversationData[] conversations;
}

[Serializable]
public class StoryEvent
{
    public string id;
    public string type;
    public string speaker;
    public string text;
    public string objectiveId;
    public string objectiveText;
}