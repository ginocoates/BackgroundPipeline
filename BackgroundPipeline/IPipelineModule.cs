using System;

namespace BackgroundPipeline
{
    public interface IPipelineModule<T> : IDisposable
    {
        /// <summary>
        /// Gets or sets a value indicating whether this module is enabled.
        /// </summary>
        bool Enabled { get; set;  }

        /// <summary>
        /// Process the frame
        /// </summary>
        /// <param name="frame">The frame to process</param>
        /// <returns>A modified/augmented frame from the module</returns>
        T Process(T frame);
    }
}
