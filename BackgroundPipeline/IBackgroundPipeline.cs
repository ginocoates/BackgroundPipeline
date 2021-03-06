﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackgroundPipeline
{
    public interface IBackgroundPipeline<T> : IDisposable
    {
        /// <summary>
        /// Fires when a frame has been completely processed
        /// </summary>
        event EventHandler<T> FrameStart;

        /// <summary>
        /// Fires when a frame has been completely processed
        /// </summary>
        event EventHandler<T> FrameComplete;

        /// <summary>
        /// Fires when all frames have been completely processed
        /// </summary>
        event EventHandler QueueComplete;

        /// <summary>
        /// Timer for the pipeline for reporting FPS and notifying consumers
        /// </summary>
        PipelineTimer Timer { get; }
        
        /// <summary>
        /// Add a frame to the queue
        /// </summary>
        /// <param name="frame">Add a frame to the queue</param>
        /// <returns>An async task</returns>
        void Enqueue(T frame);

        /// <summary>
        /// Start the pipeline processing
        /// </summary>
        void Start();

        /// <summary>
        /// End the pipeline processing
        /// </summary>
        void Stop();

        /// <summary>
        /// Abort the capture
        /// </summary>
        void Abort();

        /// <summary>
        /// Number of frames in the queue
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Check if the pipeline is has completed adding frames and is empty
        /// Exposes the BlockingCollections IsCompleted property
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Check if the pipeline has completed adding frames
        /// Exposes the BlockingCollections IsAddingCompleted property
        /// </summary>
        bool IsAddingCompleted { get; }

        /// <summary>
        /// The collection of processing modules
        /// </summary>
        List<IPipelineModule<T>> Modules { get; }
    }
}
