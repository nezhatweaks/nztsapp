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
        }

        // Add a new benchmark result to the list
        public void AddBenchmarkResult(int cpuId, double avgFps, double maxFps, double minFps,
                                        double lowPercentile1, double lowPercentile0_1,
                                        double standardDeviation, List<double> fpsValues)
        {
            if (fpsValues.Count == 0)
            {
                fpsValues.Add(0);  // Add a dummy value to avoid errors
                Console.WriteLine($"Warning: No FPS values provided for CPU {cpuId}. Defaulting to 0.");
            }

            // Convert cpuId into both user-friendly and CPU{cpuId} labels
            var coreAndThread = GetCoreAndThreadDescription(cpuId);
            var cpuLabel = $"CPU{cpuId}"; // For example: "CPU0", "CPU1", etc.

            // Create the benchmark result
            var result = new BenchmarkResult(
    cpuLabel,        // Original "CPU0", "CPU1", etc.
    coreAndThread,   // User-friendly "Core X, Thread Y"
    avgFps,          // Assuming avgFps is the main FPS value
    avgFps,          // Average FPS
    maxFps,          // Max FPS
    minFps,          // Min FPS
    lowPercentile1,  // 1% Low FPS
    lowPercentile0_1, // 0.1% Low FPS
    standardDeviation // Standard deviation
);

            // Add the result to the BenchmarkResults list
            BenchmarkResults.Add(result);


        }

        // Helper method to get the core and thread description from the cpuId
        private string GetCoreAndThreadDescription(int cpuId)
        {
            // Get the number of physical cores and logical processors (including HT)
            int physicalCoreCount = GetPhysicalCoreCount();
            int logicalProcessorCount = Environment.ProcessorCount;

            // Check if Hyper-Threading (HT) is enabled
            bool isHTEnabled = logicalProcessorCount > physicalCoreCount;

            // Core number is based on cpuId divided by logical processors per physical core
            int coreNumber = cpuId / (logicalProcessorCount / physicalCoreCount) + 1;

            if (isHTEnabled)
            {
                // If HT is enabled, show both core and thread
                int threadNumber = (cpuId % 2 == 0) ? 1 : 2;  // Example: CPU 0 could be Thread 1, CPU 1 could be Thread 2
                return $"Core {coreNumber}, Thread {threadNumber}";
            }
            else
            {
                // If HT is disabled, only show core number (no thread info)
                return $"Core {coreNumber}";
            }
        }


        // Helper method to get the number of physical cores
        private int GetPhysicalCoreCount()
        {
            int coreCount = 0;
            using (var searcher = new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
            {
                foreach (var queryObj in searcher.Get())
                {
                    coreCount = Convert.ToInt32(queryObj["NumberOfCores"]);
                }
            }
            return coreCount;
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
                writer.WriteLine("CPU Label, Core/Thread, Avg FPS, Max FPS, Min FPS, 1% Low, 0.1% Low, Best FPS, Runner-up FPS");

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

                    // Add Best and Runner-up FPS to the row
                    if (result == bestResult)
                    {
                        row += $"{bestResult.MaxFps:F2}";
                    }
                    else if (result == runnerUpResult)
                    {
                        row += $"{runnerUpResult.MaxFps:F2}";
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
