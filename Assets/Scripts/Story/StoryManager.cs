using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class StoryManager : MonoBehaviour
{
    public static StoryManager Instance { get; private set; }

    [Header("Story")]
    [SerializeField] private string chapterFileName = "chapter1";

    [Header("Intro")]
    [SerializeField] public StoryIntroUI storyIntroUI;

    private StoryData storyData;

    private int currentEventIndex;

    private StoryEvent currentEvent;

    private bool isRunning;

    private HashSet<string> completedEvents =
        new HashSet<string>();

    private HashSet<string> completedConversations =
        new HashSet<string>();

    private HashSet<string> completedObjectives =
        new HashSet<string>();

    public string CurrentChapterId =>
        storyData != null
            ? storyData.chapterId
            : "";

    public StoryEvent CurrentEvent =>
        currentEvent;

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

    private void Start()
    {
        StartChapter();
    }

    // =========================================================
    // LOAD CHAPTER
    // =========================================================

    private void LoadChapter()
    {
        TextAsset jsonFile =
            Resources.Load<TextAsset>(
                "Story/" + chapterFileName
            );

        if (jsonFile == null)
        {
            Debug.LogError(
                $"Cannot find story file: " +
                $"Resources/Story/{chapterFileName}.json"
            );

            return;
        }

        storyData =
            JsonUtility.FromJson<StoryData>(
                jsonFile.text
            );

        if (storyData == null)
        {
            Debug.LogError(
                "Failed to parse story JSON."
            );

            return;
        }

        Debug.Log(
            $"Loaded chapter: " +
            $"{storyData.chapterId} - " +
            $"{storyData.chapterTitle}"
        );
    }

    // =========================================================
    // START CHAPTER
    // =========================================================

    public void StartChapter()
    {
        if (storyData == null)
        {
            Debug.LogError(
                "Cannot start chapter. Story data is null."
            );

            return;
        }

        currentEventIndex = 0;

        currentEvent = null;

        isRunning = true;

        PlayIntro();
    }

    // =========================================================
    // STORY EVENT RUNNER
    // =========================================================

    private void PlayNextEvent()
    {
        if (!isRunning)
            return;

        if (storyData.events == null ||
            currentEventIndex >= storyData.events.Length)
        {
            FinishChapter();
            return;
        }

        currentEvent =
            storyData.events[currentEventIndex];

        Debug.Log(
            $"Story Event: " +
            $"{currentEvent.id} " +
            $"({currentEvent.type})"
        );

        switch (currentEvent.type)
        {
            case "narration":

            case "dialogue":

                PlayDialogueEvent();

                break;

            case "objective":

                PlayObjectiveEvent();

                break;

            default:

                Debug.LogWarning(
                    $"Unknown story event type: " +
                    $"{currentEvent.type}"
                );

                CompleteCurrentEvent();

                break;
        }
    }

    // =========================================================
    // DIALOGUE / NARRATION EVENT
    // =========================================================

    private void PlayDialogueEvent()
    {
        DialogueManager.Instance.StartStoryDialogue(
            currentEvent.speaker,
            currentEvent.text,
            CompleteCurrentEvent
        );
    }

    // =========================================================
    // OBJECTIVE EVENT
    // =========================================================

    private void PlayObjectiveEvent()
    {
        if (ObjectiveManager.Instance == null)
        {
            Debug.LogError(
                "ObjectiveManager not found."
            );

            return;
        }

        ObjectiveManager.Instance.SetObjective(
            currentEvent.objectiveId,
            currentEvent.objectiveText
        );

        // IMPORTANT:
        // StoryManager now waits here.
        //
        // The gameplay system must call:
        //
        // StoryManager.Instance.CompleteObjective(
        //     currentEvent.objectiveId
        // );
    }

    // =========================================================
    // COMPLETE CURRENT EVENT
    // =========================================================

    private void CompleteCurrentEvent()
    {
        if (currentEvent == null)
            return;

        completedEvents.Add(
            currentEvent.id
        );

        currentEventIndex++;

        currentEvent = null;

        PlayNextEvent();
    }

    // =========================================================
    // COMPLETE OBJECTIVE
    // =========================================================

    public void CompleteObjective(
        string objectiveId)
    {
        if (currentEvent == null)
            return;

        if (currentEvent.type != "objective")
            return;

        if (currentEvent.objectiveId != objectiveId)
            return;

        completedObjectives.Add(
            objectiveId
        );

        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.CompleteObjective();
        }

        CompleteCurrentEvent();
    }

    // =========================================================
    // NPC CONVERSATION
    // =========================================================

    public ConversationData GetAvailableConversation(
        string npcId)
    {
        if (storyData == null ||
            storyData.conversations == null)
        {
            return null;
        }

        foreach (
            ConversationData conversation
            in storyData.conversations)
        {
            if (conversation.npcId != npcId)
                continue;

            if (completedConversations.Contains(
                conversation.id))
            {
                continue;
            }

            if (!CheckCondition(
                conversation.condition))
            {
                continue;
            }

            return conversation;
        }

        return null;
    }

    private bool CheckCondition(
        DialogueCondition condition)
    {
        if (condition == null)
            return true;

        // Required conversation
        if (!string.IsNullOrEmpty(
            condition.requiredConversationId))
        {
            if (!completedConversations.Contains(
                condition.requiredConversationId))
            {
                return false;
            }
        }

        // Required objective
        if (!string.IsNullOrEmpty(
            condition.requiredObjectiveId))
        {
            if (!completedObjectives.Contains(
                condition.requiredObjectiveId))
            {
                return false;
            }
        }

        return true;
    }

    public void CompleteConversation(
        string conversationId)
    {
        if (string.IsNullOrEmpty(
            conversationId))
        {
            return;
        }

        completedConversations.Add(
            conversationId
        );

        Debug.Log(
            $"Conversation completed: " +
            $"{conversationId}"
        );
    }

    // =========================================================
    // CHAPTER COMPLETE
    // =========================================================

    private void FinishChapter()
    {
        isRunning = false;

        currentEvent = null;

        Debug.Log(
            $"Chapter completed: " +
            $"{CurrentChapterId}"
        );
    }

    // =========================================================
    // DEBUG
    // =========================================================

    public bool IsStoryRunning()
    {
        return isRunning;
    }

    public bool IsObjectiveCompleted(
        string objectiveId)
    {
        return completedObjectives.Contains(
            objectiveId
        );
    }

    private void PlayIntro()
    {
        if (storyIntroUI == null)
        {
            Debug.LogWarning(
                "StoryIntroUI is not assigned."
            );

            PlayNextEvent();

            return;
        }

        StoryEvent[] introEvents =
            GetIntroEvents();

        if (introEvents.Length == 0)
        {
            PlayNextEvent();

            return;
        }

        storyIntroUI.PlayIntro(introEvents);

        StartCoroutine(
            WaitForIntroComplete(
                introEvents.Length
            )
        );
    }
    private StoryEvent[] GetIntroEvents()
    {
        List<StoryEvent> introEvents =
            new List<StoryEvent>();

        for (int i = currentEventIndex;
            i < storyData.events.Length;
            i++)
        {
            StoryEvent storyEvent =
                storyData.events[i];

            if (storyEvent.type != "narration")
                break;

            introEvents.Add(storyEvent);
        }

        return introEvents.ToArray();
    }

    private IEnumerator WaitForIntroComplete(
    int introEventCount)
    {
        while (storyIntroUI.IsPlaying)
        {
            yield return null;
        }

        for (int i = 0; i < introEventCount; i++)
        {
            StoryEvent storyEvent =
                storyData.events[currentEventIndex];

            completedEvents.Add(
                storyEvent.id
            );

            currentEventIndex++;
        }

        PlayNextEvent();
    }
}