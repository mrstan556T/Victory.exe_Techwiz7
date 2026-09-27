using UnityEngine;

public class RestrictedArea : MonoBehaviour
{
    private bool playerInside;

    public bool IsPlayerInside => playerInside;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInside = true;

        Debug.Log("Player entered restricted area.");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInside = false;

        Debug.Log("Player left restricted area.");
    }
}