using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Modules
{
    /// <summary>
    /// 換気熱移動モジュール
    /// 
    /// 入力:
    /// - 流体(空気)比熱 c_air [J/kgK]
    /// - 流体(空気)密度 ro_air [kg/m3]
    /// - 固体(壁体)/流体(空気)の表面温度 TempIn [K]
    /// - 流体(空気)の容積 V [m3]
    /// 
    /// 出力:
    /// - 固体(壁体)/流体(空気)の移動熱量 HeatOut [W]
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
        /// 固体(壁体)/流体(空気)の表面温度 [K]
        /// </summary>
        public IVariable<double>[] TempIn { get; set; } = new IVariable<double>[2];

        /// <summary>
        /// 流体(空気)の容積 [m3]
        /// </summary>
        public double V { get; set; }

        /// <summary>
        /// 固体(壁体)/流体(空気)の移動熱量 [J/s]
        /// </summary>
        public IVariable<double>[] HeatOut { get; private set; } = new[] {
            new LinkVariable<double>("固体(壁体)/流体(空気)の換気熱移動 [J/s]"),
            new LinkVariable<double>("固体(壁体)/流体(空気)の換気熱移動 [J/s]")
        };

        public override void Init(FunctionFactory F)
        {
            var baseFunction = F.VentilationHeatTransfer(V, TempIn[0], TempIn[1], c_air, ro_air);

            (HeatOut[0] as LinkVariable<double>).Link = new Functions.Invert(baseFunction);
            (HeatOut[1] as LinkVariable<double>).Link = baseFunction;
        }
    }
}
