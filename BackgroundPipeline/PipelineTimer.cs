using System;
using System.Diagnostics;
using System.Threading;

namespace BackgroundPipeline
{
    public class PipelineTimer : IDisposable
    {
        private int tickCount;
        private Stopwatch stopWatch;
        public event EventHandler Tick;
        private int frequencyHz;
        private Timer timer;

        /// <summary>
        /// Gets a value indicating the start time for the timer
        /// </summary>
        public DateTime StartTime { get; private set; }

        /// <summary>
        /// Gets a value indicating the elapsed time for the timer
        /// </summary>
        public TimeSpan ElapsedTime
        {
            get
            {
                return DateTime.Now.Subtract(this.StartTime);
            }
        }

        /// <summary>
        /// Gets a value indicating the number of ticks per second
        /// </summary>
        public double FPS
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating if the timer is running
        /// </summary>
        public bool IsRunning { get; private set; }

        public PipelineTimer(int frequencyHz)
        {
            stopWatch = new Stopwatch();
            this.frequencyHz = (int)(1000.0f / frequencyHz);
            timer = new Timer(this.CaptureTimer_Elapsed, null, Timeout.Infinite, Timeout.Infinite);            
        }

        /// <summary>
        /// Start the timer
        /// </summary>
        internal void Start()
        {
            this.StartTime = DateTime.Now;
            timer.Change(0, frequencyHz);
            tickCount = 0;
            stopWatch.Start();
            IsRunning = true;
        }

        /// <summary>
        /// Stop the timer
        /// </summary>
        internal void Stop() 
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
            stopWatch.Stop();
            this.IsRunning = false;
        }

        /// <summary>
        /// The timer has elapsed
        /// </summary>
        /// <param name="state">The timer state</param>
        private void CaptureTimer_Elapsed(object state)
        {
            // stop the timer to stop reentrance
            Tick?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Increment the number of ticks and calculate the FPS
        /// </summary>
        internal void Increment()
        {
            if (!IsRunning) return;
            tickCount++;
            calculateFPS();
        }

        /// <summary>
        /// Calculate the FPS, update every second
        /// </summary>
        private void calculateFPS()
        {
            if (this.stopWatch.Elapsed.TotalSeconds >= 1)
            {
                FPS = Math.Round(tickCount / stopWatch.Elapsed.TotalSeconds);
                tickCount = 0;
                stopWatch.Restart();
            }
        }

        ~PipelineTimer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                timer.Dispose();
            }
        }
    }
}
