using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundPipeline
{
    public abstract class PipelineModuleBase<T> : IPipelineModule<T>
    {
        public bool IsEnabled
        {
            get;
            set;
        }

        public abstract void Dispose();

        public abstract T Process(T frame);
    }
}
