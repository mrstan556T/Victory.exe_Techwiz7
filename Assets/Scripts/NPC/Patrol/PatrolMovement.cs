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
        Transform target = waypoints[currentWaypointIndex];

        agent.isStopped = false;
        agent.speed = isReturningToPatrol ? returnSpeed : moveSpeed;
        agent.SetDestination(target.position);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
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
        agent.SetDestination(overrideTarget.position);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            isAtOverrideTarget = true;
            agent.isStopped = true;

            if (animator != null)
            {
                animator.Play("Idle");
            }
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
        agent.SetDestination(followTarget.position);

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
        if (stopWaypoints == null)
        {
            return false;
        }

        foreach (Transform stopWaypoint in stopWaypoints)
        {
            if (waypoint == stopWaypoint)
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

        if (animator != null)
        {
            animator.Play("PatrolAction");
        }
    }

    private void HandleStop()
    {
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
        isStopping = false;
        isReturningToPatrol = false;
        isAtOverrideTarget = false;

        agent.isStopped = false;
        agent.speed = moveSpeed;

        if (animator != null)
        {
            animator.Play("Walking");
        }
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
        isReturningToPatrol = true;
        currentWaypointIndex = previousWaypointIndex;

        agent.isStopped = false;
        agent.speed = returnSpeed;

        if (animator != null)
        {
            animator.Play("Walking");
        }
    }
}