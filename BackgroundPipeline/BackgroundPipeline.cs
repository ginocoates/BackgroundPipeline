﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;

namespace BackgroundPipeline
{
    public class BackgroundPipeline<T> : IBackgroundPipeline<T> where T : struct
    {
        BlockingCollection<T> frameQueue;

        CancellationTokenSource cancellationToken;

        Task processingTask;

        bool restrictSize;

        public List<IPipelineModule<T>> Modules { get; private set; }

        public PipelineTimer Timer { get; private set; }

        public FramePool<T> FramePool { get; private set; }

        public BackgroundPipeline(int frequencyHz, int poolSize, Func<T> frameGenerator, bool restrictSize)
        {
            if (poolSize <= 0)
            {
                throw new ArgumentOutOfRangeException("Please specify a valid pool size!");
            }

            this.Timer = new PipelineTimer(frequencyHz);

            this.FramePool = new FramePool<T>(frameGenerator, poolSize);

            this.restrictSize = restrictSize;

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
            this.frameQueue = new BlockingCollection<T>(new ConcurrentQueue<T>(), this.FramePool.PoolSize);

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

                        // return frame to the pool
                        this.FramePool.PutFrame(frame);
                    }
                    catch (OperationCanceledException)
                    {
                        Debug.WriteLine("Queue processing cancelled.");
                    }

                }
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
            foreach (var module in this.Modules)
            {
                module.Process(frame);
                this.Timer.Increment();
            }
        }

        public async Task Enqueue(T frame)
        {
            var count = this.Count;
            var poolSize = this.FramePool.PoolSize;

            await Task.Run(() =>
            {
                if (this.frameQueue.IsCompleted || this.frameQueue.IsAddingCompleted || (this.restrictSize && count == poolSize))
                {
                    // drop frames if we have hit the size limit
                    Debug.WriteLine($"Warning: Frame dropped. Limit of {poolSize} was reached");
                    return;
                }

                this.frameQueue.Add(frame);
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
