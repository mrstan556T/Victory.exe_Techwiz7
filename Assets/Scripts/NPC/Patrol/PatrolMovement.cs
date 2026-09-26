using UnityEngine;

public class PatrolMovement : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private Transform[] stopWaypoints;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float stopDuration = 15f;
    [SerializeField] private Animator animator;

    private int currentWaypointIndex;
    private float stopTimer;
    private bool isStopping;

    private void Update()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            return;
        }

        if (isStopping)
        {
            HandleStop();
            return;
        }

        Transform target = waypoints[currentWaypointIndex];

        RotateTowards(target);

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
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

    private void RotateTowards(Transform target)
    {
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
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
}