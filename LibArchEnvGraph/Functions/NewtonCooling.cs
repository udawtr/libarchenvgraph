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
    public class NewtonCooling : BaseVariable<double>
    {
        /// <summary>
        /// 表面積(伝熱面積) [m2]
        /// </summary>
        public double S { get; set; }

        /// <summary>
        /// 固体(壁体)の表面温度 [K]
        /// </summary>
        public IVariable<double> T1 { get; set; }

        /// <summary>
        /// 流体(空気)の温度 [K]
        /// </summary>
        public IVariable<double> T2 { get; set; }

        /// <summary>
        /// 対流熱伝達率 [-]
        /// </summary>
        /// <seealso cref="NaturalConvectiveHeatTransferRate"/>
        public IVariable<double> alpha_c { get; set; }

        public override double Get(int t)
        {
            var dT = T1.Get(t) - T2.Get(t);
            var dU = alpha_c.Get(t) * dT * S;

            System.Diagnostics.Debug.Assert(!Double.IsNaN(dU));

            System.Diagnostics.Debug.WriteLine($"[{t}] NewtonCooling: {T1.Label}:{T1.Get(t)} -> {T2.Label}:{T2.Get(t)} = {dU}");

            return dU;
        }
    }
}
