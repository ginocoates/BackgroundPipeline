using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace BackgroundPipeline
{
    public class BackgroundPipeline<T> : IBackgroundPipeline<T>
    {
        BlockingCollection<T> frameQueue;

        CancellationTokenSource cancellationToken;

        Task processingTask;

        public event EventHandler<T> FrameStart;

        public event EventHandler<T> FrameComplete;

        public event EventHandler QueueComplete;

        public List<IPipelineModule<T>> Modules { get; private set; }

        public PipelineTimer Timer { get; private set; }

        public BackgroundPipeline(int frequencyHz)
        {
            Timer = new PipelineTimer(frequencyHz);

            Modules = new List<IPipelineModule<T>>();
        }

        public int Count
        {
            get
            {
                return frameQueue.Count;
            }
        }

        public void Start()
        {
            if (Timer.IsRunning) return;

            // create a concurrent queue for thread safe FIFO processing.
            // wrapped in a blocking collection so that the pipeling blocks and waits for frames
            // to process
            frameQueue = new BlockingCollection<T>(new ConcurrentQueue<T>());

            Timer.Start();

            cancellationToken = new CancellationTokenSource();

            // run on a new thread
            processingTask = Task.Factory.StartNew(() =>
            {
                while (frameQueue != null && !frameQueue.IsCompleted && !cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        // blocks until frame is available
                        T frame;

                        if (frameQueue.TryTake(out frame, -1, cancellationToken.Token))
                        {
                            ProcessFrame(frame);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        Debug.WriteLine("Queue processing cancelled.");
                    }

                }

                OnQueueComplate();

            }, cancellationToken.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public void Stop()
        {
            if (!Timer.IsRunning) return;
            Timer.Stop();
            frameQueue.CompleteAdding();
        }

        public void Abort()
        {
            cancellationToken.Cancel();
            Stop();
        }

        private void ProcessFrame(T frame)
        {
            OnFrameStart(frame);

            foreach (var module in Modules.Where(m => m.IsEnabled))
            {
                module.Process(frame);
            }

            Timer.Increment();
            OnFrameComplete(frame);
        }


        private void OnFrameStart(T frame)
        {
            FrameStart?.Invoke(this, frame);
        }

        private void OnFrameComplete(T frame)
        {
            FrameComplete?.Invoke(this, frame);
        }


        private void OnQueueComplate()
        {
            QueueComplete?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Add frames to the queue
        /// </summary>
        /// <param name="frame">The frame to add</param>
        /// <returns>A task</returns>
        public void Enqueue(T frame)
        {
            if (!Timer.IsRunning) return;

            try
            {
                if (frameQueue.IsCompleted || frameQueue.IsAddingCompleted || cancellationToken.IsCancellationRequested) return;
                frameQueue.TryAdd(frame, -1, cancellationToken.Token);
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine("Failed to add to queue:" + ex.ToString());
            }
        }

        public bool IsCompleted { get { return frameQueue == null || frameQueue.IsCompleted; } }

        public bool IsAddingCompleted { get { return frameQueue == null || frameQueue.IsAddingCompleted; } }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    foreach (IPipelineModule<T> module in Modules)
                    {
                        module.Dispose();
                    }
                    frameQueue.Dispose();
                    Timer.Dispose();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                }
            }
        }
    }
}
