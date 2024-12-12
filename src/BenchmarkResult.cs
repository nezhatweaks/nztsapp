namespace NZTS_App
{
    public class BenchmarkResult
    {
        // Properties that describe the benchmark results for each CPU core
        public string CoreName { get; set; } = "Unknown";  // Default value for CoreName
        public string CpuLabel { get; set; } = "Unknown";  // Default value for CpuLabel

        public double Fps { get; set; }
        public double AverageFps { get; set; }
        public double MaxFps { get; set; }
        public double MinFps { get; set; }
        public double Lows1Percent { get; set; }  // 1% low FPS
        public double Lows0_1Percent { get; set; }  // 0.1% low FPS
        public double StandardDeviation { get; set; }

        // Formatted properties for CSV or display purposes
        public string FpsFormatted => Fps.ToString("F2");
        public string AverageFpsFormatted => AverageFps.ToString("F2");
        public string MaxFpsFormatted => MaxFps.ToString("F2");
        public string MinFpsFormatted => MinFps.ToString("F2");
        public string Lows1PercentFormatted => Lows1Percent.ToString("F2");
        public string Lows0_1PercentFormatted => Lows0_1Percent.ToString("F2");
        public string StandardDeviationFormatted => StandardDeviation.ToString("F2");

        // Optional constructor to initialize more specifically if needed
        public BenchmarkResult(string cpuLabel, string coreName, double fps, double avgFps, double maxFps, double minFps, double lows1Percent, double lows0_1Percent, double standardDeviation)
        {
            CpuLabel = cpuLabel;
            CoreName = coreName;
            Fps = fps;
            AverageFps = avgFps;
            MaxFps = maxFps;
            MinFps = minFps;
            Lows1Percent = lows1Percent;
            Lows0_1Percent = lows0_1Percent;
            StandardDeviation = standardDeviation;
        }
    }
}
