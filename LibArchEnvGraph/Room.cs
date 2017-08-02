using LibArchEnvGraph.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph
{
    public class Room
    {
        public double cro { get; set; }
        public double V { get; set; }

        public List<WallSurface> Walls { get; set; }

        public IVariable<double> RoomTemperature { get; set; }

        public ICalculationGraph GetCalcuationGraph()
        {
            return new HeatCapacityModule()
            {
                cro = cro,
                V = V,
            };
        }
    }
}
