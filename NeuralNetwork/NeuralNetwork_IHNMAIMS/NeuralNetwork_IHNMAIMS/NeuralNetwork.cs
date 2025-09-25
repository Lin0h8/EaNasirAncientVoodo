using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork_IHNMAIMS
{
    public class NetworkData
    {
        public int[] Layers { get; set; }
        public ActivationType[] Activations { get; set; }
        public float[][][] Weights { get; set; }
        public float[][] Biases { get; set; }
    }

    public enum ActivationType
    {
        Sigmoid,
        ReLU,
        Tanh,
        Softmax
    }

    public class NeuralNetwork
    {
        private int[] layers;
        private float[][] neurons;
        private float[][][] weights;
        private float[][] biases;
        private ActivationType[] activations;
        private float[][][] weightGradients;
        private float[][] biasGradients;

        public NeuralNetwork(int[] layers, ActivationType[] activations)
        {
            if ((layers.Length - 1) != activations.Length)
                throw new ArgumentException("Each layer must have an activation type.");

            this.layers = layers;
            this.activations = activations;
            InitializeNeurons();
            InitializeWeights();
        }

        private void InitializeNeurons()
        {
            neurons = new float[layers.Length][];
            for (int i = 0; i < layers.Length; i++)
            {
                neurons[i] = new float[layers[i]];
            }
        }

        private void InitializeWeights()
        {
            Random rand = new Random();
            weights = new float[layers.Length - 1][][];
            biases = new float[layers.Length - 1][];
            for (int i = 0; i < layers.Length - 1; i++)
            {
                weights[i] = new float[layers[i + 1]][];
                biases[i] = new float[layers[i + 1]];
                for (int j = 0; j < layers[i + 1]; j++)
                {
                    weights[i][j] = new float[layers[i]];
                    biases[i][j] = (float)(rand.NextDouble() * 2 - 1);
                    for (int k = 0; k < layers[i]; k++)
                    {
                        float limit = (float)Math.Sqrt(6.0 / (layers[i] + layers[i + 1]));
                        weights[i][j][k] = (float)(rand.NextDouble() * 2 - 1) * limit;
                    }
                }
            }
            weightGradients = new float[layers.Length - 1][][];
            biasGradients = new float[layers.Length - 1][];
            for (int i = 0; i < layers.Length - 1; i++)
            {
                weightGradients[i] = new float[layers[i + 1]][];
                biasGradients[i] = new float[layers[i + 1]];
                for (int j = 0; j < layers[i + 1]; j++)
                {
                    weightGradients[i][j] = new float[layers[i]];
                }
            }
        }

        public float[] FeedForward(float[] inputs)
        {
            if (inputs.Length != layers[0])
                throw new ArgumentException("Input size does not match network input layer size.");
            for (int i = 0; i < layers[0]; i++)
            {
                neurons[0][i] = inputs[i];
            }
            for (int i = 1; i < layers.Length; i++)
            {
                float[] tempValues = new float[layers[i]];
                for (int j = 0; j < layers[i]; j++)
                {
                    float sum = biases[i - 1][j];
                    for (int k = 0; k < layers[i - 1]; k++)
                    {
                        sum += weights[i - 1][j][k] * neurons[i - 1][k];
                    }
                    tempValues[j] = sum;
                }

                if (i == layers.Length - 1 && activations[i - 1] == ActivationType.Softmax)
                {
                    neurons[i] = Softmax(tempValues);
                }
                else
                {
                    for (int j = 0; j < layers[i]; j++)
                    {
                        neurons[i][j] = ApplyActivation(tempValues[j], activations[i - 1]);
                    }
                }
            }
            return neurons[neurons.Length - 1];
        }

        private float ApplyActivation(float x, ActivationType type)
        {
            return type switch
            {
                ActivationType.Sigmoid => Sigmoid(x),
                ActivationType.ReLU => ReLU(x),
                ActivationType.Tanh => (float)Math.Tanh(x),
                _ => throw new ArgumentOutOfRangeException(nameof(type), "Unsupported activation type"),
            };
        }

        private float ActivationDerivative(float x, ActivationType type)
        {
            return type switch
            {
                ActivationType.Sigmoid => SigmoidDerivative(x),
                ActivationType.ReLU => ReLUDerivative(x),
                ActivationType.Tanh => 1 - x * x,
                _ => throw new ArgumentOutOfRangeException(nameof(type), "Unsupported activation type"),
            };
        }

        private static float Sigmoid(float x)
        {
            return 1 / (1 + (float)Math.Exp(-x));
        }

        private static float SigmoidDerivative(float x)
        {
            return x * (1 - x);
        }

        public static float ReLU(float x)
        {
            return Math.Max(0, x);
        }

        public static float ReLUDerivative(float x)
        {
            return x > 0 ? 1 : 0;
        }

        public void Train(float[][] inputs, float[][] expectedOutputs, int epochs, float learningRate, int batchSize = 1)
        {
            Random rand = new Random();

            for (int epoch = 0; epoch < epochs; epoch++)
            {
                var indices = Enumerable.Range(0, inputs.Length).OrderBy(x => rand.Next()).ToArray();
                float totalLoss = 0;
                for (int batchStart = 0; batchStart < inputs.Length; batchStart += batchSize)
                {
                    int actualBatchSize = Math.Min(batchSize, inputs.Length - batchStart);
                    for (int i = 0; i < actualBatchSize; i++)
                    {
                        int idx = indices[batchStart + i];
                        var output = FeedForward(inputs[idx]);
                        Backpropagate(expectedOutputs[idx]);
                        totalLoss += CrossEntropyLoss(output, expectedOutputs[idx]);
                    }
                    ApplyGradients(learningRate, actualBatchSize);
                }

                Console.WriteLine($"Epoch {epoch + 1}/{epochs}, Loss: {totalLoss / inputs.Length}");
            }
        }

        private void Backpropagate(float[] expectedOutputs)
        {
            float[][] errors = new float[layers.Length][];
            for (int i = 0; i < layers.Length; i++)
            {
                errors[i] = new float[layers[i]];
            }
            for (int i = 0; i < layers[layers.Length - 1]; i++)
            {
                errors[layers.Length - 1][i] = neurons[layers.Length - 1][i] - expectedOutputs[i];
            }
            for (int i = layers.Length - 2; i >= 0; i--)
            {
                float[] deltas = new float[layers[i + 1]];
                for (int j = 0; j < layers[i + 1]; j++)
                {
                    if (activations[i] != ActivationType.Softmax)
                        deltas[j] = errors[i + 1][j] * ActivationDerivative(neurons[i + 1][j], activations[i]);
                    else
                        deltas[j] = errors[i + 1][j];
                }

                for (int j = 0; j < layers[i]; j++)
                {
                    float errorSum = 0;
                    for (int k = 0; k < layers[i + 1]; k++)
                    {
                        errorSum += deltas[k] * weights[i][k][j];
                    }

                    errors[i][j] = errorSum;
                }

                for (int j = 0; j < layers[i + 1]; j++)
                {
                    biasGradients[i][j] += deltas[j];
                    for (int k = 0; k < layers[i]; k++)
                    {
                        weightGradients[i][j][k] += deltas[j] * neurons[i][k];
                    }
                }
            }
        }

        private void ApplyGradients(float learningRate, int batchSize)
        {
            for (int i = 0; i < layers.Length - 1; i++)
            {
                for (int j = 0; j < layers[i + 1]; j++)
                {
                    biases[i][j] -= learningRate * (biasGradients[i][j] / batchSize);
                    biasGradients[i][j] = 0;
                    for (int k = 0; k < layers[i]; k++)
                    {
                        weights[i][j][k] -= learningRate * (weightGradients[i][j][k] / batchSize);
                        weightGradients[i][j][k] = 0;
                    }
                }
            }
        }

        private float CrossEntropyLoss(float[] predicted, float[] expected)
        {
            float loss = 0;
            for (int i = 0; i < predicted.Length; i++)
            {
                float p = Math.Clamp(predicted[i], 1e-7f, 1 - 1e-7f);
                loss -= expected[i] * (float)Math.Log(p);
            }
            return loss;
        }

        private float[] Softmax(float[] logits)
        {
            float maxLogit = logits.Max();
            float sumExp = 0f;
            float[] expValues = new float[logits.Length];

            for (int i = 0; i < logits.Length; i++)
            {
                expValues[i] = (float)Math.Exp(logits[i] - maxLogit);
                sumExp += expValues[i];
            }
            for (int i = 0; i < logits.Length; i++)
            {
                expValues[i] /= sumExp;
            }
            return expValues;
        }

        public void Save(string filePath)
        {
            var data = new NetworkData
            {
                Layers = layers,
                Activations = activations,
                Weights = weights,
                Biases = biases
            };
            var options = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            };
            var json = System.Text.Json.JsonSerializer.Serialize(data, options);
            System.IO.File.WriteAllText(filePath, json);
        }

        public static NeuralNetwork Load(string filePath)
        {
            var json = System.IO.File.ReadAllText(filePath);
            var data = System.Text.Json.JsonSerializer.Deserialize<NetworkData>(json);
            if (data == null)
                throw new Exception("Failed to deserialize network data.");
            var nn = new NeuralNetwork(data.Layers, data.Activations)
            {
                weights = data.Weights,
                biases = data.Biases
            };
            return nn;
        }
    }
}