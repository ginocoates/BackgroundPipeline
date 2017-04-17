using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PipelineSample
{
    class KinectFrame : IDisposable
    {
        public Guid Id { get; set; }
        public IntPtr ColorPixels { get; set; } = IntPtr.Zero;
        public IntPtr DepthPixels { get; set; } = IntPtr.Zero;
        public IntPtr InfraredPixels { get; set; } = IntPtr.Zero;
        public TimeSpan RelativeTime { get; set; }

        public KinectFrame()
        {            
            this.ColorPixels = Marshal.AllocHGlobal(Kinect2Metrics.ColorBufferLength);
            this.DepthPixels = Marshal.AllocHGlobal(Kinect2Metrics.DepthBufferLength);
            this.InfraredPixels = Marshal.AllocHGlobal(Kinect2Metrics.IRBufferLength);
            GC.AddMemoryPressure(Kinect2Metrics.ColorBufferLength + Kinect2Metrics.DepthBufferLength + Kinect2Metrics.IRBufferLength);
        }

        ~KinectFrame()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // clean up managed resources
            }

            //clean up unmanaged resources
            Marshal.FreeHGlobal(this.ColorPixels);
            Marshal.FreeHGlobal(this.DepthPixels);
            Marshal.FreeHGlobal(this.InfraredPixels);
            GC.RemoveMemoryPressure(Kinect2Metrics.ColorBufferLength + Kinect2Metrics.DepthBufferLength + Kinect2Metrics.IRBufferLength);
        }
    }
}
