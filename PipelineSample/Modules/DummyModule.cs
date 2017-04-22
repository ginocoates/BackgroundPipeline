using BackgroundPipeline;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PipelineSample.Modules
{
    class DummyModule : PipelineModuleBase<KinectFrame>
    {
        private double numFrames = 0;
        private double total = 0;

        public double MeanInterval
        {
            get
            {
                if (numFrames == 0) return 0;
                return total / numFrames;
            }
        }

        private TimeSpan LastTimestamp;

        public override KinectFrame Process(KinectFrame frame)
        {
            numFrames++;
            total += (frame.RelativeTime - LastTimestamp).TotalMilliseconds;

            // simulate a slow operation
            var end = DateTime.Now + TimeSpan.FromMilliseconds(40);
            while (DateTime.Now < end) ;
            
            LastTimestamp = frame.RelativeTime;

            return frame;
        }
    }
}
