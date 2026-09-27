using UnityEngine;

public class PatrolEscalation : MonoBehaviour
{
    [SerializeField] private PatrolWarningZone warningZone;
    [SerializeField] private PatrolResponse patrolResponse;
    [SerializeField] private float escalationDelay = 5f;

    private float warningTimer;

    public bool IsEscalated { get; private set; }

    private void Update()
    {
        if (warningZone == null || patrolResponse == null)
        {
            return;
        }

        if (!patrolResponse.IsWarning)
        {
            ResetEscalation();
            return;
        }

        if (!warningZone.IsPlayerInside)
        {
            ResetEscalation();
            return;
        }

        if (IsEscalated)
        {
            return;
        }

        warningTimer += Time.deltaTime;

        if (warningTimer >= escalationDelay)
        {
            StartEscalation();
        }
    }

    private void StartEscalation()
    {
        IsEscalated = true;

        Debug.Log("Patrol escalation started.");
    }

    private void ResetEscalation()
    {
        warningTimer = 0f;
        IsEscalated = false;
    }
}