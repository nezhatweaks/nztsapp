using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Management;

namespace NZTS_App
{
    public class CpuBenchmark
    {
        private BenchmarkMetrics _metrics;
        private CpuStatusWindow _cpuStatusWindow;

        public event Action<string> ProgressChanged;
        public event Action BenchmarkCompleted;

        public CpuBenchmark(BenchmarkMetrics metrics, CpuStatusWindow cpuStatusWindow)
        {
            _metrics = metrics;
            _cpuStatusWindow = cpuStatusWindow;

            // Initialize the events to avoid nullability warnings
            ProgressChanged = delegate { };
            BenchmarkCompleted = delegate { };
        }

        // Method to get core and thread description based on logical CPU id
        public string GetCoreAndThreadDescription(int cpuId)
        {
            int physicalCoreCount = GetPhysicalCoreCount();
            int logicalCoreCount = GetLogicalCoreCount();
            bool isHTEnabled = logicalCoreCount > physicalCoreCount;

            if (!isHTEnabled)
            {
                // HT is off, show only the core number
                return $"Core {cpuId + 1}";  // Just the core number
            }
            else
            {
                // HT is on, show both core and thread
                int coreNumber = (cpuId / 2) + 1;  // Divide logical CPUs into physical cores
                int threadNumber = (cpuId % 2) + 1; // 1 or 2 thread per core
                return $"Core {coreNumber}, Thread {threadNumber}";  // Show both core and thread
            }
        }



        // Execute WMIC command and return output as string
        private static string ExecuteWmicCommand(string command)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "wmic",
                Arguments = command,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using (var process = Process.Start(processStartInfo))
                {
                    if (process == null)
                        throw new InvalidOperationException("Failed to start WMIC process.");

                    using (var reader = process.StandardOutput)
                    {
                        // Ensure that StandardOutput is not null
                        if (reader == null)
                            throw new InvalidOperationException("StandardOutput is null.");

                        return reader.ReadToEnd().Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing WMIC command: {ex.Message}");
                return string.Empty;
            }
        }


        // Method to get the number of physical cores using WMIC
        public static int GetPhysicalCoreCount()
        {
            try
            {
                string output = ExecuteWmicCommand("cpu get NumberOfCores");

                string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                if (lines.Length > 1)
                {
                    return int.Parse(lines[1].Trim());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting physical core count: {ex.Message}");
            }

            return 0;
        }

        // Method to get the number of logical cores (including HT cores) using WMIC
        public static int GetLogicalCoreCount()
        {
            try
            {
                string output = ExecuteWmicCommand("cpu get NumberOfLogicalProcessors");

                string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                if (lines.Length > 1)
                {
                    return int.Parse(lines[1].Trim());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting logical core count: {ex.Message}");
            }

            return 0;
        }

        // Main method to run the benchmark asynchronously
        public async Task RunBenchmarkAsync()
        {
            ProgressChanged?.Invoke("Benchmarking started...");
            int totalCpus = GetLogicalCoreCount();  // Use logical core count for HT systems

            for (int cpuId = 0; cpuId < totalCpus; cpuId++)
            {
                int localCpuId = cpuId;  // Local copy to avoid closure issues
                await Task.Run(() => RunBenchmark(localCpuId));
            }

            ProgressChanged?.Invoke("Benchmarking completed.");
            BenchmarkCompleted?.Invoke();

            var resultsWindow = new BenchmarkResults(_metrics);
            resultsWindow.Show();

            SaveBenchmarkResults();
        }

        // Run the benchmark for a specific CPU core
        public void RunBenchmark(int cpuId)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // Set CPU affinity to ensure benchmarking is done on the correct CPU
            SetCpuAffinity(cpuId);

            ProgressChanged?.Invoke($"Benchmarking CPU {cpuId}...");

            int frameCount = 0;
            List<double> fpsValues = new List<double>();
            double maxFps = 0;
            double minFps = double.MaxValue;
            double totalFps = 0;

            // Benchmark for 60 seconds
            while (stopwatch.Elapsed.TotalSeconds < 60)
            {
                frameCount++;
                StressCpuWorkload(cpuId);  // Simulate CPU load

                double elapsedTime = stopwatch.Elapsed.TotalSeconds;
                double fps = frameCount / elapsedTime;  // Calculate FPS based on elapsed time
                fpsValues.Add(fps);

                maxFps = Math.Max(maxFps, fps);
                minFps = Math.Min(minFps, fps);
                totalFps += fps;

                // Update progress window every 5 seconds
                if (stopwatch.Elapsed.TotalSeconds % 5 < 1)
                {
                    _cpuStatusWindow.Dispatcher.Invoke(() =>
                    {
                        ProgressChanged?.Invoke($"CPU {cpuId} running... FPS: {fps:F2}");
                    });
                }
            }

            stopwatch.Stop();

            // Calculate statistics
            double avgFps = totalFps / frameCount;
            double standardDeviation = CalculateStandardDeviation(fpsValues, avgFps);
            double lowPercentile1 = CalculatePercentileLow(fpsValues, 1);
            double lowPercentile0_1 = CalculatePercentileLow(fpsValues, 0.1);

            // Create the benchmark result using the updated values
            BenchmarkResult result = new BenchmarkResult(
                cpuLabel: $"CPU {cpuId}",  // "CPU 0", "CPU 1", etc.
                coreName: GetCoreAndThreadDescription(cpuId),  // "Core 1, Thread 1", "Core 2, Thread 1", etc.
                fps: avgFps,  // Using avgFps for FPS value
                avgFps: avgFps,
                maxFps: maxFps,
                minFps: minFps,
                lows1Percent: lowPercentile1,
                lows0_1Percent: lowPercentile0_1,
                standardDeviation: standardDeviation
            );

            // Add result to the metrics
            _metrics.AddBenchmarkResult(cpuId, avgFps, maxFps, minFps, lowPercentile1, lowPercentile0_1, standardDeviation, fpsValues);

            // Update progress window after benchmarking is done
            _cpuStatusWindow.Dispatcher.Invoke(() =>
            {
                ProgressChanged?.Invoke($"CPU {cpuId} done! Avg FPS: {avgFps:F2} | Max FPS: {maxFps:F2} | Min FPS: {minFps:F2} | 1% Low: {lowPercentile1:F2} | 0.1% Low: {lowPercentile0_1:F2}");
            });
        }

        // Set thread affinity to a specific CPU core
        private void SetCpuAffinity(int cpuId)
        {
            // Setting affinity for logical cores. For example, core 0 will have affinity 0x1 (binary 0001)
            var mask = new IntPtr(1 << cpuId);
            IntPtr result = SetThreadAffinityMask(Thread.CurrentThread.ManagedThreadId, mask);

            if (result == IntPtr.Zero)
            {
                Console.WriteLine($"Error: Failed to set thread affinity for CPU {cpuId}");
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr SetThreadAffinityMask(int threadId, IntPtr dwThreadAffinityMask);

        // Simulate a CPU-intensive workload
        private void StressCpuWorkload(int cpuId)
        {
            long upperLimit = 100000;
            for (long i = 2; i < upperLimit; i++)
            {
                if (IsPrime(i)) { }
            }
        }

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

        private double CalculateStandardDeviation(List<double> frameTimes, double mean)
        {
            if (frameTimes.Count <= 1) return 0;

            double sumOfSquares = frameTimes.Sum(frameTime => Math.Pow(1000.0 / frameTime - mean, 2));
            double variance = sumOfSquares / frameTimes.Count;
            return Math.Sqrt(variance);
        }

        private double CalculatePercentileLow(List<double> fpsValues, double percentile)
        {
            var sortedFpsValues = fpsValues.OrderBy(f => f).ToList();
            int index = (int)(sortedFpsValues.Count * percentile / 100);
            index = Math.Max(index, 0);
            return sortedFpsValues[index];
        }

        // Save the benchmark results to a CSV file
        public void SaveBenchmarkResults()
        {
            try
            {
                var benchmarkResults = _metrics.BenchmarkResults;

                if (benchmarkResults == null || benchmarkResults.Count == 0)
                {
                    Console.WriteLine("No benchmark results to save.");
                    return;
                }

                var csvBuilder = new StringBuilder();
                csvBuilder.AppendLine("CpuLabel,CoreName,AvgFps,MaxFps,MinFps,1% Low,0.1% Low,StandardDeviation");

                var bestResult = benchmarkResults.OrderByDescending(r => r.AverageFps).First();
                var runnerUpResult = benchmarkResults.Where(r => r != bestResult).OrderByDescending(r => r.AverageFps).First();

                foreach (var result in benchmarkResults)
                {
                    string rowColor = result == bestResult ? "Green" : result == runnerUpResult ? "Yellow" : "None";
                    var row = $"{result.CpuLabel},{result.CoreName},{result.AverageFpsFormatted},{result.MaxFpsFormatted},{result.MinFpsFormatted},{result.Lows1PercentFormatted},{result.Lows0_1PercentFormatted},{result.StandardDeviationFormatted},{rowColor}";

                    csvBuilder.AppendLine(row);
                }

                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string benchmarksFolder = Path.Combine(baseDirectory, "Benchmarks");

                if (!Directory.Exists(benchmarksFolder))
                {
                    Directory.CreateDirectory(benchmarksFolder);
                }

                string fileName = $"Benchmark_{DateTime.Now:yyyy-MM-dd_HH-mm}.csv";
                string filePath = Path.Combine(benchmarksFolder, fileName);
                File.WriteAllText(filePath, csvBuilder.ToString());

                Console.WriteLine($"Benchmark results saved to: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving benchmark results: {ex.Message}");
            }
        }
    }
}
