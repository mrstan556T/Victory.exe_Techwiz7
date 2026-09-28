using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SuspectCardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text roleText;

    [Header("Button")]
    [SerializeField] private Button button;

    private SuspectId suspectId;
    private CaseConclusionManager caseManager;

    public void Setup(
        SuspectId id,
        SuspectData data,
        CaseConclusionManager manager)
    {
        suspectId = id;
        caseManager = manager;

        // Set UI data
        nameText.text = data.displayName;
        roleText.text = data.role;

        // Setup button
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        caseManager.SelectSuspect(suspectId);
    }

}