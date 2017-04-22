using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundPipeline
{
    public abstract class PipelineModuleBase<T> : IPipelineModule<T>, IDisposable
    {
        public bool IsEnabled
        {
            get;
            set;
        }

        ~PipelineModuleBase(){
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing) {
        }

        public abstract T Process(T frame);
    }
}
