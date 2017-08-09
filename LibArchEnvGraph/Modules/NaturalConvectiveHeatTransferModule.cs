using LibArchEnvGraph.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Modules
{
    /// <summary>
    /// 自然対流伝熱モジュール
    /// 
    ///             +-------------+
    ///             |             |
    ///  TempIn1 -->+             +--> HeatOut1
    ///             |  自然対流M  |
    ///  TempIn2 -->+             +--> HeatOut2
    ///             |             |
    ///             +-------------+
    ///             
    /// 入力:
    /// - 表面積 S [m2]
    /// - 表面温度 TempIn [K]
    /// - c値 c [-]
    /// 
    /// 出力:
    /// - 対流熱伝達量 HeatOut [W]
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <seealso cref="ConvectiveHeatTransferModule"/>
    /// <seealso cref="NaturalConvectiveHeatTransferRate"/>
    public class NaturalConvectiveHeatTransferModule : HeatTransferModule
    {
        /// <summary>
        /// 表面積(伝熱面積) [m2]
        /// </summary>
        public double S { get; set; }

        /// <summary>
        /// c値(自然対流作用の程度)
        /// </summary>
        /// <seealso cref="NaturalConvectiveHeatTransferRate"/>
        public double cValue { get; set; }

        /// <summary>
        /// 初期化
        /// </summary>
        public override void Init(FunctionFactory F)
        {
            var alpha_c = new NaturalConvectiveHeatTransferRate
            {
                TempIn = TempIn,
                cValue = cValue
            };

            var baseModule = new ConvectiveHeatTransferModule
            {
                S = S,
                TempIn = TempIn,
                alpha_c = alpha_c
            };

            baseModule.Init(F);

            (HeatOut[0] as LinkVariable<double>).Link = baseModule.HeatOut[0];
            (HeatOut[1] as LinkVariable<double>).Link = baseModule.HeatOut[1];
            HeatOut[0].Label = $"{Label} - 自然対流伝熱 [J/s]";
            HeatOut[1].Label = $"{Label} - 自然対流伝熱 [J/s]";
        }
    }
}
