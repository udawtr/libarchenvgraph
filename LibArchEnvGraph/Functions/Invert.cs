using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Functions
{
    /// <summary>
    /// 符号逆転
    /// </summary>
    public class Invert : IVariable<double>
    {
        private IVariable<double> var_in;

        public Invert(IVariable<double> var_in)
        {
            this.var_in = var_in;
        }

        public double Get(int t)
        {
            return -1.0 * var_in.Get(t);
        }
    }
}
