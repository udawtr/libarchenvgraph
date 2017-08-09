using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph
{
    public interface ICalculationGraph
    {
        string Label { get; set; }

        void Init(FunctionFactory F);

        void Commit(int t);
    }
}
