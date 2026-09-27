using UnityEngine;

public class PatrolResponse : MonoBehaviour
{
    [SerializeField] private RestrictedArea restrictedArea;
    [SerializeField] private PatrolDetection patrolDetection;
    [SerializeField] private PatrolWarningZone warningZone;
    [SerializeField] private Transform blockingPoint;
    [SerializeField] private Animator animator;

    private PatrolMovement patrolMovement;
    private Transform responseTarget;

    public bool IsBlocking { get; private set; }
    public bool IsWarning { get; private set; }
    public bool IsSecurityViolation { get; private set; }

    private void Awake()
    {
        patrolMovement = GetComponent<PatrolMovement>();
    }

    private void Update()
    {
        if (restrictedArea == null ||
            patrolDetection == null ||
            warningZone == null ||
            patrolMovement == null)
        {
            return;
        }

        //start security violation
        if (!IsSecurityViolation &&
            restrictedArea.IsPlayerInside &&
            patrolDetection.CurrentState == PatrolDetection.DetectionState.Detected)
        {
            StartSecurityViolation();
        }

        if (!IsSecurityViolation)
        {
            return;
        }

        //stop warning when player leaves warning zone
        if (IsWarning)
        {
            warningZone.CheckPlayer(responseTarget);

            if (!warningZone.IsPlayerInside)
            {
                StopWarning();
            }

            return;
        }

        //stop blocking when player leaves restricted area
        if (!restrictedArea.IsPlayerInside)
        {
            StopSecurityViolation();
            return;
        }

        //start warning
        if (IsBlocking && patrolMovement.IsAtOverrideTarget)
        {
            StartWarning();
            return;
        }

        //face player while blocking
        if (patrolDetection.IsPlayerCurrentlyVisible)
        {
            responseTarget = patrolDetection.DetectedPlayer;
            FacePlayer(responseTarget);
        }
        else
        {
            FaceLastKnownPosition();
        }
    }

    private void StartSecurityViolation()
    {
        if (blockingPoint == null)
        {
            return;
        }

        responseTarget = patrolDetection.DetectedPlayer;

        if (responseTarget == null)
        {
            return;
        }

        IsSecurityViolation = true;
        IsBlocking = true;

        //move to blocking point
        patrolMovement.MoveToOverrideTarget(blockingPoint);

        Debug.Log("Security violation started.");
        Debug.Log("Patrol is moving to blocking point.");
    }

    private void StartWarning()
    {
        if (responseTarget == null)
        {
            return;
        }

        IsBlocking = false;
        IsWarning = true;

        //activate warning zone
        warningZone.StartWarning(responseTarget);

        //face player
        FacePlayer(responseTarget);

        //play warning animation
        if (animator != null)
        {
            animator.Play("Warning");
        }

        Debug.Log("Patrol is warning the player.");
        Debug.Log("Player inside warning zone: " + warningZone.IsPlayerInside);
    }

    private void StopWarning()
    {
        IsWarning = false;

        warningZone.StopWarning();

        //resume patrol
        patrolMovement.ResumePatrol();

        if (animator != null)
        {
            animator.Play("Walking");
        }

        IsSecurityViolation = false;
        responseTarget = null;

        Debug.Log("Player left warning zone.");
        Debug.Log("Patrol resumed patrol.");
    }

    private void StopSecurityViolation()
    {
        IsBlocking = false;
        IsWarning = false;
        IsSecurityViolation = false;

        warningZone.StopWarning();
        responseTarget = null;

        //resume patrol
        patrolMovement.ResumePatrol();

        if (animator != null)
        {
            animator.Play("Walking");
        }

        Debug.Log("Security violation ended.");
        Debug.Log("Patrol resumed patrol.");
    }

    private void FacePlayer(Transform player)
    {
        if (player == null)
        {
            return;
        }

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            5f * Time.deltaTime
        );
    }

    private void FaceLastKnownPosition()
    {
        Vector3 direction = patrolDetection.LastKnownPlayerPosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            5f * Time.deltaTime
        );
    }
}