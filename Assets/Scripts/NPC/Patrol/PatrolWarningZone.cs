using UnityEngine;

public class PatrolWarningZone : MonoBehaviour
{
    private BoxCollider zoneCollider;
    private bool isWarningActive;

    public bool IsPlayerInside { get; private set; }

    private void Awake()
    {
        zoneCollider = GetComponent<BoxCollider>();
    }

    public void StartWarning(Transform player)
    {
        isWarningActive = true;
        IsPlayerInside = false;

        CheckPlayer(player);
    }

    public void CheckPlayer(Transform player)
    {
        if (!isWarningActive || player == null || zoneCollider == null)
        {
            return;
        }

        Vector3 localPlayerPosition = transform.InverseTransformPoint(player.position);
        Vector3 halfSize = zoneCollider.size * 0.5f;

        bool insideX = Mathf.Abs(localPlayerPosition.x - zoneCollider.center.x) <= halfSize.x;
        bool insideY = Mathf.Abs(localPlayerPosition.y - zoneCollider.center.y) <= halfSize.y;
        bool insideZ = Mathf.Abs(localPlayerPosition.z - zoneCollider.center.z) <= halfSize.z;

        IsPlayerInside = insideX && insideY && insideZ;
    }

    public void StopWarning()
    {
        isWarningActive = false;
        IsPlayerInside = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isWarningActive)
        {
            return;
        }

        if (other.CompareTag("Player") || other.transform.root.CompareTag("Player"))
        {
            IsPlayerInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isWarningActive)
        {
            return;
        }

        if (other.CompareTag("Player") || other.transform.root.CompareTag("Player"))
        {
            IsPlayerInside = false;
        }
    }
}