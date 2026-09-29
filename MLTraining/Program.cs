using System.Text.Json;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;

public class PatrolTrainingData
{
    [LoadColumn(0)]
    public float DetectionState { get; set; }

    [LoadColumn(1)]
    public float RecentPlayerActivity { get; set; }

    [LoadColumn(2)]
    public float LastKnownPlayerAvailable { get; set; }

    [LoadColumn(3)]
    public int Action { get; set; }
}

public class PatrolNormalizedTrainingData
{
    [VectorType(3)]
    public float[] Features { get; set; } =
        Array.Empty<float>();

    public int Action { get; set; }
}

public class PatrolModel
{
    //action-major weights
    //action 0 = index 0-2
    //action 1 = index 3-5
    //action 2 = index 6-8
    public float[] Weights { get; set; } = Array.Empty<float>();
    public float[] Biases { get; set; } = Array.Empty<float>();
    public string[] Actions { get; set; } =
    {
        "ContinueRoute",
        "InvestigateLocation",
        "AlternateRoute"
    };
}

public static class Program
{
    public static void Main()
    {
        MLContext mlContext = new MLContext(seed: 42);

        string datasetPath = Path.GetFullPath(
                Path.Combine(
                    "..",
                    "Assets",
                    "ML",
                    "Dataset",
                    "patrol_training.csv"
                )
            );

        string modelPath = Path.GetFullPath(
                Path.Combine(
                    "..",
                    "Assets",
                    "ML",
                    "Models",
                    "patrol_model.json"
                )
            );

        //load the training dataset
        IDataView rawData =
            mlContext.Data.LoadFromTextFile<PatrolTrainingData>(
                datasetPath,
                hasHeader: true,
                separatorChar: ','
            );

        List<PatrolTrainingData> rawSamples = mlContext.Data.CreateEnumerable<PatrolTrainingData>(rawData, reuseRowObject: false).ToList();

        //normalize observations using the runtime ranges
        List<PatrolNormalizedTrainingData> normalizedSamples = new List<PatrolNormalizedTrainingData>();

        foreach (PatrolTrainingData sample in rawSamples)
        {
            PatrolNormalizedTrainingData normalizedSample = new PatrolNormalizedTrainingData
                {
                    Features = new float[]
                    {
                        sample.DetectionState / 2.0f,
                        sample.RecentPlayerActivity,
                        sample.LastKnownPlayerAvailable
                    },
                    Action = sample.Action
                };

            normalizedSamples.Add(normalizedSample);
        }

        IDataView normalizedData = mlContext.Data.LoadFromEnumerable(normalizedSamples);

        // convert action values into ML.NET labels
        IEstimator<ITransformer> labelMapping =
            mlContext.Transforms.Conversion.MapValueToKey(
                outputColumnName: "Label",
                inputColumnName: nameof(PatrolNormalizedTrainingData.Action));

        ITransformer labelTransformer = labelMapping.Fit(normalizedData);

        IDataView keyedData = labelTransformer.Transform(normalizedData);

        //split the dataset for validation
        DataOperationsCatalog.TrainTestData split = mlContext.Data.TrainTestSplit(
                keyedData,
                testFraction: 0.25
            );

        //create the multiclass classifier
        SdcaMaximumEntropyMulticlassTrainer trainer = mlContext.MulticlassClassification.Trainers
                .SdcaMaximumEntropy(
                    labelColumnName: "Label",
                    featureColumnName:
                        nameof(
                            PatrolNormalizedTrainingData.Features
                        )
                );

        //train the model
        MulticlassPredictionTransformer<MaximumEntropyModelParameters> classifier = trainer.Fit(split.TrainSet);

        //validate the model
        IDataView predictions = classifier.Transform(split.TestSet);

        MulticlassClassificationMetrics metrics = mlContext.MulticlassClassification.Evaluate(
                predictions,
                labelColumnName: "Label",
                predictedLabelColumnName: "PredictedLabel"
            );

        Console.WriteLine($"Validation accuracy: {metrics.MicroAccuracy:P2}");

        //get trained model parameters
        MaximumEntropyModelParameters parameters = classifier.Model;

        VBuffer<float>[] weightBuffers = Array.Empty<VBuffer<float>>();

        int numberOfClasses;

        parameters.GetWeights(
            ref weightBuffers,
            out numberOfClasses
        );

        float[] biases = parameters.GetBiases().ToArray();

        if (numberOfClasses != 3)
        {
            throw new InvalidOperationException("Expected exactly 3 classes.");
        }

        if (weightBuffers.Length != 3)
        {
            throw new InvalidOperationException("Expected weights for exactly 3 classes.");
        }

        if (biases.Length != 3)
        {
            throw new InvalidOperationException("Expected exactly 3 biases.");
        }

        //flatten the weights for Unity JsonUtility
        float[] weights = new float[9];

        for (int actionIndex = 0;
             actionIndex < 3;
             actionIndex++)
        {
            float[] actionWeights = weightBuffers[actionIndex].DenseValues().ToArray();

            if (actionWeights.Length != 3)
            {
                throw new InvalidOperationException(
                    $"Expected 3 weights for action " +
                    $"{actionIndex}."
                );
            }

            for (int featureIndex = 0;
                 featureIndex < 3;
                 featureIndex++)
            {
                weights[actionIndex * 3 + featureIndex] = actionWeights[featureIndex];
            }
        }

        //create the runtime model
        PatrolModel patrolModel =
            new PatrolModel
            {
                Weights = weights,
                Biases = biases
            };

        JsonSerializerOptions jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };

        string json = JsonSerializer.Serialize(patrolModel,
                jsonOptions
            );

        //create the output directory if necessary
        string? modelDirectory =
            Path.GetDirectoryName(modelPath);

        if (modelDirectory != null)
        {
            Directory.CreateDirectory(
                modelDirectory
            );
        }

        //save the runtime model
        File.WriteAllText(
            modelPath,
            json
        );

        Console.WriteLine();
        Console.WriteLine("trained model parameters:");

        for (int actionIndex = 0;
             actionIndex < 3;
             actionIndex++)
        {
            Console.WriteLine(
                $"action {actionIndex}: " +
                $"{patrolModel.Actions[actionIndex]}"
            );

            Console.WriteLine(
                $"  weights: " +
                $"[{weights[actionIndex * 3]:F6}, " +
                $"{weights[actionIndex * 3 + 1]:F6}, " +
                $"{weights[actionIndex * 3 + 2]:F6}]"
            );

            Console.WriteLine(
                $"  bias: " +
                $"{biases[actionIndex]:F6}"
            );
        }

        Console.WriteLine();
        Console.WriteLine(
            $"Model exported: {modelPath}"
        );
    }
}