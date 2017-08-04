using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Functions
{
    /// <summary>
    /// 多変数加算
    /// </summary>
    public class Concat : IVariable<double>
    {
        private IVariable<double>[] var_in;

        public Concat(params IVariable<double>[] var_in)
        {
            this.var_in = var_in;
        }
        public Concat(IEnumerable<IVariable<double>> var_in)
        {
            this.var_in = var_in.ToArray();
        }

        public double Get(int t)
        {
            var sum = 0.0;
            foreach (var v in var_in)
            {
                sum += v.Get(t);
                System.Diagnostics.Debug.Assert(!Double.IsNaN(sum));
            }
            return sum;
        }
    }
}
