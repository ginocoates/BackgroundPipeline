using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundPipeline
{
    public class FramePool<T> where T : struct
    {
        /// <summary>
        /// A concurrent bag of frames
        /// </summary>
        private ConcurrentBag<T> frameBag;

        /// <summary>
        /// User supplied frame generator function
        /// </summary>
        private Func<T> frameGenerator;

        /// <summary>
        /// Gets the number of items in the frame bag
        /// </summary>
        public int Count
        {
            get
            {
                return this.frameBag.Count;
            }
        }

        /// <summary>
        /// The initial size of the pool
        /// </summary>
        public int PoolSize { get; private set; }

        public FramePool(Func<T> frameGenerator, int poolSize)
        {
            if (frameGenerator == null) throw new ArgumentNullException("objectGenerator");
            frameBag = new ConcurrentBag<T>();
            this.frameGenerator = frameGenerator;
            this.PoolSize = poolSize;

            // preload the frame pool
            Enumerable.Range(0, poolSize).ToList().ForEach((i) => PutFrame(frameGenerator()));
        }

        /// <summary>
        /// Get a frame from the pool or create a new one
        /// </summary>
        /// <returns>A pool or newly generated frame</returns>
        public T GetFrame()
        {
            T item;
            if (frameBag.TryTake(out item)) return item;
            return frameGenerator();
        }

        /// <summary>
        /// Return a frame to the queue
        /// </summary>
        /// <param name="item">The frame to replace in the queue</param>
        public void PutFrame(T item)
        {
            frameBag.Add(item);
        }
    }
}
