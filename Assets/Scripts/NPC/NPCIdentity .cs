using UnityEngine;

public class NPCIdentity : MonoBehaviour
{
    [Header("NPC Identity")]
    [SerializeField] private string npcId;

    public string NpcId => npcId;
}