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
    ///             +-------------+
    ///             |             |
    ///  TempIn1 -->+             +--> HeatOut1
    ///             | 換気熱移動M |
    ///  TempIn2 -->+             +--> HeatOut2
    ///             |             |
    ///             +------+------+
    ///                    |
    ///            c_air --+
    ///           ro_air --+
    ///                V --+
    /// 入力:
    /// - 流体(空気)比熱 c_air [J/kgK]
    /// - 流体(空気)密度 ro_air [kg/m3]
    /// - 固体(壁体)/流体(空気)の表面温度 TempIn [K]
    /// - 流体(空気)の容積 V [m3]
    /// 
    /// 出力:
    /// - 固体(壁体)/流体(空気)の移動熱量 HeatOut [W]
    /// </summary>
    /// <remarks>
    /// 
    ///            +-----------+
    ///            |           |
    /// TempIn1 -->+ F.Ventila |
    ///            | tionHeatT +--+------------------> HeatOut2
    /// TempIn2 -->+ ransfer   |  |
    ///            |           |  |   +----------+
    ///            +-----------+  |   |          |
    ///                           +---+ F.Invert +---> HeatOut1
    ///                               |          |
    ///                               +----------+  
    /// </remarks>
    public class VentilationHeatTransferModule : HeatTransferModule
    {
        /// <summary>
        /// 流体(空気)容積比熱 [kJ/m3K]
        /// </summary>
        public double cro { get; set; } = 1.007 * 1.024;

        /// <summary>
        /// 流体(空気)の容積 [m3]
        /// </summary>
        public double V { get; set; }


        public override void Init(FunctionFactory F)
        {
            if (cro <= 0.0) throw new InvalidOperationException("容積比熱を設定してください。");
            if (V <= 0.0) throw new InvalidOperationException("容積を設定してください。");

            var baseFunction = F.VentilationHeatTransfer(V, TempIn[0], TempIn[1], cro);

            (HeatOut[0] as LinkVariable<double>).Link = new Functions.Invert(baseFunction);
            (HeatOut[1] as LinkVariable<double>).Link = baseFunction;
        }
    }
}
