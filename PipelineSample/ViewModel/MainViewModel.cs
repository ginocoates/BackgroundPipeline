using BackgroundPipeline;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Kinect;
using PipelineSample.Modules;
using System;
using System.Diagnostics;
using System.IO;

namespace PipelineSample.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase, IDisposable
    {
        KinectSensor sensor;

        BackgroundPipeline<KinectFrame> pipeline;
        double pipelineFps;
        double renderFps;
        double backLog;
        TimeSpan elapsed;
        string log;
        Stopwatch stopWatch;
        int framesRendered;
        private DummyModule dummyModule;
        private MultiSourceFrameReader frameReader;

        public double FPS
        {
            get
            {
                return pipelineFps;
            }
            private set
            {
                pipelineFps = value;
                RaisePropertyChanged(() => FPS);
            }
        }
             
        public double BackLog
        {
            get
            {
                return backLog;
            }
            private set
            {
                backLog = value;
                RaisePropertyChanged(() => BackLog);
            }
        }

        public string Log
        {
            get
            {
                return log;
            }
            private set
            {
                log = value;
                RaisePropertyChanged(() => Log);
            }
        }

        public double RenderFPS
        {
            get
            {
                return renderFps;
            }

            set
            {
                renderFps = value;
                RaisePropertyChanged(() => RenderFPS);
            }
        }

        public RelayCommand Start { get; private set; }
        public RelayCommand Stop { get; private set; }

        public TimeSpan Elapsed
        {
            get
            {
                return elapsed;
            }

            set
            {
                elapsed = value;
                RaisePropertyChanged(()=>Elapsed);
            }
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            sensor = KinectSensor.GetDefault();
           
            frameReader = sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared);
            frameReader.MultiSourceFrameArrived += FrameReader_MultiSourceFrameArrived;

            sensor.Open();

            // create a background pipeline and pre-allocate 2 seconds of frames
            pipeline = new BackgroundPipeline<KinectFrame>(Kinect2Metrics.CameraRate);
            dummyModule = new DummyModule { IsEnabled = true };
            pipeline.Modules.Add(dummyModule);

            pipeline.Timer.Tick += (sender, args) =>
            {
                FPS = pipeline.Timer.FPS;
                Elapsed = pipeline.Timer.ElapsedTime;
                BackLog = pipeline.Count;
                var gc1 = GC.CollectionCount(1);
                var gc2 = GC.CollectionCount(2);
                var memory = Process.GetCurrentProcess().PrivateMemorySize64 / 100000000;
                Log += $"{pipeline.Timer.ElapsedTime},{RenderFPS},{FPS},{BackLog},{gc1},{gc2},{memory},{dummyModule.MeanInterval}\n";
            };

            pipeline.FrameStart += (sender, frame) =>
            {
            };

            // return the frame to the pool
            pipeline.FrameComplete += (sender, frame) =>
            {
                frame.Dispose();
            };

            pipeline.QueueComplete += (sender, args) =>
            {
                Console.WriteLine($"{DateTime.Now.ToString("ddMMyyyyhhmmssfffff")}: Queue Complete");
                File.WriteAllText($"report-{DateTime.Now.ToString("ddMMyyyy-hhmmss")}.csv", Log);
            };

            Start = new RelayCommand(StartPipeline, () => !pipeline.Timer.IsRunning);
            Stop = new RelayCommand(StopPipeline, () => pipeline.Timer.IsRunning);

            stopWatch = new Stopwatch();
            stopWatch.Start();
            RenderFPS = 0;
        }

        private void FrameReader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs args)
        {
            RenderFrame(args.FrameReference.AcquireFrame());
        }

        private void RenderFrame(MultiSourceFrame kinectFrame)
        {   
            calculateFPS();

            if (!pipeline.Timer.IsRunning) return;

            using (var depthFrame = kinectFrame.DepthFrameReference.AcquireFrame())
            using (var colorFrame = kinectFrame.ColorFrameReference.AcquireFrame())
            using (var irFrame = kinectFrame.InfraredFrameReference.AcquireFrame())
            {
                var frame = new KinectFrame();

                frame.Id = Guid.NewGuid();
                frame.RelativeTime = pipeline.Timer.ElapsedTime;

                depthFrame.CopyFrameDataToIntPtr(frame.InfraredPixels, (uint)Kinect2Metrics.DepthBufferLength);
                colorFrame.CopyConvertedFrameDataToIntPtr(frame.ColorPixels, (uint)Kinect2Metrics.ColorBufferLength, ColorImageFormat.Bgra);
                irFrame.CopyFrameDataToIntPtr(frame.InfraredPixels, (uint)Kinect2Metrics.IRBufferLength);
                
                pipeline.Enqueue(frame);
            }

            FPS = pipeline.Timer.FPS;
            BackLog = pipeline.Count;
        }

        private void calculateFPS()
        {
            framesRendered++;

            if (stopWatch.Elapsed.TotalSeconds >= 1)
            {
                RenderFPS = Math.Round(framesRendered / stopWatch.Elapsed.TotalSeconds);
                framesRendered = 0;
                stopWatch.Restart();
            }
        }
                
        private void StartPipeline()
        {
            Log = "Elapsed, RenderFPS, PipelineFPS, PipelineBackLog, Gen1, Gen2, Memory, Interval\n";
            
            pipeline.Start();
            Start.RaiseCanExecuteChanged();
            Stop.RaiseCanExecuteChanged();          
        }

        private void StopPipeline()
        {
            pipeline.Stop();
            Start.RaiseCanExecuteChanged();
            Stop.RaiseCanExecuteChanged();
            Console.WriteLine($"{DateTime.Now.ToString("ddMMyyyyhhmmssfffff")}: Pipeline Stopped");
        }

        ~MainViewModel() {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing) {
                pipeline.Dispose();
            }
        }
    }
}