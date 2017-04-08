﻿using System;

namespace BackgroundPipeline
{
    public interface IPipelineModule<T> : IDisposable
    {
        /// <summary>
        /// Process the frame
        /// </summary>
        /// <param name="frame">The frame to process</param>
        /// <returns>A modified/augmented frame from the module</returns>
        T Process(T frame);
    }
}
