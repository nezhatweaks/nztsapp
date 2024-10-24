using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Threading;
using System.Windows;

namespace NZTS_App
{
    

    public class SmartTrimManager
    {
        public bool IsRunning => smartTrimTimer.IsEnabled;

        private static SmartTrimManager? instance;
        private DispatcherTimer smartTrimTimer;
        private const int TrimIntervalSeconds = 10;
        private float cpuUsageThreshold = 50; // Default threshold

        private SmartTrimManager()
        {
            smartTrimTimer = new DispatcherTimer();
            smartTrimTimer.Interval = TimeSpan.FromSeconds(TrimIntervalSeconds);
            smartTrimTimer.Tick += SmartTrimTimer_Tick;
        }

        public static SmartTrimManager Instance => instance ??= new SmartTrimManager();

        public void Start()
        {
            smartTrimTimer.Start();
            
        }

        public void Stop()
        {
            smartTrimTimer.Stop();
            
        }


        public void SetCpuUsageThreshold(float threshold)
        {
            cpuUsageThreshold = threshold;
        }

        private void SmartTrimTimer_Tick(object? sender, EventArgs e)
        {
            var processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                try
                {
                    float cpuUsage = GetCpuUsage(process);
                    if (cpuUsage > cpuUsageThreshold)
                    {
                        if (process.PriorityClass != ProcessPriorityClass.High)
                        {
                            process.PriorityClass = ProcessPriorityClass.High;
                        }
                    }
                    else
                    {
                        if (process.PriorityClass != ProcessPriorityClass.Normal)
                        {
                            process.PriorityClass = ProcessPriorityClass.Normal;
                        }
                    }
                }
                catch (Exception)
                {
                    // Handle process errors here if needed
                }
            }
        }

        private float GetCpuUsage(Process process)
        {
            if (process == null || process.HasExited)
                return 0;

            using (var cpuCounter = new PerformanceCounter("Process", "% Processor Time", process.ProcessName))
            {
                cpuCounter.NextValue();
                System.Threading.Thread.Sleep(100);
                return cpuCounter.NextValue();
            }
        }
    }

}
