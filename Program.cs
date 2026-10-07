using System;
using System.IO;
using System.Linq;

namespace CoinDenomImageProcessing
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("==========================================================");
            Console.WriteLine("    PHILIPPINE COIN RECOGNITION & COUNTING SYSTEM         ");
            Console.WriteLine("    Pure C# Image Processing (Zero External Libraries)     ");
            Console.WriteLine("==========================================================");
            Console.WriteLine();

            // Locate image
            string imagePath = "GetImage.jpeg";
            if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
            {
                imagePath = args[0].Trim('"', '\'');
            }

            if (!File.Exists(imagePath))
            {
                // Check in application directory
                string altPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GetImage.jpeg");
                if (File.Exists(altPath))
                {
                    imagePath = altPath;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("[Notice] 'GetImage.jpeg' was not found in the current directory.");
                    Console.ResetColor();
                    Console.Write("Please enter the path to the coin image (or drag & drop here): ");
                    string? input = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(input))
                    {
                        imagePath = input.Trim('"', '\'', ' ');
                    }
                }
            }

            if (!File.Exists(imagePath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[Error] Unable to locate image: {imagePath}");
                Console.ResetColor();
                WaitForExit();
                return;
            }

            Console.WriteLine($"[1/5] Loading image: {Path.GetFileName(imagePath)}...");
            var detector = new CoinDetector();

            Console.WriteLine("[2/5] Performing grayscale conversion and Otsu thresholding...");
            var coins = detector.ProcessImage(imagePath);
            Console.WriteLine($"      Computed Otsu Threshold: {detector.ThresholdUsed}");

            Console.WriteLine("[3/5] Performing connected component & internal hole analysis...");
            Console.WriteLine($"      Detected total valid coin candidates: {coins.Count}");

            Console.WriteLine("[4/5] Classifying coins by diameter & morphology...");

            int count5Cent = coins.Count(c => c.Type == CoinType.FiveCent);
            int count10Cent = coins.Count(c => c.Type == CoinType.TenCent);
            int count25Cent = coins.Count(c => c.Type == CoinType.TwentyFiveCent);
            int count1Peso = coins.Count(c => c.Type == CoinType.OnePeso);
            int count5Peso = coins.Count(c => c.Type == CoinType.FivePeso);
            int totalCoins = coins.Count;

            decimal val5Cent = count5Cent * 0.05m;
            decimal val10Cent = count10Cent * 0.10m;
            decimal val25Cent = count25Cent * 0.25m;
            decimal val1Peso = count1Peso * 1.00m;
            decimal val5Peso = count5Peso * 5.00m;
            decimal grandTotal = val5Cent + val10Cent + val25Cent + val1Peso + val5Peso;

            Console.WriteLine();
            Console.WriteLine("----------------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("                COIN COUNTING RESULTS                     ");
            Console.ResetColor();
            Console.WriteLine("----------------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Total Coins: {totalCoins}");
            Console.WriteLine($"5 Cent:  {count5Cent,2} coins  (Value: ₱{val5Cent,5:F2})");
            Console.WriteLine($"10 Cent: {count10Cent,2} coins  (Value: ₱{val10Cent,5:F2})");
            Console.WriteLine($"25 Cent: {count25Cent,2} coins  (Value: ₱{val25Cent,5:F2})");
            Console.WriteLine($"1 Peso:  {count1Peso,2} coins  (Value: ₱{val1Peso,5:F2})");
            Console.WriteLine($"5 Peso:  {count5Peso,2} coins  (Value: ₱{val5Peso,5:F2})");
            Console.ResetColor();
            Console.WriteLine("----------------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Total Monetary Value: ₱{grandTotal:F2}");
            Console.ResetColor();
            Console.WriteLine("----------------------------------------------------------");
            Console.WriteLine();

            Console.WriteLine("[5/5] Generating annotated visualization image...");
            string outputImagePath = Path.Combine(
                Path.GetDirectoryName(imagePath) ?? "",
                "output_annotated.jpg");
            if (string.IsNullOrEmpty(Path.GetDirectoryName(imagePath)))
            {
                outputImagePath = "output_annotated.jpg";
            }

            try
            {
                detector.SaveAnnotatedImage(imagePath, outputImagePath);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"      Annotated image successfully saved to: {outputImagePath}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"      Could not save annotated image: {ex.Message}");
            }

            Console.WriteLine();
            Console.WriteLine("Processing finished successfully.");
            WaitForExit();
        }

        private static void WaitForExit()
        {
            // If running interactively, wait before closing window
            if (!Console.IsInputRedirected)
            {
                Console.WriteLine();
                Console.Write("Press Enter to exit...");
                Console.ReadLine();
            }
        }
    }
}
