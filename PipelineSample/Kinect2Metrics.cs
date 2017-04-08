using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PipelineSample
{
    public class Kinect2Metrics
    {
        /// <summary>
        /// Capture rate of Kinect
        /// </summary>
        public static int CameraRate = 30;

        /// <summary>
        /// Unit of measurement for Kinect
        /// </summary>
        public static string Units = "m";

        /// <summary>
        /// Number of tracked joints for Kinect
        /// </summary>
        public static int TrackedJoints = 25;

        /// <summary>
        /// Kinect DPI.
        /// </summary>
        public static readonly double DPI = 96.0;

        /// <summary>
        /// Default format.
        /// </summary>
        public static readonly PixelFormat FORMAT = PixelFormats.Bgr32;

        /// <summary>
        /// Bytes per pixel in depth stream.
        /// </summary>
        public static readonly int BytesPerPixelColorDepth = (FORMAT.BitsPerPixel + 7) / 8;

        /// <summary>
        /// Number of bytes per pixel in the depth stream
        /// </summary>
        public static readonly int BytesPerPixelDepth = 2;

        /// <summary>
        /// Bytes per pixel in the RGB stream
        /// </summary>
        public static readonly int BytesPerPixelRGB = FORMAT.BitsPerPixel / 8;

        /// <summary>
        /// Width of depth stream
        /// </summary>
        public static readonly int DepthFrameWidth = 512;

        /// <summary>
        /// Height of depth stream
        /// </summary>
        public static readonly int DepthFrameHeight = 424;

        /// <summary>
        /// Stride for the depth frame
        /// </summary>
        public static readonly int DepthStride = Kinect2Metrics.DepthFrameWidth * Kinect2Metrics.BytesPerPixelDepth;

        /// <summary>
        /// Length of depth buffer
        /// </summary>
        public static int DepthBufferLength = Kinect2Metrics.DepthStride * Kinect2Metrics.DepthFrameHeight;

        /// <summary>
        /// Width of IR stream
        /// </summary>
        public static readonly int IRFrameWidth = 512;

        /// <summary>
        /// Height of IR stream
        /// </summary>
        public static readonly int IRFrameHeight = 424;

        /// <summary>
        /// Number of bytes per pixel in the depth stream
        /// </summary>
        public static readonly int BytesPerPixelIR = 2;

        /// <summary>
        /// Stride for the IR frame
        /// </summary>
        public static readonly int IRStride = Kinect2Metrics.IRFrameWidth * Kinect2Metrics.BytesPerPixelIR;

        /// <summary>
        /// Length of depth buffer
        /// </summary>
        public static int IRBufferLength = Kinect2Metrics.IRFrameWidth * Kinect2Metrics.IRFrameHeight;

        /// <summary>
        /// Width of RGB frame
        /// </summary>
        public static int RGBFrameWidth = 1920;

        /// <summary>
        /// Height of RGB Frame
        /// </summary>
        public static int RGBFrameHeight = 1080;

        /// <summary>
        /// The stride for color frames
        /// </summary>
        public static int ColorStride = Kinect2Metrics.RGBFrameWidth * Kinect2Metrics.BytesPerPixelRGB;

        /// <summary>
        /// Buffer length for RGB frames
        /// </summary>
        public static int ColorBufferLength = ColorStride * Kinect2Metrics.RGBFrameHeight;

        /// <summary>
        /// Size of the mapped color pixels buffer
        /// </summary>
        public static int MappedColorToCameraSpaceBufferLength = Kinect2Metrics.RGBFrameWidth * Kinect2Metrics.RGBFrameHeight * Marshal.SizeOf(new CameraSpacePoint());

        public static int MappedDepthToColorSpaceBufferLength = Kinect2Metrics.RGBFrameWidth * Kinect2Metrics.RGBFrameHeight * Marshal.SizeOf(new DepthSpacePoint());

        /// <summary>
        /// Size of the mapped depth pixels buffer
        /// </summary>
        public static int MappedDepthToColorBufferLength = Kinect2Metrics.DepthFrameWidth * Kinect2Metrics.DepthFrameHeight * Marshal.SizeOf(new ColorSpacePoint());

        public static int MappedDepthToCameraBufferLength = Kinect2Metrics.DepthFrameWidth * Kinect2Metrics.DepthFrameHeight * Marshal.SizeOf(new CameraSpacePoint());
    }
}
