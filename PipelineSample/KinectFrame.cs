using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineSample
{
    struct KinectFrame
    {
        public byte[] ColorPixels { get; set; }
        public ushort[] DepthPixels { get; set; }
        public ushort[] InfraredPixels { get; set; }
    }
}
