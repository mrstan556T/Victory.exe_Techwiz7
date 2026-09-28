using System.Collections;
using TMPro;
using UnityEngine;

public class StoryIntroUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject darkScenePanel;
    [SerializeField] private TMP_Text introText;

    [Header("Typewriter")]
    [SerializeField] private float characterDelay = 0.03f;

    [Header("Timing")]
    [SerializeField] private float delayAfterText = 1.5f;

    private Coroutine introCoroutine;

    public bool IsPlaying { get; private set; }

    private void Update()
    {
        if (!IsPlaying)
            return;

        if (Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SkipIntro();
        }
    }

    public void PlayIntro(StoryEvent[] events)
    {
        if (events == null || events.Length == 0)
            return;

        if (introCoroutine != null)
        {
            StopCoroutine(introCoroutine);
        }

        introCoroutine = StartCoroutine(
            PlayIntroSequence(events)
        );
    }

    private IEnumerator PlayIntroSequence(
        StoryEvent[] events)
    {
        IsPlaying = true;

        darkScenePanel.SetActive(true);

        introText.text = "";

        foreach (StoryEvent storyEvent in events)
        {
            if (storyEvent.type != "narration")
                continue;

            yield return StartCoroutine(
                TypeText(storyEvent.text)
            );

            yield return new WaitForSeconds(
                delayAfterText
            );

            introText.text = "";
        }

        darkScenePanel.SetActive(false);

        IsPlaying = false;

        introCoroutine = null;
    }

    private IEnumerator TypeText(string text)
    {
        introText.text = "";

        foreach (char character in text)
        {
            introText.text += character;

            yield return new WaitForSeconds(
                characterDelay
            );
        }
    }

    public void SkipIntro()
    {
        if (!IsPlaying)
            return;

        if (introCoroutine != null)
        {
            StopCoroutine(introCoroutine);
            introCoroutine = null;
        }

        introText.text = "";

        darkScenePanel.SetActive(false);

        IsPlaying = false;
    }
}