using System.Net.NetworkInformation;

namespace NeuralNetwork_IHNMAIMS
{
    //IHNMAIMS: I Have No Mouth And I Must Scream
    //Neural Network V.1.0
    //(c) 2025 Genesis Symmetry
    //Created by: Melvin "Asymetr1c" Abrahamsson (Owner of Genesis Symmetry)
    internal class Program
    {
        private static bool forceNew = false;
        private static int epochs = 0;

        private static void Main(string[] args)
        {
            Console.Title = "Neural Network V.1.0 (IHNMAIMS) - Genesis Symmetry";
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.SetWindowSize((int)(Console.LargestWindowWidth / 1.5), (int)(Console.LargestWindowHeight / 1.5));
            Console.WriteLine(@"
__/\\\\\\\\\\\__/\\\________/\\\__/\\\\\_____/\\\__/\\\\____________/\\\\_____/\\\\\\\\\_____/\\\\\\\\\\\__/\\\\____________/\\\\_____/\\\\\\\\\\\___
 _\/////\\\///__\/\\\_______\/\\\_\/\\\\\\___\/\\\_\/\\\\\\________/\\\\\\___/\\\\\\\\\\\\\__\/////\\\///__\/\\\\\\________/\\\\\\___/\\\/////////\\\_
  _____\/\\\_____\/\\\_______\/\\\_\/\\\/\\\__\/\\\_\/\\\//\\\____/\\\//\\\__/\\\/////////\\\_____\/\\\_____\/\\\//\\\____/\\\//\\\__\//\\\______\///__
   _____\/\\\_____\/\\\\\\\\\\\\\\\_\/\\\//\\\_\/\\\_\/\\\\///\\\/\\\/_\/\\\_\/\\\_______\/\\\_____\/\\\_____\/\\\\///\\\/\\\/_\/\\\___\////\\\_________
    _____\/\\\_____\/\\\/////////\\\_\/\\\\//\\\\/\\\_\/\\\__\///\\\/___\/\\\_\/\\\\\\\\\\\\\\\_____\/\\\_____\/\\\__\///\\\/___\/\\\______\////\\\______
     _____\/\\\_____\/\\\_______\/\\\_\/\\\_\//\\\/\\\_\/\\\____\///_____\/\\\_\/\\\/////////\\\_____\/\\\_____\/\\\____\///_____\/\\\_________\////\\\___
      _____\/\\\_____\/\\\_______\/\\\_\/\\\__\//\\\\\\_\/\\\_____________\/\\\_\/\\\_______\/\\\_____\/\\\_____\/\\\_____________\/\\\__/\\\______\//\\\__
       __/\\\\\\\\\\\_\/\\\_______\/\\\_\/\\\___\//\\\\\_\/\\\_____________\/\\\_\/\\\_______\/\\\__/\\\\\\\\\\\_\/\\\_____________\/\\\_\///\\\\\\\\\\\/___
        _\///////////__\///________\///__\///_____\/////__\///______________\///__\///________\///__\///////////__\///______________\///____\///////////_____");
            Console.ResetColor();
            Console.WriteLine("Neural Network V.1.0");
            Console.WriteLine("(c) 2025 Genesis Symmetry");
            Console.WriteLine("Created by: Melvin \"Asymetr1c\" Abrahamsson (Owner of Genesis Symmetry)");

            ShowMenu();
        }

        private static void ShowMenu()
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\nMenu:");
                Console.WriteLine("1. Train network");
                Console.WriteLine(forceNew ? "2. Mode toggle: Force new" : "2. Mode toggle: Retrain");
                Console.WriteLine("3. Check existing network");
                Console.WriteLine("4. Set epochs (Current: " + epochs + ")");
                Console.WriteLine("5. Exit");
                Console.ResetColor();
                Console.Write("Select an option: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        TrainNetwork();
                        break;

                    case "2":
                        forceNew = !forceNew;
                        Console.WriteLine(forceNew ? "Force new training enabled." : "Force new training disabled.");
                        break;

                    case "3":
                        CheckExistingNetwork();
                        break;

                    case "4":
                        int.TryParse(Console.ReadLine(), out epochs);
                        break;

                    case "5":
                        Console.WriteLine("Exiting...");
                        return;

                    default:
                        Console.WriteLine("Unrecognized option. Please try again.");
                        break;
                }
            }
        }

        private static void TrainNetwork()
        {
            Console.WriteLine("Training new network...");

            string dataPath = "dataset";
            int imgWidth = 28, imgHeight = 28, numClasses = 10;

            var (inputsList, outputsList) = ImageLoader.LoadDataset(dataPath, imgWidth, imgHeight, numClasses);
            float[][] inputs = inputsList.ToArray();
            float[][] outputs = outputsList.ToArray();
            NeuralNetwork nn;
            if (!forceNew && File.Exists("trained_network.json"))
            {
                nn = NeuralNetwork.Load("trained_network.json");
            }
            else
            {
                int[] layers = { imgWidth * imgHeight, 128, 64, numClasses };
                ActivationType[] activations =
                {
                ActivationType.ReLU,
                ActivationType.ReLU,
                ActivationType.Softmax
            };
                nn = new NeuralNetwork(layers, activations);
            }

            float learningRate = 0.01f;
            int batchSize = 32;
            Random random = new Random();

            for (int epoch = 0; epoch < epochs; epoch++)
            {
                Console.ForegroundColor = ConsoleColor.Green;

                int sampleCount = inputs.Length;
                int[] indices = Enumerable.Range(0, sampleCount).ToArray();
                indices = indices.OrderBy(x => random.Next()).ToArray();

                float[][] shuffledInputs = new float[sampleCount][];
                float[][] shuffledOutputs = new float[sampleCount][];
                for (int i = 0; i < sampleCount; i++)
                {
                    shuffledInputs[i] = inputs[indices[i]];
                    shuffledOutputs[i] = outputs[indices[i]];
                }

                nn.Train(inputs, outputs, 1, learningRate, batchSize);
                Console.ResetColor();

                Console.Write("[");
                for (int i = 0; i < 50; i++)
                {
                    Console.Write(i < 50 ? "#" : " ");
                }
                Console.WriteLine($"] Epoch {epoch + 1} complete.");
            }

            string savePath = "trained_network.json";
            nn.Save(savePath);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\nNetwork trained and saved to {savePath}");
            Console.ResetColor();
        }

        private static void CheckExistingNetwork()
        {
            string loadPath = "trained_network.json";
            string dataSetPath = "dataset";

            if (!File.Exists(loadPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"No trained network found at {loadPath}. Please train a network first.");
                Console.ResetColor();
                return;
            }

            NeuralNetwork nn = NeuralNetwork.Load(loadPath);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Loaded trained network from {loadPath}");
            Console.ResetColor();

            Random rnd = new Random();

            string[] classFolders = Directory.GetDirectories(dataSetPath);
            if (classFolders.Length == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"No class directories found in dataset path: {dataSetPath}");
                Console.ResetColor();
                return;
            }

            string randomClassFolder = classFolders[rnd.Next(classFolders.Length)];
            string actualLabel = Path.GetFileName(randomClassFolder);

            string[] imageFiles = Directory.GetFiles(randomClassFolder, "*.png");
            if (imageFiles.Length == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"No PNG images found in class directory: {randomClassFolder}");
                Console.ResetColor();
                return;
            }

            string randomImageFile = imageFiles[rnd.Next(imageFiles.Length)];
            float[] input = ImageLoader.LoadImage(randomImageFile, 28, 28);
            var prediction = nn.FeedForward(input);
            int predictedLabel = Array.IndexOf(prediction, prediction.Max());

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\nImage: {randomImageFile}");
            Console.WriteLine($"Actual Label: {actualLabel}");
            Console.WriteLine($"Predicted Label: {predictedLabel}");
            Console.WriteLine($"Confidence: {prediction[predictedLabel]:0.0000}");
            Console.WriteLine("Prediction details:");
            for (int i = 0; i < prediction.Length; i++)
            {
                Console.WriteLine($"Class {i}: {prediction[i]:0.0000}");
            }
            Console.ResetColor();
        }
    }
}