using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CaseConclusionManager : MonoBehaviour
{
    [Header("Open Button")]
    [SerializeField] private Button openCaseButton;
    [Header("Case Unlock")]
    [SerializeField] private int requiredEvidenceCount = 5;
    [SerializeField] private string requiredConversationId = "JACE_CH6_01";
    private bool canOpenCase = false;
    [Header("Main Panels")]
    [SerializeField] private GameObject conclusionPanel;
    [SerializeField] private GameObject deductionPanel;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private GameObject wrongAccusationPanel;

    [Header("Suspect UI")]
    [SerializeField] private TMP_Text selectedSuspectText;
    [SerializeField] private Button accuseButton;

    [SerializeField] private SuspectCardUI suspectCardPrefab;
    [SerializeField] private Transform suspectContainer;

    [SerializeField] private SuspectData miraData;
    [SerializeField] private SuspectData lenaData;
    [SerializeField] private SuspectData rookData;
    [SerializeField] private SuspectData noraData;
    [SerializeField] private SuspectData jaceData;

    [Header("Deduction UI")]
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Button continueButton;

    [Header("Result UI")]
    [SerializeField] private TMP_Text resultTitle;
    [SerializeField] private TMP_Text resultDescription;

    [Header("Case")]
    [SerializeField] private SuspectId correctSuspect = SuspectId.Mira;
    [Header("Camera")]
    [SerializeField] private CameraController cameraController;

    private SuspectId selectedSuspect;

    private int deductionIndex;

    private readonly string[] deductionLines =
    {
        "Evan Cole died at approximately 6:30 PM.",

        "The burn mark on his NeuroLink wasn't caused by a conventional weapon.",

        "The empty container tells us that something had already been taken.",

        "That something was the AeroFilter-X.",

        "Jace had access to the back alley and knew Evan's delivery schedule.",

        "But the evidence against Jace is too obvious.",

        "The electric gun found at his house was planted.",

        "Nora saw a man leaving at around 7:30 PM.",

        "But Evan was already dead an hour earlier.",

        "The person Nora saw wasn't the murderer.",

        "He was part of the cover-up.",

        "Only one suspect had a clear motive to take the AeroFilter-X.",

        "Mira Kane knew exactly how valuable the device was.",

        "She had no confirmed alibi for the time of the murder.",

        "She killed Evan at 6:30 PM and took the AeroFilter-X.",

        "Then she created a false trail to frame Jace.",

        "Mira Kane is the culprit."
    };

    private void Awake()
    {
        if (accuseButton != null)
            accuseButton.onClick.AddListener(AccuseSelectedSuspect);

        if (continueButton != null)
            continueButton.onClick.AddListener(ShowNextDeduction);

        if (openCaseButton != null)
            openCaseButton.onClick.AddListener(OpenCaseConclusion);

        HideAll();
    }
    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (!Keyboard.current.rKey.wasPressedThisFrame)
            return;

        CheckCaseUnlock();

        if (!canOpenCase)
        {
            Debug.Log("Case is not unlocked yet.");
            return;
        }

        if (conclusionPanel != null && !conclusionPanel.activeSelf)
        {
            OpenCaseConclusion();
        }
    }
    private void Start()
    {
        CreateSuspectCards();
        CheckCaseUnlock();
    }
    private void CreateSuspectCards()
    {
        CreateCard(SuspectId.Mira, miraData);
        CreateCard(SuspectId.Lena, lenaData);
        CreateCard(SuspectId.Rook, rookData);
        CreateCard(SuspectId.Nora, noraData);
        CreateCard(SuspectId.Jace, jaceData);
    }
    private void CreateCard(
        SuspectId id,
        SuspectData data)
    {
        SuspectCardUI card =
            Instantiate(
                suspectCardPrefab,
                suspectContainer
            );

        card.Setup(
            id,
            data,
            this
        );
    }

    public void OpenCaseConclusion()
    {
        conclusionPanel.SetActive(true);

        if (deductionPanel != null)
            deductionPanel.SetActive(false);

        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (wrongAccusationPanel != null)
            wrongAccusationPanel.SetActive(false);

        if (selectedSuspectText != null)
            selectedSuspectText.text = "No suspect selected.";

        if (accuseButton != null)
            accuseButton.interactable = false;

        if (cameraController != null)
            cameraController.EnableUIInput();
    }

    public void SelectSuspect(SuspectId suspect)
    {
        selectedSuspect = suspect;

        selectedSuspectText.text =
            $"Selected Suspect: {suspect}";

        accuseButton.interactable = true;
    }

    private void AccuseSelectedSuspect()
    {

        if (selectedSuspect == correctSuspect)
        {
            StartDeductionSequence();
        }
        else
        {
            wrongAccusationPanel.SetActive(true);
        }
    }

    private void StartDeductionSequence()
    {
        deductionIndex = 0;

        deductionPanel.SetActive(true);

        ShowCurrentDeduction();
    }

    private void ShowCurrentDeduction()
    {
        characterNameText.text = "Kael Orion";

        dialogueText.text = deductionLines[deductionIndex];

        if (deductionIndex >= deductionLines.Length - 1)
        {
            continueButton.GetComponentInChildren<TMP_Text>().text =
                "Finish Investigation";
        }
        else
        {
            continueButton.GetComponentInChildren<TMP_Text>().text =
                "Continue";
        }
    }

    private void ShowNextDeduction()
    {
        deductionIndex++;

        if (deductionIndex >= deductionLines.Length)
        {
            ShowCaseResolution();
            return;
        }

        ShowCurrentDeduction();
    }

    private void ShowCaseResolution()
    {
        deductionPanel.SetActive(false);
        resultPanel.SetActive(true);

        resultTitle.text = "CASE CLOSED";

        resultDescription.text =
            "Mira Kane has been identified as the culprit.\n\n" +
            "The murder of Evan Cole has been solved.";
    }

    public void CloseWrongAccusation()
    {
        wrongAccusationPanel.SetActive(false);
        selectedSuspectText.text = "No suspect selected.";
        accuseButton.interactable = false;
    }

    private void HideAll()
    {
        conclusionPanel.SetActive(false);
        deductionPanel.SetActive(false);
        resultPanel.SetActive(false);
        wrongAccusationPanel.SetActive(false);
    }
    private void OnDisable()
    {
        Debug.LogError(">>> CaseConclusionManager WAS DISABLED!");
    }
    public void CloseCaseConclusion()
    {
        conclusionPanel.SetActive(false);

        selectedSuspect = default;

        if (selectedSuspectText != null)
            selectedSuspectText.text = "No suspect selected.";

        if (accuseButton != null)
            accuseButton.interactable = false;

        if (cameraController != null)
            cameraController.EnableGameplayInput();
    }
    
    private void CheckCaseUnlock()
    {
        bool hasEnoughEvidence = GetCollectedEvidenceCount() >= requiredEvidenceCount;

        bool conversationCompleted =
            StoryManager.Instance != null &&
            StoryManager.Instance.IsConversationCompleted(
                requiredConversationId
            );

        canOpenCase = hasEnoughEvidence && conversationCompleted;

        Debug.Log(
            $"Case Unlock | Evidence: {hasEnoughEvidence} | " +
            $"Jace Conversation: {conversationCompleted} | " +
            $"Can Open Case: {canOpenCase}"
        );
    }
    private int GetCollectedEvidenceCount()
    {
        // TODO: lấy số evidence đã collect từ EvidenceManager
        return 0;
    }
}