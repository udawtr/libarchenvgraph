using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Modules
{
    public class ContainerModule : BaseModule
    {
        public List<ICalculationGraph> Modules { get; set; } = new List<ICalculationGraph>();

        public override void Init(FunctionFactory F)
        {
            foreach (var m in Modules)
            {
                m.Init(F);
            }
        }

        public override void Commit(int t)
        {
            foreach (var m in Modules)
            {
                m.Commit(t);
            }
        }
    }
}
