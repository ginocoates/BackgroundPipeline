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
        double fps;
        double poolFrames;
        double backLog;
        string log;
        private Timer timer;

        public double FPS
        {
            get
            {
                return this.fps;
            }
            private set
            {
                this.fps = value;
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

        public RelayCommand Start { get; private set; }
        public RelayCommand Stop { get; private set; }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            // create a background pipeline and pre-allocate 2 seconds of frames
            this.pipeline = new BackgroundPipeline<KinectFrame>(Kinect2Metrics.CameraRate, 60, () => this.CreateFrame(), false);
            this.pipeline.AddModule(new DummyModule());
            pipeline.Timer.Tick += (sender, args) =>
            {
                this.FPS = ((PipelineTimer)sender).FPS;
                this.PoolFrames = pipeline.FramePool.Count;
                this.BackLog = pipeline.Count;

                this.Log += $"{this.FPS},{this.PoolFrames},{this.BackLog}\n";
            };

            this.Start = new RelayCommand(this.StartPipeline, () => !this.pipeline.Timer.IsRunning);
            this.Stop = new RelayCommand(this.StopPipeline, () => this.pipeline.Timer.IsRunning);
        }

        private void StartPipeline()
        {
            this.Log = "FPS, PoolFrames, BackLog\n";
            this.pipeline.Start();
            var frequencyHz = (int)(1000.0f / 250);
            this.timer = new System.Threading.Timer(async (state) => {
                var frame = this.pipeline.FramePool.GetFrame();
                await this.pipeline.Enqueue(frame);
            }, null, 0, frequencyHz);

            this.Start.RaiseCanExecuteChanged();
            this.Stop.RaiseCanExecuteChanged();
        }

        private void StopPipeline()
        {
            this.pipeline.Stop();
            File.WriteAllText("report.csv", this.Log);
            this.timer.Change(Timeout.Infinite, Timeout.Infinite);
            this.Start.RaiseCanExecuteChanged();
            this.Stop.RaiseCanExecuteChanged();
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