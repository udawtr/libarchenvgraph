using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Modules
{
    /// <summary>
    /// 換気熱移動ブロック
    /// </summary>
    public class VentilationHeatTransferModule : BaseModule
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

        /// <summary>
        /// 流体(空気)の移動熱量 [J/s]
        /// </summary>
        public IVariable<double> dUf { get; private set; } = new LinkVariable<double>();

        /// <summary>
        /// 固体(壁体)の移動熱量 [J/s]
        /// </summary>
        public IVariable<double> dUs { get; private set; } = new LinkVariable<double>();

        public override void Init(FunctionFactory F)
        {
            var baseFunction = F.VentilationHeatTransfer(V, Ts, Tf, c_air, ro_air);

            (dUf as LinkVariable<double>).Link = new Functions.Invert(baseFunction);
            (dUs as LinkVariable<double>).Link = baseFunction;
        }
    }
}
