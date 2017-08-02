using LibArchEnvGraph.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph
{
    public class Wall
    {
        public double cro { get; set; }

        public double depth { get; set; }

        public double S { get; set; }

        public bool IsFloor { get; set; }

        public bool IsCeiling { get; set; }

        public ICalculationGraph GetCalcuationGraph()
        {
            return new SerialHeatConductionModule()
            {
                cro = cro,
                depth = depth,
                S = S
            };
        }
    }
}
