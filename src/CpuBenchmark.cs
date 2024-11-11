using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NZTS_App
{
    public class CpuBenchmark
    {
        private BenchmarkMetrics _metrics;
        private CpuStatusWindow _cpuStatusWindow;

        public event Action<string> ProgressChanged;  // Event to report progress
        public event Action BenchmarkCompleted;  // Event when benchmark finishes

        public CpuBenchmark(BenchmarkMetrics metrics, CpuStatusWindow cpuStatusWindow)
        {
            _metrics = metrics;
            _cpuStatusWindow = cpuStatusWindow;

            // Initialize the events to avoid nullability warnings
            ProgressChanged = delegate { };
            BenchmarkCompleted = delegate { };
        }

        // Main method to run the benchmark asynchronously
        public async Task RunBenchmarkAsync()
        {
            // Notify that benchmarking has started
            ProgressChanged?.Invoke("Benchmarking started...");

            // Get the number of processors/cores available on the system
            int totalCpus = Environment.ProcessorCount;

            // Sequentially benchmark each core
            for (int cpuId = 0; cpuId < totalCpus; cpuId++)
            {
                int localCpuId = cpuId;  // Local copy of the cpuId to avoid closure issue in the task

                // Run the benchmark for this CPU core and await its completion
                await Task.Run(() => RunBenchmark(localCpuId));
            }

            // Notify that benchmarking is complete
            ProgressChanged?.Invoke("Benchmarking completed.");
            BenchmarkCompleted?.Invoke();

            // Once the benchmark is completed, open the results window
            var resultsWindow = new BenchmarkResults(_metrics);  // Assuming _metrics has the benchmark results
            resultsWindow.Show();

            // Save benchmark results to a CSV file
            SaveBenchmarkResults();
        }

        // Method that runs the benchmark for a specific CPU
        public void RunBenchmark(int cpuId)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // Set thread affinity for the benchmark to ensure it runs on the specified CPU core
            SetCpuAffinity(cpuId); // Set CPU affinity before starting the benchmark

            // Notify about the progress of the benchmarking
            ProgressChanged?.Invoke($"Benchmarking CPU {cpuId}...");

            int frameCount = 0;
            List<double> fpsValues = new List<double>(); // To hold the FPS for each frame
            double maxFps = 0;
            double minFps = double.MaxValue;
            double totalFps = 0;

            // Simulating benchmarking for 60 seconds with a CPU-intensive workload
            while (stopwatch.Elapsed.TotalSeconds < 60)
            {
                frameCount++;

                // Perform a CPU-intensive task here to simulate real-world workload
                StressCpuWorkload(cpuId);

                // Calculate FPS as frames per second over the elapsed time
                double elapsedTime = stopwatch.Elapsed.TotalSeconds;  // Total time elapsed in seconds
                double fps = frameCount / elapsedTime; // Calculate FPS as frames per second
                fpsValues.Add(fps);

                // Track max, min, and total FPS
                maxFps = Math.Max(maxFps, fps);
                minFps = Math.Min(minFps, fps);
                totalFps += fps;

                // Update the UI safely on the UI thread using Dispatcher
                if (stopwatch.Elapsed.TotalSeconds % 5 < 1)  // every 5 seconds
                {
                    _cpuStatusWindow.Dispatcher.Invoke(() =>
                    {
                        ProgressChanged?.Invoke($"CPU {cpuId} running... FPS: {fps:F2}");
                    });
                }
            }

            stopwatch.Stop();

            // Calculate the final FPS metrics after benchmarking ends
            double avgFps = totalFps / frameCount;
            double standardDeviation = CalculateStandardDeviation(fpsValues, avgFps);

            // Calculate 1% and 0.1% lows in terms of FPS
            double lowPercentile1 = CalculatePercentileLow(fpsValues, 1);  // 1% Low
            double lowPercentile0_1 = CalculatePercentileLow(fpsValues, 0.1);  // 0.1% Low

            // Add benchmark result to metrics
            _metrics.AddBenchmarkResult(cpuId, avgFps, maxFps, minFps, lowPercentile1, lowPercentile0_1, standardDeviation, fpsValues);

            // Final progress update after completion
            _cpuStatusWindow.Dispatcher.Invoke(() =>
            {
                ProgressChanged?.Invoke($"CPU {cpuId} done! Avg FPS: {avgFps:F2} | Max FPS: {maxFps:F2} | Min FPS: {minFps:F2} | 1% Low: {lowPercentile1:F2} | 0.1% Low: {lowPercentile0_1:F2}");
            });
        }

        // Set thread affinity to ensure it runs on a specific CPU core
        private void SetCpuAffinity(int cpuId)
        {
            // Use the current thread's threadId to set its affinity
            int threadId = (int)Thread.CurrentThread.ManagedThreadId;

            var mask = new IntPtr(1 << cpuId); // Set the CPU affinity mask
            SetThreadAffinityMask(threadId, mask);
        }

        // P/Invoke to set thread affinity
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr SetThreadAffinityMask(int threadId, IntPtr dwThreadAffinityMask);

        // Simulate a CPU-intensive workload (e.g., matrix multiplication, etc.)
        private void StressCpuWorkload(int cpuId)
        {
            // Use an algorithm that stresses the CPU (e.g., finding prime numbers)
            long upperLimit = 100000;  // Try to find primes up to this number
            long primesFound = 0;
            for (long i = 2; i < upperLimit; i++)
            {
                if (IsPrime(i))
                {
                    primesFound++;
                }
            }
        }

        // Simple prime number checking algorithm (CPU intensive)
        private bool IsPrime(long number)
        {
            if (number < 2) return false;
            for (long i = 2; i <= Math.Sqrt(number); i++)
            {
                if (number % i == 0)
                {
                    return false;
                }
            }
            return true;
        }

        // Method to calculate standard deviation
        private double CalculateStandardDeviation(List<double> frameTimes, double mean)
        {
            double sumOfSquares = frameTimes.Sum(frameTime => Math.Pow(1000.0 / frameTime - mean, 2));
            double variance = sumOfSquares / frameTimes.Count;
            return Math.Sqrt(variance);
        }

        // Method to calculate percentile low (1% or 0.1% lows) correctly
        private double CalculatePercentileLow(List<double> fpsValues, double percentile)
        {
            // Sort the FPS values in ascending order (lower FPS values are worse)
            var sortedFpsValues = fpsValues.OrderBy(f => f).ToList();

            // Find the index corresponding to the requested percentile
            int index = (int)(sortedFpsValues.Count * percentile / 100);  // This gives us the worst frames for 1% or 0.1% low

            // Ensure the index is within bounds
            index = Math.Max(index, 0);  // Make sure we do not access out-of-range indexes

            // Return the FPS value at the calculated index
            return sortedFpsValues[index];  // This is the correct percentile low FPS value
        }

        // Save benchmark results to a CSV file
        public void SaveBenchmarkResults()
        {
            try
            {
                // Access the benchmark results from _metrics
                var benchmarkResults = _metrics.BenchmarkResults;

                // Check if benchmark results are present
                if (benchmarkResults == null || benchmarkResults.Count == 0)
                {
                    Console.WriteLine("No benchmark results to save.");
                    return; // Early exit if no results are available
                }

                // Create a StringBuilder to store CSV data
                var csvBuilder = new StringBuilder();

                // Add the header to the CSV
                csvBuilder.AppendLine("CpuLabel and CoreName,AvgFps,MaxFps,MinFps,1% Low,0.1% Low,StandardDeviation");

                // Find the best and runner-up results based on AverageFps (or any other metric)
                var bestResult = benchmarkResults.OrderByDescending(r => r.AverageFps).First();
                var runnerUpResult = benchmarkResults.Where(r => r != bestResult).OrderByDescending(r => r.AverageFps).First();

                // Loop through each benchmark result and append to the CSV
                foreach (var result in benchmarkResults)
                {
                    // Construct the thread label based on CpuLabel and CoreName
                    // You can customize this based on your own naming convention.
                    string threadName = $"{result.CpuLabel} {result.CoreName}";

                    // Determine the row color (green for best, yellow for runner-up)
                    string rowColor = result == bestResult ? "Green" : result == runnerUpResult ? "Yellow" : "None";

                    // Create the row for this benchmark result
                    var row = $"{threadName},{result.AverageFpsFormatted},{result.MaxFpsFormatted},{result.MinFpsFormatted},{result.Lows1PercentFormatted},{result.Lows0_1PercentFormatted},{result.StandardDeviationFormatted},{rowColor}";

                    // Append the row to the CSV string
                    csvBuilder.AppendLine(row);
                }

                // Create the "Benchmarks" folder if it doesn't exist
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;  // Get the base directory of the application
                string benchmarksFolder = Path.Combine(baseDirectory, "Benchmarks");

                // Ensure the directory exists
                if (!Directory.Exists(benchmarksFolder))
                {
                    Directory.CreateDirectory(benchmarksFolder); // Create the folder if it doesn't exist
                    Console.WriteLine($"Created directory: {benchmarksFolder}");
                }

                // Generate a filename based on the current date and time
                string fileName = $"Benchmark_{DateTime.Now:yyyy-MM-dd_HH-mm}.csv";  // Format the date and time for the filename

                // Define the full file path
                string filePath = Path.Combine(benchmarksFolder, fileName);

                // Write the CSV data to the file
                File.WriteAllText(filePath, csvBuilder.ToString());

                Console.WriteLine($"Benchmark results saved to: {filePath}");
            }
            catch (Exception ex)
            {
                // Log any exceptions
                Console.WriteLine($"Error saving benchmark results: {ex.Message}");
            }
        }



    }
}