using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Modules
{
    public abstract class BaseModule : ICalculationGraph
    {
        public string Label { get; set; }

        public virtual void Commit(int t)
        {
        }

        public virtual void Init(FunctionFactory F)
        {
        }
    }
}
