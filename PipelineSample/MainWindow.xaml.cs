using PipelineSample.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PipelineSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Stopwatch stopwatch;

        public MainViewModel ViewModel
        {
            get { return this.DataContext as MainViewModel; }
        }

        public MainWindow()
        {
            InitializeComponent();
            stopwatch = new Stopwatch();
            stopwatch.Start();
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        /// <summary>
        /// Render a frame in the viewmodel at rate of 30fps
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (stopwatch.ElapsedMilliseconds >= 25)
            {
                stopwatch.Reset();
                stopwatch.Start();
                this.ViewModel.Render.Execute(null);
            }
        }
    }
}
