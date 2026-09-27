using TMPro;
using UnityEngine;

public class PatrolEscalationUI : MonoBehaviour
{
    [SerializeField] private PatrolEscalation patrolEscalation;
    [SerializeField] private TextMeshProUGUI warningText;

    private void Update()
    {
        if (patrolEscalation == null || warningText == null)
        {
            return;
        }

        bool showWarning = patrolEscalation.IsEscalated;

        warningText.gameObject.SetActive(showWarning);

        if (showWarning)
        {
            warningText.text = "You must left this zone.";
        }
    }
}