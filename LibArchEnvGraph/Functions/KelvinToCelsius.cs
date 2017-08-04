using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Functions
{
    public class KelvinToCelsius : IVariable<double>
    {
        public IVariable<double> Temp;

        public double Get(int t)
        {
            return Temp.Get(t) - 273.15;
        }
    }
}
