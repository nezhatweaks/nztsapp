public class BenchmarkResult
{
    public string? CoreName { get; set; }  // Make the property nullable

    public string? CpuLabel { get; set; }  // Make the property nullable
    public double Fps { get; set; } // FPS for this core/thread
    public double AverageFps { get; set; }
    public double MaxFps { get; set; }
    public double MinFps { get; set; }
    public double Lows1Percent { get; set; }
    public double Lows0_1Percent { get; set; }
    public double StandardDeviation { get; set; }
}

public class BenchmarkViewModel
{
    public List<BenchmarkResult> BenchmarkResults { get; set; }

    public BenchmarkViewModel(List<BenchmarkResult> benchmarkResults)
    {
        BenchmarkResults = benchmarkResults;
    }

    // You can also have methods here to calculate metrics like average, standard deviation, etc.
}
