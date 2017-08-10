using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Modules
{

    /// <summary>
    /// 熱貫流モジュール
    /// 
    ///             +-----------+
    ///             |           |
    ///  TempIn1 -->+           +--> HeatOut1
    ///             |  熱貫流M  |
    ///  TempIn2 -->+           +--> HeatOut2
    ///             |           |
    ///             +-----+-----+
    ///                   |
    ///               K --+
    ///               S --+
    /// 入力:
    /// - 表面温度 T1,T2 [K]
    /// - 熱貫流率 K [W/m2K]
    /// - 面積 S [m2]
    /// 
    /// 出力:
    /// - 貫流熱量 HeatOut [W]
    /// </summary>
    /// <remarks>
    ///            +-----------+
    ///            |           |
    /// TempIn1 -->+ F.Overall |
    ///            |   Heat    +--+------------------> HeatOut2
    /// TempIn2 -->+   Transmi |  |
    ///            |   ssion   |  |   +----------+
    ///            +-----------+  |   |          |
    ///                           +---+ F.Invert +---> HeatOut1
    ///                               |          |
    ///                               +----------+  
    /// </remarks>
    public class OverallHeatTransmissionModule : HeatTransferModule
    {
        /// <summary>
        /// 熱貫流率 [W/m2K]
        /// </summary>
        public double K { get; set; }

        /// <summary>
        /// 面積 [m2]
        /// </summary>
        public double S { get; set; }

        public override void Init(FunctionFactory F)
        {
            var q = F.OverallHeatTransmission(TempIn[0], TempIn[1], K, S);

            (HeatOut[0] as LinkVariable<double>).Link = F.Invert(q);
            (HeatOut[1] as LinkVariable<double>).Link = q;
        }
    }
}
