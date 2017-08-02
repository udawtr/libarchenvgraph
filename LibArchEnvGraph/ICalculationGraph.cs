using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph
{
    public interface ICalculationGraph
    {
        void Init(FunctionFactory F);

        void Commit(int t);
    }
}
