﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace BackgroundPipeline
{
    public class BackgroundPipeline<T> : IBackgroundPipeline<T> where T : struct
    {
        BlockingCollection<T> frameQueue;

        CancellationTokenSource cancellationToken;

        Task processingTask;

        private int queueSize;

        public event EventHandler<T> FrameComplete;

        public event EventHandler QueueComplete;

        public List<IPipelineModule<T>> Modules { get; private set; }

        public PipelineTimer Timer { get; private set; }

        public BackgroundPipeline(int frequencyHz, int queueSize)
        {
            if (queueSize <= 0)
            {
                throw new ArgumentOutOfRangeException("Please specify a valid pool size!");
            }

            this.queueSize = queueSize;

            this.Timer = new PipelineTimer(frequencyHz);

            this.Modules = new List<IPipelineModule<T>>();
        }

        public int Count
        {
            get
            {
                return this.frameQueue.Count;
            }
        }

        public void Start()
        {
            if (this.Timer.IsRunning) return;

            // create a concurrent queue for thread safe FIFO processing.
            // wrapped in a blocking collection so that the pipeling blocks and waits for frames
            // to process
            this.frameQueue = new BlockingCollection<T>(new ConcurrentQueue<T>(), this.queueSize);

            this.Timer.Start();

            this.cancellationToken = new CancellationTokenSource();

            this.processingTask = Task.Factory.StartNew(() =>
            {
                while (this.frameQueue != null && !this.frameQueue.IsCompleted && !this.cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        // blocks until frame is available
                        T frame;

                        if (!this.frameQueue.TryTake(out frame, -1, this.cancellationToken.Token)) continue;

                        ProcessFrame(frame);
                    }
                    catch (OperationCanceledException)
                    {
                        Debug.WriteLine("Queue processing cancelled.");
                    }

                }

                this.OnQueueComplate();

            }, this.cancellationToken.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public void Stop()
        {
            if (!this.Timer.IsRunning) return;
            this.Timer.Stop();
            this.frameQueue.CompleteAdding();
        }

        public void Abort()
        {
            this.cancellationToken.Cancel();
            this.Stop();
        }

        private void ProcessFrame(T frame)
        {
            foreach (var module in this.Modules.Where(m => m.IsEnabled))
            {
                module.Process(frame);
            }

            this.Timer.Increment();
            this.OnFrameComplete(frame);
        }

        private void OnFrameComplete(T frame)
        {
            if (this.FrameComplete != null)
            {
                this.FrameComplete(this, frame);
            }
        }


        private void OnQueueComplate()
        {
            if (this.QueueComplete != null) {
                this.QueueComplete(this, EventArgs.Empty);
            }
        }

        public async Task Enqueue(T frame)
        {
            if (!this.Timer.IsRunning || this.frameQueue.IsCompleted || this.frameQueue.IsAddingCompleted) return;

            await Task.Run(() =>
            {
                try
                {
                    this.frameQueue.Add(frame);
                }
                catch (InvalidOperationException ex) {
                    Debug.WriteLine("Failed to add to queue:" + ex.ToString());
                }
            });
        }

        public bool IsCompleted { get { return this.frameQueue == null || this.frameQueue.IsCompleted; } }

        public bool IsAddingCompleted { get { return this.frameQueue == null || this.frameQueue.IsAddingCompleted; } }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    foreach (IPipelineModule<T> module in this.Modules)
                    {
                        module.Dispose();
                    }
                    this.frameQueue.Dispose();
                    this.Timer.Dispose();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                }
            }
        }
    }
}
