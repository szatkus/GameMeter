using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Timers;
using System.Collections.Generic;
using System.Windows.Media;

namespace GameMeter
{

    public class MeterEventArgs : EventArgs
    {
        public Dictionary<int, Plot> Plots;
        public bool Finish;
    }

    public class Plot
    {
        public Queue<int> Values = new Queue<int>();
        public Color color;
        public int Best = -1;
        static Random random = new Random();
        public Plot()
        {
            color = Color.FromRgb((byte)random.Next(0, 255), (byte)random.Next(0, 255), (byte)random.Next(0, 255));
            for (int i = 0; i < 100; i++)
            {
                Values.Enqueue(0);
            }
        }
    }

    public delegate void ChangedEventHandler(object sender, MeterEventArgs e);

    class ProcessMeter
    {
        public String FileName;
        public event ChangedEventHandler Changed;
        private Dictionary<int, Plot> Plots;
        private Process Process;
        public int RefreshInterval = 1000;
        public int Scale = 100;
        Dictionary<int, TimeSpan> times = new Dictionary<int, TimeSpan>();
        MeterEventArgs args = new MeterEventArgs();
        Timer timer;
        public int[] thresholds = new int[] { 0, 5, 10, 25, 50, 75, 90 };
        public int[] winners;

        public ProcessMeter()
        {
            winners = new int[thresholds.Length];
        }

        internal void Run()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = FileName;
            startInfo.WorkingDirectory = FileName.Substring(0, FileName.LastIndexOf("\\"));
            Process = Process.Start(startInfo);
           
        }

        internal void Run(Process process)
        {
            Process = process;
            args.Finish = false;
            Plots = new Dictionary<int, Plot>();
            args.Plots = Plots;
            timer = new Timer(RefreshInterval);
            timer.Elapsed += OnTimedEvent;
            timer.Start();
            for (int i = 0; i < winners.Length; i++)
            {
                winners[i] = 0;
            }
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            if (!Process.HasExited)
            {
                Process.Refresh();
                foreach (ProcessThread thread in Process.Threads)
                {
                    try
                    {
                        if (!Plots.ContainsKey(thread.Id))
                        {
                            Plots[thread.Id] = new Plot();
                        }
                        if (times.ContainsKey(thread.Id))
                        {
                            Plot plot = Plots[thread.Id];
                            double delta = (thread.TotalProcessorTime - times[thread.Id]).TotalMilliseconds;
                            int usage = (int)Math.Ceiling((delta / RefreshInterval) * Scale);
                            plot.Values.Dequeue();
                            plot.Values.Enqueue(usage);
                            if (usage > plot.Best)
                            {
                                for (int i = 0; i < thresholds.Length; i++)
                                {
                                    int threshold = thresholds[i];
                                    if (usage >= threshold && plot.Best < threshold)
                                    {
                                        winners[i]++;
                                    }
                                }
                                plot.Best = usage;
                            }
                        }

                        times[thread.Id] = thread.TotalProcessorTime;
                    }
                    catch
                    {
                    }
                }
            }
            else
            {
                args.Finish = true;
                timer.Stop();
            }
            Changed(this, args);
        }


        
    }
}
