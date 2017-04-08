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
        public RelayCommand AddBatch { get; private set; }
        public RelayCommand Stop { get; private set; }
        public RelayCommand Abort { get; private set; }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            // create a background pipeline and pre-allocate 2 seconds of frames
            this.pipeline = new BackgroundPipeline<KinectFrame>(Kinect2Metrics.CameraRate, 60, () => this.CreateFrame());
            this.pipeline.Modules.Add(new DummyModule { IsEnabled = true });
            pipeline.Timer.Tick += (sender, args) =>
            {
                this.FPS = ((PipelineTimer)sender).FPS;
                this.PoolFrames = pipeline.FramePool.Count;
                this.BackLog = pipeline.Count;

                this.Log += $"{this.FPS},{this.PoolFrames},{this.BackLog}\n";
            };

            // return the frame to the pool
            this.pipeline.FrameComplete += (sender, frame) =>
            {
                this.pipeline.FramePool.PutFrame(frame);
            };

            this.Start = new RelayCommand(this.StartPipeline, () => !this.pipeline.Timer.IsRunning);
            this.AddBatch = new RelayCommand(this.AddBatchToPipeline, () => this.pipeline.Timer.IsRunning);
            this.Stop = new RelayCommand(this.StopPipeline, () => this.pipeline.Timer.IsRunning);
            this.Abort = new RelayCommand(this.AbortPipeline, () => this.pipeline.Timer.IsRunning);
        }

        private async void AddBatchToPipeline()
        {
            for (var i = 0; i < 100; i++)
            {
                var frame = this.pipeline.FramePool.GetFrame();
                await this.pipeline.Enqueue(frame);
            }
        }

        private void AbortPipeline()
        {
            this.pipeline.Abort();
            this.Start.RaiseCanExecuteChanged();
            this.Stop.RaiseCanExecuteChanged();
            this.Abort.RaiseCanExecuteChanged();
            this.AddBatch.RaiseCanExecuteChanged();
        }

        private void StartPipeline()
        {
            this.Log = "FPS, PoolFrames, BackLog\n";
            this.pipeline.Start();
            this.Start.RaiseCanExecuteChanged();
            this.Stop.RaiseCanExecuteChanged();
            this.Abort.RaiseCanExecuteChanged();
            this.AddBatch.RaiseCanExecuteChanged();
        }

        private void StopPipeline()
        {
            this.pipeline.Stop();
            File.WriteAllText("report.csv", this.Log);
            this.Start.RaiseCanExecuteChanged();
            this.Stop.RaiseCanExecuteChanged();
            this.Abort.RaiseCanExecuteChanged();
            this.AddBatch.RaiseCanExecuteChanged();
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