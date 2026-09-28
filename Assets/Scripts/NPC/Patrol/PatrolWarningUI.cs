using UnityEngine;
using UnityEngine.UI;

public class PatrolWarningUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PatrolResponse patrolResponse;
    [SerializeField] private Image redOverlay;

    [Header("Blink")]
    [SerializeField] private float blinkSpeed = 5f;
    [SerializeField] private float maxAlpha = 0.35f;

    private void Awake()
    {
        if (redOverlay != null)
        {
            Color color = redOverlay.color;
            color.a = 0f;
            redOverlay.color = color;
        }
    }

    private void Update()
    {
        if (patrolResponse == null ||
            redOverlay == null)
        {
            return;
        }

        if (patrolResponse.IsWarning)
        {
            ShowWarning();
        }
        else
        {
            HideWarning();
        }
    }

    private void ShowWarning()
    {
        float alpha =
            (Mathf.Sin(Time.time * blinkSpeed) + 1f) * 0.5f;

        alpha *= maxAlpha;

        Color color = redOverlay.color;
        color.a = alpha;
        redOverlay.color = color;
    }

    private void HideWarning()
    {
        Color color = redOverlay.color;
        color.a = 0f;
        redOverlay.color = color;
    }
}