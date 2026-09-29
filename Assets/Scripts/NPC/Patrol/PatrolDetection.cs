using UnityEngine;

public class PatrolDetection : MonoBehaviour
{
    public enum DetectionState
    {
        Lost,
        Detected,
        Tracking
    }

    [Header("Detection")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float fieldOfView = 120f;
    [SerializeField] private float memoryTime = 2.5f;
    [SerializeField] private Transform eyePoint;

    [Header("Layers")]
    [SerializeField] private LayerMask obstacleMask;

    private Transform detectedPlayer;
    private Vector3 lastKnownPlayerPosition;
    private float memoryTimer;
    private DetectionState currentState = DetectionState.Lost;

    public bool IsPlayerDetected => currentState != DetectionState.Lost;
    public bool IsPlayerCurrentlyVisible => currentState == DetectionState.Detected;
    public bool IsTracking => currentState == DetectionState.Tracking;
    public Transform DetectedPlayer => detectedPlayer;
    public Vector3 LastKnownPlayerPosition => lastKnownPlayerPosition;
    public DetectionState CurrentState => currentState;

    private void Update()
    {
        Transform player = FindVisiblePlayer();

        if (player != null)
        {
            detectedPlayer = player;
            lastKnownPlayerPosition = player.position;
            memoryTimer = memoryTime;

            SetState(DetectionState.Detected);
            return;
        }

        if (detectedPlayer != null)
        {
            float distance = Vector3.Distance(transform.position, detectedPlayer.position);

            if (distance <= detectionRange && memoryTimer > 0f)
            {
                memoryTimer -= Time.deltaTime;
                SetState(DetectionState.Tracking);
                return;
            }
        }

        detectedPlayer = null;
        memoryTimer = 0f;
        SetState(DetectionState.Lost);
    }

    private Transform FindVisiblePlayer()
    {
        Collider[] colliders = Physics.OverlapSphere(
            eyePoint.position,
            detectionRange
        );

        foreach (Collider targetCollider in colliders)
        {
            if (!targetCollider.CompareTag("Player"))
            {
                continue;
            }

            Transform player = targetCollider.transform;

            if (!IsInsideFieldOfView(player))
            {
                continue;
            }

            if (!HasLineOfSight(player))
            {
                continue;
            }

            return player;
        }

        return null;
    }

    private bool IsInsideFieldOfView(Transform player)
    {
        Vector3 directionToPlayer = player.position - eyePoint.position;
        directionToPlayer.y = 0f;

        if (directionToPlayer.sqrMagnitude <= 0.001f)
        {
            return true;
        }

        float angle = Vector3.Angle(
            eyePoint.forward,
            directionToPlayer
        );

        return angle <= fieldOfView * 0.5f;
    }

    private bool HasLineOfSight(Transform player)
    {
        Vector3 directionToPlayer = player.position - eyePoint.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (Physics.Raycast(
            eyePoint.position,
            directionToPlayer.normalized,
            out RaycastHit hit,
            distanceToPlayer,
            obstacleMask,
            QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        return true;
    }

    private void SetState(DetectionState newState)
    {
        if (currentState == newState)
        {
            return;
        }

        currentState = newState;

        if (currentState == DetectionState.Detected)
        {
            Debug.Log("Player detected.");
        }
        else if (currentState == DetectionState.Tracking)
        {
            Debug.Log("Patrol is tracking the player.");
        }
        else if (currentState == DetectionState.Lost)
        {
            Debug.Log("Player lost.");
        }
    }

    private void OnDrawGizmos()
    {
        if (eyePoint == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            eyePoint.position,
            detectionRange
        );

        Vector3 leftDirection = Quaternion.Euler(
            0f,
            -fieldOfView * 0.5f,
            0f
        ) * eyePoint.forward;

        Vector3 rightDirection = Quaternion.Euler(
            0f,
            fieldOfView * 0.5f,
            0f
        ) * eyePoint.forward;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(
            eyePoint.position,
            leftDirection * detectionRange
        );

        Gizmos.DrawRay(
            eyePoint.position,
            rightDirection * detectionRange
        );

        if (currentState == DetectionState.Tracking)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(
                transform.position,
                lastKnownPlayerPosition
            );

            Gizmos.DrawWireSphere(
                lastKnownPlayerPosition,
                0.3f
            );
        }
    }
}