using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork_IHNMAIMS
{
    internal class ImageLoader
    {
        public static float[] LoadImage(string path, int width, int height)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Image file not found: {path}");
            }

            using (var bitmap = new System.Drawing.Bitmap(path))
            {
                Bitmap resized = new Bitmap(bitmap, new Size(width, height));
                float[] pixels = new float[width * height];

                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        Color pixel = resized.GetPixel(i, j);
                        pixels[j * width + i] = (pixel.R + pixel.G + pixel.B) / 3f / 255f;
                    }
                }
                return pixels;
            }
        }

        public static (List<float[]>, List<float[]>) LoadDataset(string directory, int width, int height, int numClasses)
        {
            var inputs = new List<float[]>();
            var outputs = new List<float[]>();

            foreach (var dir in Directory.GetDirectories(directory))
            {
                string labelName = Path.GetFileName(dir);
                if (!int.TryParse(labelName, out int label))
                {
                    Console.WriteLine($"Skipping invalid label directory: {dir}");
                    continue;
                }

                foreach (var file in Directory.GetFiles(dir, "*.png"))
                {
                    try
                    {
                        float[] pixels = LoadImage(file, width, height);
                        inputs.Add(pixels);

                        float[] expected = new float[numClasses];
                        expected[label] = 1f;
                        outputs.Add(expected);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading image {file}: {ex.Message}");
                    }
                }
            }

            return (inputs, outputs);
        }
    }
}