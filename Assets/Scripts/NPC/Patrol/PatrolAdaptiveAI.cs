using System;
using UnityEngine;

public class PatrolAdaptiveAI : MonoBehaviour
{
    [Serializable]
    private class PatrolRuntimeModel
    {
        public float[] Weights;
        public float[] Biases;
        public string[] Actions;
    }

    [Header("References")]
    [SerializeField] private PatrolAdaptiveObservation observation;
    [SerializeField] private PatrolAdaptiveActionController actionController;
    [SerializeField] private PatrolResponse patrolResponse;

    [Header("Model")]
    [SerializeField] private TextAsset modelFile;

    [Header("Inference")]
    [SerializeField] private bool runOnStart = true;

    [Header("Execution")]
    [SerializeField] private bool enableAdaptiveExecution;
    [SerializeField] private float decisionInterval = 1f;

    private float decisionTimer;
    private PatrolRuntimeModel runtimeModel;

    private void Awake()
    {
        // get adaptive components
        if (observation == null)
        {
            observation = GetComponent<PatrolAdaptiveObservation>();
        }

        if (actionController == null)
        {
            actionController = GetComponent<PatrolAdaptiveActionController>();
        }

        if (patrolResponse == null)
        {
            patrolResponse = GetComponent<PatrolResponse>();
        }

        // load the runtime model
        LoadModel();
    }

    private void Start()
    {
        // run the first inference
        if (runOnStart)
        {
            RunInference();
        }
    }

    private void Update()
    {
        if (!enableAdaptiveExecution)
        {
            return;
        }

        decisionTimer -= Time.deltaTime;

        if (decisionTimer > 0f)
        {
            return;
        }

        decisionTimer = decisionInterval;

        ExecuteAdaptiveDecision();
    }

    public void ExecuteAdaptiveDecision()
    {
        // preserve phase 02 security response
        if (IsSecurityResponseActive())
        {
            return;
        }

        // run inference and get the selected action
        PatrolAdaptiveAction action = RunInference();

        if (actionController == null)
        {
            return;
        }

        // execute the selected action
        actionController.ExecuteAction(action);
    }

    private bool IsSecurityResponseActive()
    {
        // let patrol response control movement during security violation
        if (patrolResponse == null)
        {
            return false;
        }

        return patrolResponse.IsSecurityViolation;
    }

    private void LoadModel()
    {
        // validate the model file
        if (modelFile == null)
        {
            Debug.LogWarning("PatrolAdaptiveAI: model file is missing.");
            runtimeModel = null;
            return;
        }

        try
        {
            runtimeModel = JsonUtility.FromJson<PatrolRuntimeModel>(modelFile.text);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("PatrolAdaptiveAI: failed to load model. " + exception.Message);
            runtimeModel = null;
            return;
        }

        // validate the model structure
        if (!IsModelValid())
        {
            Debug.LogWarning("PatrolAdaptiveAI: invalid model.");
            runtimeModel = null;
            return;
        }

        Debug.Log("PatrolAdaptiveAI: model loaded successfully.");
    }

    private bool IsModelValid()
    {
        // validate model arrays
        if (runtimeModel == null)
        {
            return false;
        }

        if (runtimeModel.Weights == null || runtimeModel.Weights.Length != 9)
        {
            return false;
        }

        if (runtimeModel.Biases == null || runtimeModel.Biases.Length != 3)
        {
            return false;
        }

        if (runtimeModel.Actions == null || runtimeModel.Actions.Length != 3)
        {
            return false;
        }

        return true;
    }

    public PatrolAdaptiveAction RunInference()
    {
        // use the safe fallback when inference is unavailable
        if (!IsModelValid())
        {
            return PatrolAdaptiveAction.ContinueRoute;
        }

        if (observation == null)
        {
            return PatrolAdaptiveAction.ContinueRoute;
        }

        // get the current observations
        float detection = observation.GetDetectionValue() / 2.0f;
        float recentActivity = observation.GetRecentPlayerActivityValue();
        float lastKnownAvailable = observation.GetLastKnownPlayerAvailableValue();

        float[] features = { detection, recentActivity, lastKnownAvailable };

        // calculate scores for all actions
        float continueScore = CalculateScore(0, features);
        float investigateScore = CalculateScore(1, features);
        float alternateScore = CalculateScore(2, features);

        // select the highest scoring action
        int selectedAction = 0;
        float highestScore = continueScore;

        if (investigateScore > highestScore)
        {
            highestScore = investigateScore;
            selectedAction = 1;
        }

        if (alternateScore > highestScore)
        {
            highestScore = alternateScore;
            selectedAction = 2;
        }

        PatrolAdaptiveAction action = (PatrolAdaptiveAction)selectedAction;

        Debug.Log(
            "PatrolAdaptiveAI | Detection: " +
            detection +
            " | Activity: " +
            recentActivity +
            " | LastKnown: " +
            lastKnownAvailable +
            " | Scores: [" +
            continueScore.ToString("F3") +
            ", " +
            investigateScore.ToString("F3") +
            ", " +
            alternateScore.ToString("F3") +
            "] | Action: " +
            action
        );

        return action;
    }

    private float CalculateScore(int actionIndex, float[] features)
    {
        // calculate the linear model score
        int weightIndex = actionIndex * 3;

        float score = runtimeModel.Biases[actionIndex];

        score += features[0] * runtimeModel.Weights[weightIndex];
        score += features[1] * runtimeModel.Weights[weightIndex + 1];
        score += features[2] * runtimeModel.Weights[weightIndex + 2];

        return score;
    }

    [ContextMenu("Test Runtime Inference")]
    private void TestRuntimeInference()
    {
        // test model inference without executing movement
        RunInference();
    }

    [ContextMenu("Run Inference And Execute")]
    private void RunInferenceAndExecute()
    {
        // preserve phase 02 security response
        if (IsSecurityResponseActive())
        {
            Debug.Log("PatrolAdaptiveAI: execution blocked by security response.");
            return;
        }

        // run inference and pass the action to the controller
        PatrolAdaptiveAction action = RunInference();

        if (actionController == null)
        {
            return;
        }

        actionController.ExecuteAction(action);
    }
}