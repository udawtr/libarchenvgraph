using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Functions
{
    /// <summary>
    /// 減算
    /// </summary>
    public class Subtract : BaseVariable<double>
    {
        private IVariable<double> var_A;
        private IVariable<double> var_B;

        public Subtract(IVariable<double> var_A, IVariable<double> var_B)
        {
            this.var_A = var_A;
            this.var_B = var_B;
        }

        public override double Update(int t)
        {
            return var_A.Get(t) - var_B.Get(t);
        }
    }
}
