using UnityEngine;
using UnityEngine.Events;

public class PatrolResponse : MonoBehaviour
{
    public enum ResponseState
    {
        Idle,
        Warning,
        Dialogue,
        Completed
    }

    [Header("References")]
    [SerializeField] private RestrictedArea restrictedArea;
    [SerializeField] private PatrolDetection patrolDetection;
    [SerializeField] private Transform blockingPoint;

    [Header("Warning")]
    [SerializeField] private float warningDuration = 2f;

    [Header("Dialogue")]
    [SerializeField] private UnityEvent onDialogueRequested;

    private PatrolMovement patrolMovement;
    private Transform responseTarget;
    private ResponseState currentState = ResponseState.Idle;
    private float warningTimer;

    public bool IsSecurityViolation =>
        currentState != ResponseState.Idle;

    public bool IsResponseActive =>
        currentState != ResponseState.Idle &&
        currentState != ResponseState.Completed;

    public bool IsWarning =>
        currentState == ResponseState.Warning;

    public bool IsDialogue =>
        currentState == ResponseState.Dialogue;

    public ResponseState CurrentState =>
        currentState;

    public Transform ResponseTarget =>
        responseTarget;

    private void Awake()
    {
        patrolMovement = GetComponent<PatrolMovement>();

        if (restrictedArea == null)
        {
            FindRestrictedArea();
        }

        if (patrolDetection == null)
        {
            patrolDetection = GetComponent<PatrolDetection>();
        }
    }

    private void Update()
{
    if (restrictedArea == null)
    {
        Debug.LogWarning(
            "PatrolResponse: RestrictedArea is missing."
        );

        return;
    }

    if (patrolDetection == null)
    {
        Debug.LogWarning(
            "PatrolResponse: PatrolDetection is missing."
        );

        return;
    }

    if (patrolMovement == null)
    {
        Debug.LogWarning(
            "PatrolResponse: PatrolMovement is missing."
        );

        return;
    }

    if (blockingPoint == null)
    {
        Debug.LogWarning(
            "PatrolResponse: BlockingPoint is missing."
        );

        return;
    }

    if (currentState == ResponseState.Idle)
    {
        CheckForSecurityViolation();
        return;
    }

    if (currentState == ResponseState.Warning)
    {
        UpdateWarning();
        return;
    }
}

    private void CheckForSecurityViolation()
    {
        if (!restrictedArea.IsPlayerInside)
        {
            return;
        }

        if (patrolDetection.CurrentState !=
            PatrolDetection.DetectionState.Detected)
        {
            return;
        }

        StartSecurityResponse();
    }

    private void FindRestrictedArea()
    {
        RestrictedArea[] areas =
            FindObjectsByType<RestrictedArea>(
                FindObjectsSortMode.None
            );

        if (areas.Length > 0)
        {
            restrictedArea = areas[0];
        }
    }

    private void StartSecurityResponse()
    {
        responseTarget =
            patrolDetection.DetectedPlayer;

        if (responseTarget == null)
        {
            return;
        }

        currentState = ResponseState.Warning;
        warningTimer = warningDuration;

        // stop patrol movement during security response
        patrolMovement.StopPatrol();

        Debug.Log("Security response started.");
        Debug.Log("Patrol warning started.");
    }

    private void UpdateWarning()
    {
        warningTimer -= Time.deltaTime;

        if (warningTimer > 0f)
        {
            return;
        }

        StartDialogue();
    }

    private void StartDialogue()
    {
        currentState = ResponseState.Dialogue;

        Debug.Log("Patrol dialogue requested.");

        if (onDialogueRequested != null)
        {
            onDialogueRequested.Invoke();
        }
    }

    public void OnDialogueCompleted()
    {
        if (currentState != ResponseState.Dialogue)
        {
            return;
        }

        Debug.Log("Patrol dialogue completed.");

        TeleportPlayerToBlockingPoint();
    }

    private void TeleportPlayerToBlockingPoint()
    {
        if (responseTarget == null)
        {
            EndResponse();
            return;
        }

        if (blockingPoint == null)
        {
            Debug.LogError(
                "PatrolResponse: BlockingPoint is missing."
            );

            EndResponse();
            return;
        }

        currentState = ResponseState.Completed;

        // teleport player after dialogue
        responseTarget.position =
            blockingPoint.position;

        Debug.Log(
            "Player teleported to BlockingPoint."
        );

        EndResponse();
    }

    private void EndResponse()
    {
        currentState = ResponseState.Idle;
        responseTarget = null;

        patrolMovement.ResumePatrol();

        Debug.Log(
            "Security response completed."
        );

        Debug.Log(
            "Patrol resumed patrol."
        );
    }
}