using System;
using BackgroundPipeline;
using GalaSoft.MvvmLight;
using PipelineSample;
using Microsoft.Kinect;
using System.Runtime.InteropServices;
using GalaSoft.MvvmLight.Command;
using System.IO;
using System.Threading;
using PipelineSample.Modules;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime;

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
    public class MainViewModel : ViewModelBase
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

        public double FPS
        {
            get
            {
                return this.pipelineFps;
            }
            private set
            {
                this.pipelineFps = value;
                RaisePropertyChanged(() => this.FPS);
            }
        }
             
        public double BackLog
        {
            get
            {
                return this.backLog;
            }
            private set
            {
                this.backLog = value;
                RaisePropertyChanged(() => this.BackLog);
            }
        }

        public string Log
        {
            get
            {
                return this.log;
            }
            private set
            {
                this.log = value;
                RaisePropertyChanged(() => this.Log);
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
                RaisePropertyChanged(() => this.RenderFPS);
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
                RaisePropertyChanged(()=>this.Elapsed);
            }
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            sensor = KinectSensor.GetDefault();

            sensor.Open();

            var frameReader = sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared);
            frameReader.MultiSourceFrameArrived += (sender, args) => {
                this.RenderFrame(args.FrameReference.AcquireFrame());
            };

            // create a background pipeline and pre-allocate 2 seconds of frames
            this.pipeline = new BackgroundPipeline<KinectFrame>(Kinect2Metrics.CameraRate);
            this.dummyModule = new DummyModule { IsEnabled = true };
            this.pipeline.Modules.Add(this.dummyModule);

            pipeline.Timer.Tick += (sender, args) =>
            {
                this.FPS = this.pipeline.Timer.FPS;
                this.Elapsed = this.pipeline.Timer.ElapsedTime;
                this.BackLog = pipeline.Count;
                var gc1 = GC.CollectionCount(1);
                var gc2 = GC.CollectionCount(2);
                var memory = Process.GetCurrentProcess().PrivateMemorySize64 / 100000000;
                this.Log += $"{this.pipeline.Timer.ElapsedTime},{this.RenderFPS},{this.FPS},{this.BackLog},{gc1},{gc2},{memory},{this.dummyModule.MeanInterval}\n";
            };

            this.pipeline.FrameStart += (sender, frame) =>
            {
            };

            // return the frame to the pool
            this.pipeline.FrameComplete += (sender, frame) =>
            {
                frame.Dispose();
            };

            this.pipeline.QueueComplete += (sender, args) =>
            {
                Console.WriteLine($"{DateTime.Now.ToString("ddMMyyyyhhmmssfffff")}: Queue Complete");
                File.WriteAllText($"report-{DateTime.Now.ToString("ddMMyyyy-hhmmss")}.csv", this.Log);
            };

            this.Start = new RelayCommand(this.StartPipeline, () => !this.pipeline.Timer.IsRunning);
            this.Stop = new RelayCommand(this.StopPipeline, () => this.pipeline.Timer.IsRunning);

            this.stopWatch = new Stopwatch();
            this.stopWatch.Start();
            this.RenderFPS = 0;
        }

        private void RenderFrame(MultiSourceFrame kinectFrame)
        {
            this.calculateFPS();

            if (!this.pipeline.Timer.IsRunning) return;
            
            var frame = new KinectFrame();
            
            frame.Id = Guid.NewGuid();
            frame.RelativeTime = this.pipeline.Timer.ElapsedTime;

            this.pipeline.Enqueue(frame);

            this.FPS = this.pipeline.Timer.FPS;
            this.BackLog = pipeline.Count;
        }

        private void calculateFPS()
        {
            this.framesRendered++;

            if (this.stopWatch.Elapsed.TotalSeconds >= 1)
            {
                this.RenderFPS = Math.Round(this.framesRendered / this.stopWatch.Elapsed.TotalSeconds);
                this.framesRendered = 0;
                this.stopWatch.Restart();
            }
        }
                
        private void StartPipeline()
        {
            this.Log = "Elapsed, RenderFPS, PipelineFPS, PipelineBackLog, Gen1, Gen2, Memory, Interval\n";
            this.pipeline.Start();
            this.Start.RaiseCanExecuteChanged();
            this.Stop.RaiseCanExecuteChanged();          
        }

        private void StopPipeline()
        {
            this.pipeline.Stop();
            this.Start.RaiseCanExecuteChanged();
            this.Stop.RaiseCanExecuteChanged();
            Console.WriteLine($"{DateTime.Now.ToString("ddMMyyyyhhmmssfffff")}: Pipeline Stopped");
        }
    }
}