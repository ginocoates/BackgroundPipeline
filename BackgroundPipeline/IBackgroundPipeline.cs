using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackgroundPipeline
{
    public interface IBackgroundPipeline<T> : IDisposable
    {
        /// <summary>
        /// Timer for the pipeline for reporting FPS and notifying consumers
        /// </summary>
        PipelineTimer Timer { get; }
        
        /// <summary>
        /// Add a frame to the queue
        /// </summary>
        /// <param name="frame">Add a frame to the queue</param>
        /// <returns>An async task</returns>
        Task Enqueue(T frame);

        /// <summary>
        /// Start the pipeline processing
        /// </summary>
        void Start();

        /// <summary>
        /// End the pipeline processing
        /// </summary>
        void Stop();

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
        IReadOnlyCollection<IPipelineModule<T>> Modules {get;}

        /// <summary>
        /// Add a module to the pipeline
        /// </summary>
        /// <param name="module"></param>
        void AddModule(IPipelineModule<T> module);
    }
}
