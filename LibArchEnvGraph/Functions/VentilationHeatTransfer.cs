using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Functions
{
    /// <summary>
    /// 換気熱移動
    /// </summary>
    public class VentilationHeatTransfer : BaseVariable<double>
    {
        /// <summary>
        /// 流体(空気)容積比熱 [kJ/m3K]
        /// </summary>
        public double cro { get; set; } = 1.007 * 1.024;

        /// <summary>
        /// 固体(壁体)の表面温度 [K]
        /// </summary>
        public IVariable<double> T1 { get; set; }

        /// <summary>
        /// 流体(空気)の温度 [K]
        /// </summary>
        public IVariable<double> T2 { get; set; }

        /// <summary>
        /// 流体(空気)の容積 [m3]
        /// </summary>
        public double V { get; set; }

        public override double Update(int t)
        {
            var dU = cro * V * (T1.Get(t) - T2.Get(t));
            return dU;
        }
    }
}
