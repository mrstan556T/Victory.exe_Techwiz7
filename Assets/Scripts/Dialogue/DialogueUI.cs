using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Button nextButton;

    [Header("Typewriter")]
    [SerializeField] private float characterDelay = 0.03f;

    private Coroutine typingCoroutine;
    private bool isTyping;
    private string currentFullText;

    private void Awake()
    {
        dialoguePanel.SetActive(false);

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextButtonClicked);
        }
    }

    private void Update()
    {
        if (!dialoguePanel.activeSelf)
            return;

        // Enter ?? Next
        if (Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OnNextButtonClicked();
        }
    }

    public void ShowDialogue(DialogueLine line)
    {
        dialoguePanel.SetActive(true);

        speakerNameText.text = line.speakerName;

        StartTyping(line.dialogueText);
    }

    private void StartTyping(string text)
    {
        currentFullText = text;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeText(text));
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;

        dialogueText.text = "";

        foreach (char character in text)
        {
            dialogueText.text += character;

            yield return new WaitForSeconds(characterDelay);
        }

        isTyping = false;
        typingCoroutine = null;
    }

    private void OnNextButtonClicked()
    {
        // N?u text v?n ?ang ch?y ? hi?n full text tr??c
        if (isTyping)
        {
            CompleteTyping();
            return;
        }

        // Text ?ã hi?n ??y ?? ? sang line ti?p theo
        DialogueManager.Instance.NextLine();
    }

    private void CompleteTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialogueText.text = currentFullText;
        isTyping = false;
    }

    public void HideDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialoguePanel.SetActive(false);

        dialogueText.text = "";
        speakerNameText.text = "";

        isTyping = false;
        currentFullText = "";
    }
}