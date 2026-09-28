using UnityEngine;

[CreateAssetMenu(
    fileName = "SuspectData",
    menuName = "Cyberpunk Detective/Suspect Data"
)]
public class SuspectData : ScriptableObject
{
    public string suspectId;
    public string displayName;
    public string role;
}