namespace NZTS_App
{
    public class BenchmarkResult
    {
        public string CoreName { get; set; } = "Unknown";  // Default value for CoreName
        public string CpuLabel { get; set; } = "Unknown";  // Default value for CpuLabel

        public double Fps { get; set; }
        public double AverageFps { get; set; }
        public double MaxFps { get; set; }
        public double MinFps { get; set; }
        public double Lows1Percent { get; set; }
        public double Lows0_1Percent { get; set; }
        public double StandardDeviation { get; set; }

        // Properties that return the formatted value
        public string FpsFormatted => Fps.ToString("F2");
        public string AverageFpsFormatted => AverageFps.ToString("F2");
        public string MaxFpsFormatted => MaxFps.ToString("F2");
        public string MinFpsFormatted => MinFps.ToString("F2");
        public string Lows1PercentFormatted => Lows1Percent.ToString("F2");
        public string Lows0_1PercentFormatted => Lows0_1Percent.ToString("F2");
        public string StandardDeviationFormatted => StandardDeviation.ToString("F2");
    }
}
