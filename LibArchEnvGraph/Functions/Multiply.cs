using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Functions
{
    /// <summary>
    /// 乗算
    /// </summary>
    public class Multiply : BaseVariable<double>
    {
        private IVariable<double> var_A;
        private IVariable<double> var_B;


        public Multiply(IVariable<double> var_A, IVariable<double> var_B)
        {
            this.var_A = var_A;
            this.var_B = var_B;
        }

        public override double Update(int t)
        {
            var A = var_A.Get(t);
            var B = var_B.Get(t);

            System.Diagnostics.Debug.Assert(!Double.IsNaN(A));
            System.Diagnostics.Debug.Assert(!Double.IsNaN(B));

            return A * B;
        }
    }
}
