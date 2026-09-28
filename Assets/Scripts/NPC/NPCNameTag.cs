using System.Collections;
using TMPro;
using UnityEngine;

public class NPCNameTag : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text nameText;

    [Header("Distance")]
    [SerializeField] private float showDistance = 10f;
    [SerializeField] private float hideDistance = 12f;
    [SerializeField] private float fadeSpeed = 5f;

    [Header("Occlusion")]
    [SerializeField] private bool useOcclusion = false;
    [SerializeField] private LayerMask occlusionMask;

    private Camera mainCamera;
    private Transform npcRoot;
    private NPCIdentity npcIdentity;

    private float targetAlpha;

    private void Awake()
    {
        npcRoot = transform.root;

        npcIdentity =
            GetComponentInParent<NPCIdentity>();

        targetAlpha = 0f;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }

    private void Start()
    {
        mainCamera = Camera.main;

        StartCoroutine(
            InitializeNameTag()
        );
    }

    private IEnumerator InitializeNameTag()
    {
        while (StoryManager.Instance == null)
        {
            yield return null;
        }

        LoadNPCName();
    }

    private void LateUpdate()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;

            if (mainCamera == null)
                return;
        }

        UpdateBillboard();
        UpdateVisibility();
        UpdateFade();
    }

    private void LoadNPCName()
    {
        if (npcIdentity == null)
        {
            Debug.LogWarning(
                $"NPCIdentity not found for {gameObject.name}"
            );

            return;
        }

        if (nameText == null)
        {
            Debug.LogWarning(
                $"Name Text not assigned for {gameObject.name}"
            );

            return;
        }

        string displayName =
            StoryManager.Instance.GetNPCDisplayName(
                npcIdentity.NpcId
            );

        nameText.text = displayName;

        Debug.Log(
            $"NPC NameTag loaded: " +
            $"{npcIdentity.NpcId} -> {displayName}"
        );
    }

    private void UpdateBillboard()
    {
        Vector3 direction =
            transform.position -
            mainCamera.transform.position;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation =
                Quaternion.LookRotation(direction);
        }
    }

    private void UpdateVisibility()
    {
        float distance =
            Vector3.Distance(
                mainCamera.transform.position,
                transform.position
            );

        if (distance > hideDistance)
        {
            targetAlpha = 0f;
            return;
        }

        if (distance <= showDistance)
        {
            if (useOcclusion &&
                !IsVisibleFromCamera())
            {
                targetAlpha = 0f;
                return;
            }

            targetAlpha = 1f;
        }
    }

    private bool IsVisibleFromCamera()
    {
        Vector3 direction =
            transform.position -
            mainCamera.transform.position;

        float distance = direction.magnitude;

        if (Physics.Raycast(
            mainCamera.transform.position,
            direction.normalized,
            out RaycastHit hit,
            distance,
            occlusionMask,
            QueryTriggerInteraction.Ignore))
        {
            if (hit.transform == npcRoot ||
                hit.transform.IsChildOf(npcRoot))
            {
                return true;
            }

            return false;
        }

        return true;
    }

    private void UpdateFade()
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha =
            Mathf.MoveTowards(
                canvasGroup.alpha,
                targetAlpha,
                fadeSpeed * Time.deltaTime
            );
    }
}