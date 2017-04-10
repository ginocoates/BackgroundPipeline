using System;
using System.Diagnostics;
using System.Threading;

namespace BackgroundPipeline
{
    public class PipelineTimer
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
            this.stopWatch = new Stopwatch();
            this.frequencyHz = (int)(1000.0f / frequencyHz);
            timer = new Timer(this.CaptureTimer_Elapsed, null, Timeout.Infinite, Timeout.Infinite);            
        }

        /// <summary>
        /// Start the timer
        /// </summary>
        internal void Start()
        {
            this.StartTime = DateTime.Now;
            timer.Change(0, this.frequencyHz);
            this.tickCount = 0;
            this.stopWatch.Start();
            this.IsRunning = true;
        }

        /// <summary>
        /// Stop the timer
        /// </summary>
        internal void Stop() 
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
            this.stopWatch.Stop();
            this.IsRunning = false;
        }

        /// <summary>
        /// The timer has elapsed
        /// </summary>
        /// <param name="state">The timer state</param>
        private void CaptureTimer_Elapsed(object state)
        {
            if (Tick != null)
            {
                // stop the timer to stop reentrance
                Tick(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Increment the number of ticks and calculate the FPS
        /// </summary>
        internal void Increment()
        {
            this.tickCount++;
            this.calculateFPS();
        }

        /// <summary>
        /// Calculate the FPS, update every second
        /// </summary>
        private void calculateFPS()
        {
            this.tickCount++;

            if (this.stopWatch.Elapsed.TotalSeconds >= 1)
            {
                this.FPS = Math.Round(this.tickCount / this.stopWatch.Elapsed.TotalSeconds);
                this.tickCount = 0;
                this.stopWatch.Restart();
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.timer.Dispose();
            }
        }
    }
}
