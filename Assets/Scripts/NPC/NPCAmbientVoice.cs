using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class NPCAmbientVoice : MonoBehaviour
{
    [Header("Voice Clips")]
    [SerializeField] private AudioClip[] voiceClips;

    [Header("Timing")]
    [SerializeField] private float minDelay = 5f;
    [SerializeField] private float maxDelay = 12f;

    [Header("Audio")]
    [SerializeField] private float volume = 1f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 1f;
        audioSource.volume = volume;
    }

    private void Start()
    {
        ScheduleNextVoice();
    }

    private void ScheduleNextVoice()
    {
        float delay = Random.Range(minDelay, maxDelay);

        Invoke(nameof(PlayRandomVoice), delay);
    }

    private void PlayRandomVoice()
    {
        if (voiceClips == null || voiceClips.Length == 0)
        {
            ScheduleNextVoice();
            return;
        }

        AudioClip clip = voiceClips[Random.Range(0, voiceClips.Length)];

        audioSource.PlayOneShot(clip);

        ScheduleNextVoice();
    }
}