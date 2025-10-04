using System;
using System.IO;
using System.Linq;

namespace NeuralNetwork_IHNMAIMS
{
    public class NetworkProcessor
    {
        private readonly NeuralNetwork _network;

        public NetworkProcessor(string networkFilePath)
        {
            if (!File.Exists(networkFilePath))
            {
                throw new FileNotFoundException("The neural network model file was not found.", networkFilePath);
            }
            _network = NeuralNetwork.Load(networkFilePath);
        }

        public int RecognizeDigit(float[] imagePixels)
        {
            if (imagePixels.Length != 28 * 28)
            {
                throw new ArgumentException("Input image must be a 28x28 (784 pixels) float array.", nameof(imagePixels));
            }

            float[] prediction = _network.FeedForward(imagePixels);

            int predictedDigit = Array.IndexOf(prediction, prediction.Max());

            return predictedDigit;
        }
    }
}
