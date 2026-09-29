using UnityEngine;

public class PatrolAdaptiveActionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PatrolMovement patrolMovement;
    [SerializeField] private PatrolDetection patrolDetection;
    [SerializeField] private PatrolResponse patrolResponse;
    [SerializeField] private RestrictedArea restrictedArea;

    [Header("Investigation")]
    [SerializeField] private Transform investigationPoint;

    private PatrolAdaptiveAction lastAction;
    private bool hasExecutedAction;

    private void Awake()
    {
        //get patrol components
        if (patrolMovement == null)
        {
            patrolMovement = GetComponent<PatrolMovement>();
        }

        if (patrolDetection == null)
        {
            patrolDetection = GetComponent<PatrolDetection>();
        }

        if (patrolResponse == null)
        {
            patrolResponse = GetComponent<PatrolResponse>();
        }

        if (restrictedArea == null)
        {
            restrictedArea = FindFirstObjectByType<RestrictedArea>();
        }
    }

    public void ExecuteAction(PatrolAdaptiveAction action)
    {
        //let security response control movement inside restricted area
        if (restrictedArea != null &&
            restrictedArea.IsPlayerInside)
        {
            Debug.Log(
                "PatrolAdaptiveActionController: " +
                "restricted area response has priority."
            );

            return;
        }

        if (patrolResponse != null &&
            patrolResponse.IsSecurityViolation)
        {
            return;
        }

        if (hasExecutedAction &&
            lastAction == action)
        {
            if (action == PatrolAdaptiveAction.ContinueRoute &&
                patrolMovement != null &&
                patrolMovement.IsAtOverrideTarget)
            {
                patrolMovement.ResumePatrol();
            }

            return;
        }

        lastAction = action;
        hasExecutedAction = true;

        //execute the selected adaptive action
        switch (action)
        {
            case PatrolAdaptiveAction.ContinueRoute:
                ExecuteContinueRoute();
                break;

            case PatrolAdaptiveAction.InvestigateLocation:
                ExecuteInvestigateLocation();
                break;

            case PatrolAdaptiveAction.AlternateRoute:
                ExecuteAlternateRoute();
                break;

            default:
                ExecuteContinueRoute();
                break;
        }
    }

    private void ExecuteContinueRoute()
    {
        //continue the normal patrol route
        if (patrolMovement == null)
        {
            return;
        }

        if (patrolMovement.IsAtOverrideTarget || patrolMovement.IsMovingToOverrideTarget)
        {
            patrolMovement.ResumePatrol();
        }
    }

    private void ExecuteInvestigateLocation()
    {
        //investigate the last known player location
        if (patrolMovement == null ||
            patrolDetection == null ||
            investigationPoint == null)
        {
            ExecuteContinueRoute();
            return;
        }

        if (!patrolDetection.IsPlayerDetected)
        {
            ExecuteContinueRoute();
            return;
        }

        investigationPoint.position =
            patrolDetection.LastKnownPlayerPosition;

        patrolMovement.MoveToOverrideTarget(
            investigationPoint
        );

        Debug.Log(
            "Patrol is investigating the last known player location."
        );
    }

    private void ExecuteAlternateRoute()
    {
        //alternate route will be connected after the route extension
        Debug.Log(
            "AlternateRoute action selected."
        );
    }

    [ContextMenu("Test Continue Route")]
    private void TestContinueRoute()
    {
        //test continue route
        ExecuteAction(
            PatrolAdaptiveAction.ContinueRoute
        );
    }

    [ContextMenu("Test Investigate Location")]
    private void TestInvestigateLocation()
    {
        //test investigate location
        ExecuteAction(
            PatrolAdaptiveAction.InvestigateLocation
        );
    }

    [ContextMenu("Test Alternate Route")]
    private void TestAlternateRoute()
    {
        //test alternate route
        ExecuteAction(
            PatrolAdaptiveAction.AlternateRoute
        );
    }
}