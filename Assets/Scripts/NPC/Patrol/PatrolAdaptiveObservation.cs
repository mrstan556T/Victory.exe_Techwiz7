using UnityEngine;

public class PatrolAdaptiveObservation : MonoBehaviour
{
    public enum RecentPlayerActivity
    {
        None,
        Detected
    }

    [Header("References")]
    [SerializeField] private PatrolDetection patrolDetection;

    [Header("Observation")]
    [SerializeField]
    private PatrolDetection.DetectionState detectionState = PatrolDetection.DetectionState.Lost;

    [SerializeField]
    private RecentPlayerActivity recentPlayerActivity = RecentPlayerActivity.None;

    [SerializeField]
    private bool lastKnownPlayerAvailable;

    [Header("Activity Memory")]
    [SerializeField] private float activityMemoryTime = 5f;

    private float activityTimer;

    public PatrolDetection.DetectionState DetectionState => detectionState;

    public RecentPlayerActivity PlayerActivity => recentPlayerActivity;

    public bool LastKnownPlayerAvailable => lastKnownPlayerAvailable;

    private void Awake()
    {
        // get the patrol detection component
        if (patrolDetection == null)
        {
            patrolDetection = GetComponent<PatrolDetection>();
        }
    }

    private void Update()
    {
        // update the adaptive observations
        UpdateDetectionObservation();
        UpdatePlayerActivity();
        UpdateLastKnownPlayerObservation();
    }

    private void UpdateDetectionObservation()
    {
        // read the current detection state
        if (patrolDetection == null)
        {
            detectionState = PatrolDetection.DetectionState.Lost;

            return;
        }

        detectionState = patrolDetection.CurrentState;
    }

    private void UpdatePlayerActivity()
    {
        // record recent player detection
        if (patrolDetection == null)
        {
            recentPlayerActivity = RecentPlayerActivity.None;

            return;
        }

        if (patrolDetection.CurrentState ==
            PatrolDetection.DetectionState.Detected)
        {
            recentPlayerActivity = RecentPlayerActivity.Detected;

            activityTimer = activityMemoryTime;

            return;
        }

        if (activityTimer > 0f)
        {
            activityTimer -= Time.deltaTime;

            recentPlayerActivity = RecentPlayerActivity.Detected;

            return;
        }

        recentPlayerActivity =  RecentPlayerActivity.None;
    }

    private void UpdateLastKnownPlayerObservation()
    {
        //track whether a usable player position is available
        if (patrolDetection == null)
        {
            lastKnownPlayerAvailable = false;
            return;
        }

        if (patrolDetection.IsPlayerDetected)
        {
            lastKnownPlayerAvailable = true;
        }
    }

    public int GetDetectionValue()
    {
        //convert detection state into the ML value
        switch (detectionState)
        {
            case PatrolDetection.DetectionState.Detected:
                return 1;

            case PatrolDetection.DetectionState.Tracking:
                return 2;

            case PatrolDetection.DetectionState.Lost:
            default:
                return 0;
        }
    }

    public int GetRecentPlayerActivityValue()
    {
        //convert recent activity into the ML value
        if (recentPlayerActivity == RecentPlayerActivity.Detected)
        {
            return 1;
        }

        return 0;
    }

    public int GetLastKnownPlayerAvailableValue()
    {
        //convert last known player availability into the ML value
        if (lastKnownPlayerAvailable)
        {
            return 1;
        }

        return 0;
    }

    public Vector3 GetLastKnownPlayerPosition()
    {
        //return the last known player position
        if (patrolDetection == null)
        {
            return Vector3.zero;
        }

        return patrolDetection.LastKnownPlayerPosition;
    }

    [ContextMenu("Test Adaptive Observation")]
    private void TestAdaptiveObservation()
    {
        //test all adaptive observations
        Debug.Log(
            "Detection: " + GetDetectionValue() + " | Activity: " + GetRecentPlayerActivityValue() +  " | LastKnown: " +  GetLastKnownPlayerAvailableValue()
        );
    }
}