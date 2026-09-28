using UnityEngine;
using TMPro;

public class SuspectSelectionUI : MonoBehaviour
{
    [SerializeField] private TMP_Text selectedSuspectText;

    private SuspectId selectedSuspect;

    public void SelectSuspect(SuspectId suspect)
    {
        selectedSuspect = suspect;

        selectedSuspectText.text =
            $"Selected: {suspect}";
    }

    public SuspectId GetSelectedSuspect()
    {
        return selectedSuspect;
    }
}