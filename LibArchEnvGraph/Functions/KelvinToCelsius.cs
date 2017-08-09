using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Functions
{
    public class KelvinToCelsius : BaseVariable<double>
    {
        public IVariable<double> Temp;

        public override double Update(int t)
        {
            return Temp.Get(t) - 273.15;
        }
    }
}
