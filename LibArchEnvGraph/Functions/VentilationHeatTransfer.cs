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
    public class VentilationHeatTransfer : IVariable<double>
    {
        /// <summary>
        /// 流体(空気)比熱 [J/kgK]
        /// </summary>
        public double c_air { get; set; } = 1.007;

        /// <summary>
        /// 流体(空気)密度 [kg/m3]
        /// </summary>
        public double ro_air { get; set; } = 1.024;

        /// <summary>
        /// 固体(壁体)の表面温度 [℃]
        /// </summary>
        public IVariable<double> Ts { get; set; }

        /// <summary>
        /// 流体(空気)の温度 [℃]
        /// </summary>
        public IVariable<double> Tf { get; set; }

        /// <summary>
        /// 流体(空気)の容積 [m3]
        /// </summary>
        public double V { get; set; }

        public double Get(int t)
        {
            var dU = c_air * ro_air * V * (Ts.Get(t) - Tf.Get(t));
            return dU;
        }
    }
}
