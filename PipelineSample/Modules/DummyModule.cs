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
            // simulate a slow operation
            var end = DateTime.Now + TimeSpan.FromMilliseconds(60);
            while (DateTime.Now < end) ;

            return frame;
        }
    }
}
