using UnityEngine;

public class StoryObjectiveTrigger : MonoBehaviour
{
    [SerializeField] private string objectiveId;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (StoryManager.Instance == null)
            return;

        StoryManager.Instance.CompleteObjective(
            objectiveId
        );
    }
}