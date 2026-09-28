using System.Collections.Generic;
using UnityEngine;

public class StoryManager : MonoBehaviour
{
    public static StoryManager Instance { get; private set; }

    [Header("Story")]
    [SerializeField] private string chapterFileName = "chapter1";

    private StoryData storyData;

    private int currentEventIndex;

    private HashSet<string> completedEvents = new HashSet<string>();
    private HashSet<string> completedConversations = new HashSet<string>();

    public string CurrentChapterId =>
        storyData != null ? storyData.chapterId : "";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        LoadChapter();
    }

    private void LoadChapter()
    {
        TextAsset jsonFile =
            Resources.Load<TextAsset>("Story/" + chapterFileName);

        if (jsonFile == null)
        {
            Debug.LogError(
                $"Cannot find story file: Resources/Story/{chapterFileName}.json"
            );

            return;
        }

        storyData =
            JsonUtility.FromJson<StoryData>(jsonFile.text);

        if (storyData == null)
        {
            Debug.LogError("Failed to parse story JSON.");
            return;
        }

        Debug.Log(
            $"Loaded chapter: {storyData.chapterId} - {storyData.chapterTitle}"
        );
    }

    public ConversationData GetAvailableConversation(string npcId)
    {
        if (storyData == null ||
            storyData.conversations == null)
        {
            return null;
        }

        foreach (ConversationData conversation in storyData.conversations)
        {
            if (conversation.npcId != npcId)
                continue;

            if (completedConversations.Contains(conversation.id))
                continue;

            if (!CheckCondition(conversation.condition))
                continue;

            return conversation;
        }

        return null;
    }

    private bool CheckCondition(DialogueCondition condition)
    {
        if (condition == null)
            return true;

        if (!string.IsNullOrEmpty(condition.requiredConversationId))
        {
            if (!completedConversations.Contains(
                condition.requiredConversationId))
            {
                return false;
            }
        }

        // Evidence và objective sẽ xử lý sau.
        return true;
    }

    public void CompleteConversation(string conversationId)
    {
        if (string.IsNullOrEmpty(conversationId))
            return;

        completedConversations.Add(conversationId);

        Debug.Log(
            $"Conversation completed: {conversationId}"
        );
    }

    public StoryEvent GetNextEvent()
    {
        if (storyData == null ||
            storyData.events == null)
        {
            return null;
        }

        while (currentEventIndex < storyData.events.Length)
        {
            StoryEvent storyEvent =
                storyData.events[currentEventIndex];

            if (!completedEvents.Contains(storyEvent.id))
            {
                return storyEvent;
            }

            currentEventIndex++;
        }

        return null;
    }

    public void CompleteEvent(string eventId)
    {
        completedEvents.Add(eventId);

        currentEventIndex++;

        Debug.Log(
            $"Story event completed: {eventId}"
        );
    }

    public ConversationData GetIntroConversation()
    {
        if (storyData == null ||
            storyData.events == null)
        {
            return null;
        }

        List<DialogueLine> lines =
            new List<DialogueLine>();

        foreach (StoryEvent storyEvent in storyData.events)
        {
            if (storyEvent.type != "narration")
                continue;

            lines.Add(new DialogueLine
            {
                speaker = storyEvent.speaker,
                text = storyEvent.text
            });
        }

        if (lines.Count == 0)
            return null;

        return new ConversationData
        {
            id = "CHAPTER_01_INTRO",
            npcId = "",
            lines = lines.ToArray()
        };
    }
}