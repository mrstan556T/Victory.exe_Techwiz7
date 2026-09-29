using UnityEngine;
using UnityEngine.AI;

public class PatrolMovement : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private Transform[] stopWaypoints;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float returnSpeed = 2f;
    [SerializeField] private float followSpeed = 1.5f;
    [SerializeField] private float stopDuration = 15f;
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent agent;

    private int currentWaypointIndex;
    private int previousWaypointIndex;
    private float stopTimer;
    private bool isStopping;
    private bool isReturningToPatrol;
    private Transform overrideTarget;
    private Transform followTarget;
    private Vector3 overrideTargetPosition;
    private bool isAtOverrideTarget;

    public bool IsMovingToOverrideTarget => overrideTarget != null && !isAtOverrideTarget;
    public bool IsAtOverrideTarget => isAtOverrideTarget;

    private void Awake()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }
    }

    private void Update()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        if (agent == null || !agent.isOnNavMesh)
        {
            Debug.LogError("PatrolMovement: Agent is not on NavMesh.");
            return;
        }

        if (followTarget != null)
        {
            FollowTarget();
            return;
        }

        if (overrideTarget != null)
        {
            MoveToOverrideTarget();
            return;
        }

        if (waypoints == null || waypoints.Length == 0)
        {
            return;
        }

        if (isStopping)
        {
            HandleStop();
            return;
        }

        MoveToWaypoint();
    }

    private void MoveToWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            return;
        }

        Transform target = waypoints[currentWaypointIndex];
        if (target == null)
        {
            MoveToNextWaypoint();
            return;
        }

        agent.isStopped = false;
        agent.speed = isReturningToPatrol ? returnSpeed : moveSpeed;

        if (!agent.hasPath || Vector3.Distance(agent.destination, target.position) > 0.1f)
        {
            agent.SetDestination(target.position);
        }

        if (animator != null && agent.velocity.sqrMagnitude > 0.01f)
        {
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Walking"))
            {
                animator.Play("Walking");
            }
        }

        float arrivalThreshold = Mathf.Max(agent.stoppingDistance + 0.3f, 0.5f);
        Vector3 diff = transform.position - target.position;
        diff.y = 0f;
        float horizontalDist = diff.magnitude;

        if (!agent.pathPending && (agent.remainingDistance <= arrivalThreshold || horizontalDist <= arrivalThreshold))
        {
            Debug.Log("Patrol reached waypoint: " + target.name);
            Debug.Log("Is stop waypoint: " + IsStopWaypoint(target));

            agent.isStopped = true;
            agent.ResetPath();
            isReturningToPatrol = false;

            if (IsStopWaypoint(target))
            {
                StartStop();
            }
            else
            {
                MoveToNextWaypoint();
            }
        }
    }

    private void MoveToOverrideTarget()
    {
        if (isAtOverrideTarget)
        {
            return;
        }

        agent.isStopped = false;
        agent.speed = moveSpeed;

        if (!agent.hasPath || Vector3.Distance(agent.destination, overrideTargetPosition) > 0.1f)
        {
            agent.SetDestination(overrideTargetPosition);
        }

        float arrivalThreshold = Mathf.Max(agent.stoppingDistance + 0.3f, 0.5f);
        Vector3 diff = transform.position - overrideTargetPosition;
        diff.y = 0f;
        float horizontalDist = diff.magnitude;

        if (!agent.pathPending && (agent.remainingDistance <= arrivalThreshold || horizontalDist <= arrivalThreshold))
        {
            isAtOverrideTarget = true;
            agent.isStopped = true;
            agent.ResetPath();

            if (animator != null)
            {
                animator.Play("Idle");
            }

            Debug.Log("PatrolMovement reached override target.");
        }
    }

    private void FollowTarget()
    {
        if (followTarget == null)
        {
            return;
        }

        agent.isStopped = false;
        agent.speed = followSpeed;

        if (!agent.hasPath || Vector3.Distance(agent.destination, followTarget.position) > 0.5f)
        {
            agent.SetDestination(followTarget.position);
        }

        if (animator == null)
        {
            return;
        }

        if (agent.velocity.sqrMagnitude > 0.01f)
        {
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Walking"))
            {
                animator.Play("Walking");
            }
        }
    }

    private bool IsStopWaypoint(Transform waypoint)
    {
        if (stopWaypoints == null || waypoint == null)
        {
            return false;
        }

        foreach (Transform stopWaypoint in stopWaypoints)
        {
            if (stopWaypoint == null)
            {
                continue;
            }

            if (stopWaypoint == waypoint)
            {
                return true;
            }

            if (string.Equals(stopWaypoint.name, waypoint.name, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            Vector3 diff = stopWaypoint.position - waypoint.position;
            diff.y = 0f;
            if (diff.sqrMagnitude < 0.25f)
            {
                return true;
            }
        }

        return false;
    }

    private void StartStop()
    {
        isStopping = true;
        stopTimer = stopDuration;
        agent.isStopped = true;
        agent.ResetPath();

        if (animator != null)
        {
            animator.Play("PatrolAction");
        }

        Debug.Log("Patrol stopped for " + stopDuration + " seconds.");
    }

    private void HandleStop()
    {
        agent.isStopped = true;
        stopTimer -= Time.deltaTime;

        if (stopTimer <= 0f)
        {
            isStopping = false;
            agent.isStopped = false;

            if (animator != null)
            {
                animator.Play("Walking");
            }

            MoveToNextWaypoint();
        }
    }

    private void MoveToNextWaypoint()
    {
        currentWaypointIndex++;

        if (currentWaypointIndex >= waypoints.Length)
        {
            currentWaypointIndex = 0;
        }

        if (waypoints != null && waypoints.Length > 0 && waypoints[currentWaypointIndex] != null)
        {
            agent.isStopped = false;
            agent.speed = isReturningToPatrol ? returnSpeed : moveSpeed;
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }

    public void MoveToOverrideTarget(Transform target)
    {
        if (target == null)
        {
            return;
        }

        previousWaypointIndex = currentWaypointIndex;
        followTarget = null;
        overrideTarget = target;
        overrideTargetPosition = target.position;
        isStopping = false;
        isReturningToPatrol = false;
        isAtOverrideTarget = false;

        agent.isStopped = false;
        agent.speed = moveSpeed;

        if (animator != null)
        {
            animator.Play("Walking");
        }

        Debug.Log("PatrolMovement target: " + target.name);
        Debug.Log("PatrolMovement target position: " + overrideTargetPosition);
    }

    public void FollowPlayer(Transform target)
    {
        if (target == null)
        {
            return;
        }

        overrideTarget = null;
        followTarget = target;
        isStopping = false;
        isReturningToPatrol = false;
        isAtOverrideTarget = false;

        agent.isStopped = false;
        agent.speed = followSpeed;
    }

    public void ResumePatrol()
    {
        followTarget = null;
        overrideTarget = null;
        isAtOverrideTarget = false;
        isStopping = false;
        isReturningToPatrol = true;
        currentWaypointIndex = previousWaypointIndex;

        agent.isStopped = false;
        agent.speed = returnSpeed;

        if (waypoints != null && waypoints.Length > 0 && waypoints[currentWaypointIndex] != null)
        {
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }

        if (animator != null)
        {
            animator.Play("Walking");
        }

        Debug.Log("PatrolMovement resumed patrol.");
    }

    public void StopPatrol()
    {
        followTarget = null;
        overrideTarget = null;
        isStopping = false;
        isReturningToPatrol = false;
        isAtOverrideTarget = false;

        agent.isStopped = true;
        agent.ResetPath();

        if (animator != null)
        {
            animator.Play("Idle");
        }

        Debug.Log("PatrolMovement stopped.");
    }
}