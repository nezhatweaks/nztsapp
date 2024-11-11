using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace NZTS_App
{
    public class BenchmarkMetrics : INotifyPropertyChanged
    {
        private List<BenchmarkResult> _benchmarkResults;

        // Property to hold the list of benchmark results
        public List<BenchmarkResult> BenchmarkResults
        {
            get { return _benchmarkResults; }
            set
            {
                if (_benchmarkResults != value)
                {
                    _benchmarkResults = value;
                    OnPropertyChanged(nameof(BenchmarkResults));  // Notify UI when the BenchmarkResults list changes
                }
            }
        }

        // Initialize PropertyChanged event to prevent nullability warning
        public event PropertyChangedEventHandler? PropertyChanged;  // Make it nullable to match INotifyPropertyChanged

        public BenchmarkMetrics()
        {
            _benchmarkResults = new List<BenchmarkResult>(); // Initialize the list
            PropertyChanged = null; // Initialize PropertyChanged to null
        }

        // Add a new benchmark result to the list
        public void AddBenchmarkResult(int cpuId, double avgFps, double maxFps, double minFps,
                                        double lowPercentile1, double lowPercentile0_1,
                                        double standardDeviation, List<double> fpsValues)
        {
            if (fpsValues.Count == 0)
            {
                fpsValues.Add(0);  // Add a dummy value to avoid errors
            }

            // Convert cpuId into both user-friendly and CPU{cpuId} labels
            var coreAndThread = GetCoreAndThreadDescription(cpuId);
            var cpuLabel = $"CPU{cpuId}"; // For example: "CPU0", "CPU1", etc.

            // Create the benchmark result
            var result = new BenchmarkResult
            {
                CpuLabel = cpuLabel,       // Original "CPU0", "CPU1", etc.
                CoreName = coreAndThread,  // User-friendly "Core X, Thread Y"
                Fps = avgFps,              // Assuming avgFps is the main FPS value
                AverageFps = avgFps,
                MaxFps = maxFps,
                MinFps = minFps,
                Lows1Percent = lowPercentile1,
                Lows0_1Percent = lowPercentile0_1,
                StandardDeviation = standardDeviation
            };

            // Add the result to the BenchmarkResults list
            BenchmarkResults.Add(result);
        }

        // Helper method to get the core and thread description from the cpuId
        private string GetCoreAndThreadDescription(int cpuId)
        {
            // Assuming no hyper-threading, just sequential mapping
            int coreNumber = cpuId / 2 + 1;  // CPU 0, 1 -> Core 1, CPU 2, 3 -> Core 2, etc.
            int threadNumber = (cpuId % 2 == 0) ? 1 : 2;  // Even cpuId -> Thread 1, Odd cpuId -> Thread 2

            return $"Core {coreNumber}, Thread {threadNumber}";
        }

        // Implement INotifyPropertyChanged
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Method to save benchmark results to a CSV file
        public void SaveBenchmarkResultsToCsv()
        {
            // Ensure the "Benchmarks" folder exists
            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Benchmarks");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Define the file name with timestamp
            string fileName = $"BenchmarkResults_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            string filePath = Path.Combine(directoryPath, fileName);

            // Create or overwrite the CSV file
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // Write the header row
                writer.WriteLine("CPU Label, Core and Thread, Avg FPS, Max FPS, Min FPS, 1% Low, 0.1% Low, Best FPS, Runner-up FPS");

                // Get the benchmark results
                var benchmarkResults = BenchmarkResults;

                // Find the best and runner-up results based on max FPS
                var bestResult = benchmarkResults.OrderByDescending(r => r.MaxFps).First();
                var runnerUpResult = benchmarkResults.OrderByDescending(r => r.MaxFps)
                                                      .Where(r => r != bestResult)
                                                      .FirstOrDefault();

                // Iterate through all benchmark results
                foreach (var result in benchmarkResults)
                {
                    // Prepare the row data
                    string row = $"{result.CpuLabel}, {result.CoreName}, {result.AverageFps:F2}, {result.MaxFps:F2}, {result.MinFps:F2}, {result.Lows1Percent:F2}, {result.Lows0_1Percent:F2}, ";

                    // Mark the best and runner-up rows with appropriate labels
                    if (result == bestResult)
                    {
                        row += "Best";
                    }
                    else if (result == runnerUpResult)
                    {
                        row += "Runner-up";
                    }
                    else
                    {
                        row += "";
                    }

                    // Write the row to the CSV file
                    writer.WriteLine(row);
                }
            }

            // Notify that the results were saved
            Console.WriteLine($"Benchmark results saved to: {filePath}");
        }
    }
}