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
        BackgroundPipeline<KinectFrame> pipeline;
        FramePool<KinectFrame> framePool;
        double pipelineFps;
        double renderFps;
        double poolFrames;
        double backLog;
        string log;
        Stopwatch stopWatch;
        int framesRendered;

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

        public double PoolFrames
        {
            get
            {
                return this.poolFrames;
            }
            private set
            {
                this.poolFrames = value;
                RaisePropertyChanged(() => this.PoolFrames);
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
        public RelayCommand Render { get; private set; }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            // create a background pipeline and pre-allocate 2 seconds of frames
            this.pipeline = new BackgroundPipeline<KinectFrame>(Kinect2Metrics.CameraRate, 60);
            this.framePool = new FramePool<KinectFrame>(this.CreateFrame, 160);
            this.pipeline.Modules.Add(new DummyModule { IsEnabled = true });

            pipeline.Timer.Tick += (sender, args) =>
            {
                this.FPS = ((PipelineTimer)sender).FPS;
                this.PoolFrames = this.framePool.Count;
                this.BackLog = pipeline.Count;

                this.Log += $"{this.RenderFPS},{this.FPS},{this.PoolFrames},{this.BackLog}\n";
            };

            // return the frame to the pool
            this.pipeline.FrameComplete += (sender, frame) =>
            {
                this.framePool.PutFrame(frame);
            };

            this.pipeline.QueueComplete += (sender, args) =>
            {
                Console.WriteLine($"Pipeline Queue Completed {DateTime.Now.ToString()}");
                File.WriteAllText("report.csv", this.Log);
            };

            this.Start = new RelayCommand(this.StartPipeline, () => !this.pipeline.Timer.IsRunning);
            this.Stop = new RelayCommand(this.StopPipeline, () => this.pipeline.Timer.IsRunning);
            this.Render = new RelayCommand(this.RenderFrame);

            this.stopWatch = new Stopwatch();
            this.stopWatch.Start();
            this.RenderFPS = 0;
        }

        private async void RenderFrame()
        {
            this.calculateFPS();

            if (!this.pipeline.Timer.IsRunning) return;

            var frame = this.framePool.GetFrame();
            await this.pipeline.Enqueue(frame);
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
            this.Log = "RenderFPS, PipelineFPS, PoolFrames, PipelineBackLog\n";
            this.pipeline.Start();
            this.Start.RaiseCanExecuteChanged();
            this.Stop.RaiseCanExecuteChanged();          
        }

        private void StopPipeline()
        {
            this.pipeline.Stop();
            this.Start.RaiseCanExecuteChanged();
            this.Stop.RaiseCanExecuteChanged();
            Console.WriteLine($"Pipeline Stopped {DateTime.Now.ToString()}");
        }

        private KinectFrame CreateFrame()
        {
            var frame = new KinectFrame
            {
                ColorPixels = new byte[Kinect2Metrics.ColorBufferLength],
                InfraredPixels = new ushort[Kinect2Metrics.IRFrameWidth * Kinect2Metrics.IRFrameHeight],
                DepthPixels = new ushort[Kinect2Metrics.DepthFrameWidth * Kinect2Metrics.DepthFrameHeight],
            };
            
            return frame;
        }
    }
}