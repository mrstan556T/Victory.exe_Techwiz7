using TMPro;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject objectivePanel;
    [SerializeField] private TMP_Text objectiveText;

    public string CurrentObjectiveId { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        HideObjective();
    }

    public void SetObjective(
        string objectiveId,
        string text)
    {
        CurrentObjectiveId = objectiveId;

        if (objectivePanel != null)
        {
            objectivePanel.SetActive(true);
        }

        if (objectiveText != null)
        {
            objectiveText.text = text;
        }

        Debug.Log(
            $"New Objective: {objectiveId} - {text}"
        );
    }

    public void CompleteObjective()
    {
        if (string.IsNullOrEmpty(CurrentObjectiveId))
            return;

        Debug.Log(
            $"Objective completed: {CurrentObjectiveId}"
        );

        CurrentObjectiveId = "";

        HideObjective();
    }

    public void HideObjective()
    {
        if (objectivePanel != null)
        {
            objectivePanel.SetActive(false);
        }
    }
}