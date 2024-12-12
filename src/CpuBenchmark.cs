using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Management;
using System.Threading;

namespace NZTS_App
{

    
    public class CpuBenchmark
    {
        private BenchmarkMetrics _metrics;
        private CpuStatusWindow _cpuStatusWindow;
        private CancellationTokenSource _cancellationTokenSource;

        public event Action<string> ProgressChanged;
        public event Action BenchmarkCompleted;
        private SemaphoreSlim _semaphore;  // Global semaphore to limit concurrency based on available CPUs


        private int _benchmarkDurationSeconds = 60;  // Default benchmark duration, can be adjusted

        public CpuBenchmark(BenchmarkMetrics metrics, CpuStatusWindow cpuStatusWindow)
        {
            _metrics = metrics;
            _cpuStatusWindow = cpuStatusWindow;
            ProgressChanged = delegate { };
            BenchmarkCompleted = delegate { };
            _cancellationTokenSource = new CancellationTokenSource();
            _semaphore = new SemaphoreSlim(Environment.ProcessorCount); // Limit to logical cores count
        }

        public string GetCoreAndThreadDescription(int cpuId)
        {
            int physicalCoreCount = GetPhysicalCoreCount();
            int logicalCoreCount = GetLogicalCoreCount();

            if (logicalCoreCount <= physicalCoreCount)
                return $"Core {cpuId + 1}";  // No hyper-threading, just core
            else
            {
                int coreNumber = (cpuId / 2) + 1;
                int threadNumber = (cpuId % 2) + 1;
                return $"Core {coreNumber}, Thread {threadNumber}";
            }
        }

        private static string ExecuteWmicCommand(string command)
        {
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "wmic",
                    Arguments = command,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(processStartInfo))
                {
                    if (process == null) throw new InvalidOperationException("Failed to start WMIC process.");

                    using (var reader = process.StandardOutput)
                    {
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

        public static int GetPhysicalCoreCount()
        {
            try
            {
                var query = new ObjectQuery("SELECT * FROM Win32_Processor");
                var searcher = new ManagementObjectSearcher(query);

                foreach (ManagementObject obj in searcher.Get())
                {
                    // Check if the "NumberOfCores" property exists and is not null
                    if (obj["NumberOfCores"] != null)
                    {
                        int coreCount;
                        // Try parsing the value to an integer
                        if (int.TryParse(obj["NumberOfCores"].ToString(), out coreCount))
                        {
                            return coreCount;
                        }
                        else
                        {
                            Console.WriteLine("Error: Failed to parse the NumberOfCores value.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error: NumberOfCores property is null.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting physical core count: {ex.Message}");
            }

            // Fallback to Environment.ProcessorCount if retrieval fails
            return Environment.ProcessorCount;
        }


        public static int GetLogicalCoreCount()
        {
            return Environment.ProcessorCount;  // Returns the number of logical cores directly
        }




        public async Task RunBenchmarkAsync(int durationSeconds = 60)
        {
            _benchmarkDurationSeconds = durationSeconds; // Allow custom benchmark duration
            ProgressChanged?.Invoke("Benchmarking started...");
            int totalCpus = GetLogicalCoreCount();  // Use logical cores for benchmarking
            var tasks = new List<Task>();

            // Semaphore for concurrency limit (using logical cores count)
            for (int cpuId = 0; cpuId < totalCpus; cpuId++)
            {
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                    break;

                int localCpuId = cpuId;
                await _semaphore.WaitAsync();  // Wait until a slot is available
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        RunBenchmark(localCpuId, _benchmarkDurationSeconds, _cancellationTokenSource.Token);
                    }
                    finally
                    {
                        _semaphore.Release();  // Release the slot after benchmarking
                    }
                }));
            }

            // Wait for all tasks to complete
            await Task.WhenAll(tasks);

            ProgressChanged?.Invoke("Benchmarking completed.");
            BenchmarkCompleted?.Invoke();  // This will trigger showing the results window and other actions.

            var resultsWindow = new BenchmarkResults(_metrics);
            resultsWindow.Show();  // Show results after all tasks are done

            SaveBenchmarkResults();  // Save the benchmark results after showing them
        }


        public void RunBenchmark(int cpuId, int durationSeconds, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            SetCpuAffinity(cpuId);

            ProgressChanged?.Invoke($"Benchmarking CPU {cpuId}...");

            int frameCount = 0;
            List<double> fpsValues = new List<double>();
            double maxFps = 0;
            double minFps = double.MaxValue;
            double totalFps = 0;

            while (stopwatch.Elapsed.TotalSeconds < durationSeconds && !cancellationToken.IsCancellationRequested)
            {
                frameCount++;
                StressCpuWorkload(cpuId);

                double elapsedTime = stopwatch.Elapsed.TotalSeconds;
                double fps = frameCount / elapsedTime;
                fpsValues.Add(fps);

                maxFps = Math.Max(maxFps, fps);
                minFps = Math.Min(minFps, fps);
                totalFps += fps;

                if (stopwatch.Elapsed.TotalSeconds % 5 < 1)
                {
                    _cpuStatusWindow.Dispatcher.Invoke(() =>
                    {
                        ProgressChanged?.Invoke($"CPU {cpuId} running... FPS: {fps:F2}");
                    });
                }
            }

            stopwatch.Stop();

            double avgFps = totalFps / frameCount;
            double standardDeviation = CalculateStandardDeviation(fpsValues, avgFps);
            double lows1Percent = CalculatePercentileLow(fpsValues, 1);
            double lows0_1Percent = CalculatePercentileLow(fpsValues, 0.1);

            BenchmarkResult result = new BenchmarkResult(
                cpuLabel: $"CPU {cpuId}",
                coreName: GetCoreAndThreadDescription(cpuId),
                fps: avgFps,
                avgFps: avgFps,
                maxFps: maxFps,
                minFps: minFps,
                lows1Percent: lows1Percent,
                lows0_1Percent: lows0_1Percent,
                standardDeviation: standardDeviation
            );

            _metrics.AddBenchmarkResult(cpuId, avgFps, maxFps, minFps, lows1Percent, lows0_1Percent, standardDeviation, fpsValues);

            _cpuStatusWindow.Dispatcher.Invoke(() =>
            {
                ProgressChanged?.Invoke($"CPU {cpuId} done! Avg FPS: {avgFps:F2} | Max FPS: {maxFps:F2} | Min FPS: {minFps:F2} | 1% Low: {lows1Percent:F2} | 0.1% Low: {lows0_1Percent:F2}");
            });
        }

        private void SetCpuAffinity(int cpuId)
        {
            try
            {
                var mask = new IntPtr(1 << cpuId);
                IntPtr result = SetThreadAffinityMask(Thread.CurrentThread.ManagedThreadId, mask);
                if (result == IntPtr.Zero)
                {
                    Console.WriteLine($"Warning: Failed to set thread affinity for CPU {cpuId}.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting thread affinity for CPU {cpuId}: {ex.Message}");
            }
        }


        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr SetThreadAffinityMask(int threadId, IntPtr dwThreadAffinityMask);

        private void StressCpuWorkload(int cpuId)
        {
            // Simulate a more varied workload, like busy looping and memory access
            long upperLimit = 60000;
            double result = 0;

            for (long i = 2; i < upperLimit; i++)
            {
                if (IsPrime(i))
                {
                    result += Math.Sqrt(i);  // Add a computational load
                }
            }
        }

        private bool IsPrime(long number)
        {
            if (number < 2) return false;
            for (long i = 2; i <= Math.Sqrt(number); i++)
            {
                if (number % i == 0) return false;
            }
            return true;
        }

        private double CalculateStandardDeviation(List<double> frameTimes, double mean)
        {
            double sumOfSquares = frameTimes.Sum(f => Math.Pow(f - mean, 2));
            return Math.Sqrt(sumOfSquares / frameTimes.Count);
        }

        private double CalculatePercentileLow(List<double> fpsValues, double percentile)
        {
            var sortedFpsValues = fpsValues.OrderBy(f => f).ToList();
            int index = (int)(sortedFpsValues.Count * percentile / 100);
            return sortedFpsValues[Math.Max(index, 0)];
        }

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

                foreach (var result in benchmarkResults)
                {
                    var row = $"{result.CpuLabel},{result.CoreName},{result.AverageFps},{result.MaxFps},{result.MinFps},{result.Lows1Percent},{result.Lows0_1Percent},{result.StandardDeviation}";
                    csvBuilder.AppendLine(row);
                }

                string fileName = $"Benchmark_{DateTime.Now:yyyy-MM-dd_HH-mm}.csv";
                string directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Benchmarks");
                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                string filePath = Path.Combine(directoryPath, fileName);
                File.WriteAllText(filePath, csvBuilder.ToString());

                Console.WriteLine($"Benchmark results saved to {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving benchmark results: {ex.Message}");
            }
        }
    }
}
