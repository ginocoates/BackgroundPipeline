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
        public override void Dispose()
        {
            
        }

        public override KinectFrame Process(KinectFrame frame)
        {
            //Debug.WriteLine("Processing Frame...");
            Thread.Sleep(5);
            return frame;
        }
    }
}
