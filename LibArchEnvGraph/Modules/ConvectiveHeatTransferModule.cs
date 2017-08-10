using LibArchEnvGraph.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Modules
{
    /// <summary>
    /// 対流熱移動モジュール
    /// 
    ///             +---------------+
    ///             |               |
    ///  TempIn1 -->+               +--> HeatOut1
    ///             |               |
    ///  TempIn2 -->+  対流熱移動M  +--> HeatOut2
    ///             |               |
    ///  alpha_c -->+               |
    ///             |               |
    ///             +-------+-------+
    ///                     |
    ///                     S
    /// 入力:
    /// - 表面積 S [m2]
    /// - 表面温度 TempIn [K]
    /// - 対流熱伝達率 alpha_c [W/m2K]
    /// 
    /// 出力:
    /// - 対流伝熱量 HeatOut [W]
    /// </summary>
    /// <remarks>
    /// 
    ///            +-----------+
    ///            |           |
    /// TempIn1 -->+ F.Newton  |
    ///            |   Cooling +--+------------------> HeatOut2
    /// TempIn2 -->+           |  |
    ///            |           |  |   +----------+
    ///            +-----------+  |   |          |
    ///                           +---+ F.Invert +---> HeatOut1
    ///                               |          |
    ///                               +----------+  
    /// </remarks>
    /// <seealso cref="NewtonCooling"/>
    public class ConvectiveHeatTransferModule : HeatTransferModule
    {
        /// <summary>
        /// 表面積(伝熱面積) [m2]
        /// </summary>
        public double S { get; set; }

        /// <summary>
        /// 対流熱伝達率 [W/m2K]
        /// </summary>
        public IVariable<double> alpha_c { get; set; }


        public override void Init(FunctionFactory F)
        {
            var newtonCooling = F.NewtonCooling(S, TempIn[0], TempIn[1], alpha_c);
            (HeatOut[0] as LinkVariable<double>).Link = new Invert(newtonCooling);
            (HeatOut[1] as LinkVariable<double>).Link = newtonCooling;

            newtonCooling.Label = $"対流熱伝達量 ({TempIn[0].Label}-{TempIn[1].Label})";
            HeatOut[0].Label = $"対流熱伝達量 ({TempIn[0].Label}-{TempIn[1].Label})";
            HeatOut[1].Label = $"対流熱伝達量 ({TempIn[0].Label}-{TempIn[1].Label})";
        }
    }
}
