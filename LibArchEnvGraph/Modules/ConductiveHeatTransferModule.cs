using LibArchEnvGraph.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Modules
{
    /// <summary>
    /// 熱伝導モジュール
    /// 
    ///               +-----------+
    ///               |           |
    ///    HeatIn1 -->+           +--> HeatOut1
    ///               |  熱伝導M  |
    ///    HeatIn2 -->+           +--> HeatOut2
    ///               |           |
    ///               +-----------+
    ///
    /// 入力:
    /// - 熱伝達率 Rambda [W/mK]
    /// - 厚み dx [m]
    /// - 表面積 S [m2]
    /// - 表面温度 TempIn [K]
    /// 
    /// 出力:
    /// - 熱伝導による熱移動量 HeatOut [W]
    /// </summary>
    /// <remarks>
    /// 
    ///            +-----------+
    ///            |           |
    /// TempIn1 -->+ F.Fourier |
    ///            |           +--+------------------> HeatOut1
    /// TempIn2 -->+           |  |
    ///            |           |  |   +----------+
    ///            +-----------+  |   |          |
    ///                           +---+ F.Invert +---> HeatOut2
    ///                               |          |
    ///                               +----------+  
    /// </remarks>
    /// <seealso cref="Fourier"/>
    public class ConductiveHeatTransferModule : HeatTransferModule
    {
        /// <summary>
        /// 熱伝導率 [W/mK]
        /// </summary>

        public double Rambda { get; set; }

        /// <summary>
        /// 固体(壁体)の厚み [m]
        /// </summary>
        public double dx { get; set; }

        /// <summary>
        /// 熱流の通過面積 [m2]
        /// </summary>
        public double S { get; set; }

        public override void Init(FunctionFactory F)
        {
            var fourier = F.Fourier(dx, Rambda, S, TempIn[0], TempIn[1]);

            (HeatOut[0] as LinkVariable<double>).Link = fourier;
            (HeatOut[1] as LinkVariable<double>).Link = new Invert(fourier);
        }
    }
}
