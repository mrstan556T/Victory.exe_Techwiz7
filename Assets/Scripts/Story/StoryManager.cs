using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryManager : MonoBehaviour
{
    public static StoryManager Instance { get; private set; }

    [Header("Story")]
    [SerializeField] private string startingChapter = "chapter1";

    private string currentChapterFileName;

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


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        LoadChapter(startingChapter);
    }

    private void Start()
    {
        StartChapter();
    }


    // =========================================================
    // LOAD CHAPTER
    // =========================================================

    private bool LoadChapter(string chapterFileName)
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

            return false;
        }

        StoryData loadedStory =
            JsonUtility.FromJson<StoryData>(
                jsonFile.text
            );

        if (loadedStory == null)
        {
            Debug.LogError(
                $"Failed to parse story JSON: {chapterFileName}"
            );

            return false;
        }

        storyData = loadedStory;

        currentChapterFileName = chapterFileName;

        Debug.Log(
            $"Loaded chapter: " +
            $"{storyData.chapterId} - " +
            $"{storyData.chapterTitle}"
        );

        return true;
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

        Debug.Log(
            $"Starting chapter: " +
            $"{storyData.chapterId}"
        );

        PlayIntro();
    }


    // =========================================================
    // LOAD NEXT CHAPTER
    // =========================================================

    private void LoadNextChapter()
    {
        int currentChapterNumber =
            GetChapterNumber(currentChapterFileName);

        int nextChapterNumber =
            currentChapterNumber + 1;

        string nextChapterFileName =
            $"chapter{nextChapterNumber}";

        Debug.Log(
            $"Trying to load next chapter: " +
            $"{nextChapterFileName}"
        );

        if (!LoadChapter(nextChapterFileName))
        {
            Debug.Log(
                $"No next chapter found. " +
                $"Story completed."
            );

            isRunning = false;
            return;
        }

        StartChapter();
    }


    private int GetChapterNumber(
        string chapterFileName)
    {
        if (string.IsNullOrEmpty(chapterFileName))
            return 0;

        string number =
            chapterFileName.Replace(
                "chapter",
                ""
            );

        if (int.TryParse(
            number,
            out int result))
        {
            return result;
        }

        return 0;
    }


    // =========================================================
    // STORY EVENT RUNNER
    // =========================================================

    private void PlayNextEvent()
    {
        if (!isRunning)
            return;

        // IMPORTANT:
        // Do NOT finish the chapter just because
        // there are no story events.
        //
        // Some chapters are conversation-only.
        //
        if (storyData.events == null ||
            currentEventIndex >= storyData.events.Length)
        {
            Debug.Log(
                $"No more story events in " +
                $"{storyData.chapterId}. " +
                $"Waiting for required conversation."
            );

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
            ObjectiveManager.Instance.CompleteObjective(objectiveId);
        }

        CompleteCurrentEvent();
    }


    // =========================================================
    // NPC CONVERSATION
    // =========================================================

    public ConversationData GetAvailableConversation(string npcId)
    {
        if (storyData == null ||
            storyData.conversations == null)
        {
            Debug.LogWarning("StoryData or conversations is null.");
            return null;
        }

        Debug.Log(
            $"Looking for conversation. NPC = {npcId}, " +
            $"CurrentEvent = {currentEvent?.id}, " +
            $"CurrentObjective = {currentEvent?.objectiveId}"
        );

        foreach (ConversationData conversation in storyData.conversations)
        {
            Debug.Log(
                $"Checking conversation: {conversation.id}, " +
                $"NPC = {conversation.npcId}"
            );

            if (conversation.npcId != npcId)
                continue;

            if (completedConversations.Contains(conversation.id))
                continue;

            if (!CheckCondition(conversation.condition))
            {
                Debug.Log(
                    $"Conversation condition failed: {conversation.id}"
                );

                continue;
            }

            Debug.Log(
                $"Conversation FOUND: {conversation.id}"
            );

            return conversation;
        }

        Debug.LogWarning(
            $"No available conversation found for NPC: {npcId}"
        );

        return null;
    }


    private bool CheckCondition(DialogueCondition condition)
    {
        if (condition == null)
            return true;

        // Required conversation
        if (!string.IsNullOrEmpty(condition.requiredConversationId))
        {
            if (!completedConversations.Contains(
                condition.requiredConversationId))
            {
                return false;
            }
        }

        // Required objective = objective currently active
        if (!string.IsNullOrEmpty(condition.requiredObjectiveId))
        {
            if (currentEvent == null ||
                currentEvent.type != "objective" ||
                currentEvent.objectiveId != condition.requiredObjectiveId)
            {
                return false;
            }
        }

        return true;
    }


    // =========================================================
    // COMPLETE CONVERSATION
    // =========================================================

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


        // =====================================================
        // CHECK CHAPTER COMPLETION
        // =====================================================

        if (storyData != null &&
            !string.IsNullOrEmpty(
                storyData.completionConversationId))
        {
            if (conversationId ==
                storyData.completionConversationId)
            {
                Debug.Log(
                    $"Required conversation completed. " +
                    $"Finishing chapter: " +
                    $"{storyData.chapterId}"
                );

                FinishChapter();
            }
        }
    }

    public bool IsConversationCompleted(string conversationId)
    {
        if (string.IsNullOrEmpty(conversationId))
            return false;

        return completedConversations.Contains(conversationId);
    }


    // =========================================================
    // CHAPTER COMPLETE
    // =========================================================

    private void FinishChapter()
    {
        if (!isRunning)
            return;

        isRunning = false;

        currentEvent = null;

        Debug.Log(
            $"Chapter completed: " +
            $"{CurrentChapterId}"
        );

        LoadNextChapter();
    }


    // =========================================================
    // NPC DATA
    // =========================================================

    public NPCData GetNPCData(
        string npcId)
    {
        if (storyData == null ||
            storyData.npcs == null)
        {
            return null;
        }

        foreach (NPCData npc in storyData.npcs)
        {
            if (npc.npcId == npcId)
            {
                return npc;
            }
        }

        return null;
    }


    public string GetNPCDisplayName(
        string npcId)
    {
        NPCData npc =
            GetNPCData(npcId);

        if (npc == null)
        {
            Debug.LogWarning(
                $"NPC not found: {npcId}"
            );

            return npcId;
        }

        return npc.displayName;
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


    // =========================================================
    // INTRO
    // =========================================================

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

        storyIntroUI.PlayIntro(
            introEvents
        );

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

        if (storyData.events == null)
            return introEvents.ToArray();

        for (
            int i = currentEventIndex;
            i < storyData.events.Length;
            i++)
        {
            StoryEvent storyEvent =
                storyData.events[i];

            if (storyEvent.type != "narration")
                break;

            introEvents.Add(
                storyEvent
            );
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

        for (
            int i = 0;
            i < introEventCount;
            i++)
        {
            StoryEvent storyEvent =
                storyData.events[
                    currentEventIndex
                ];

            completedEvents.Add(
                storyEvent.id
            );

            currentEventIndex++;
        }

        PlayNextEvent();
    }
}