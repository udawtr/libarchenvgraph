using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Functions
{
    /// <summary>
    /// ニュートンの冷却測
    /// </summary>
    public class NewtonCooling : IVariable<double>
    {
        /// <summary>
        /// 表面積(伝熱面積) [m2]
        /// </summary>
        public double S { get; set; }

        /// <summary>
        /// 固体(壁体)の表面温度 [℃]
        /// </summary>
        public IVariable<double> Ts { get; set; }

        /// <summary>
        /// 流体(空気)の温度 [℃]
        /// </summary>
        public IVariable<double> Tf { get; set; }

        /// <summary>
        /// 対流熱伝達率 [-]
        /// </summary>
        /// <seealso cref="NaturalConvectiveHeatTransferRate"/>
        public IVariable<double> alpha_c { get; set; }

        public double Get(int t)
        {
            var dT = Ts.Get(t) - Tf.Get(t);
            var dU = alpha_c.Get(t) * dT * S;
            return dU;
        }
    }
}
