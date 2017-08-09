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
    /// 入力:
    /// - 表面積 S [m2]
    /// - 表面温度 TempIn [K]
    /// - c値 c [-]
    /// 
    /// 出力:
    /// - 対流熱伝達量 HeatOut [W]
    /// </summary>
    /// <seealso cref="ConvectiveHeatTransferModule"/>
    /// <seealso cref="NaturalConvectiveHeatTransferRate"/>
    public class NaturalConvectiveHeatTransferModule : BaseModule
    {
        /// <summary>
        /// 表面積(伝熱面積) [m2]
        /// </summary>
        public double S { get; set; }

        /// <summary>
        /// 固体(壁体)/流体(空気)の表面温度 [K]
        /// </summary>
        public IVariable<double>[] TempIn { get; set; } = new IVariable<double>[2];

        /// <summary>
        /// c値(自然対流作用の程度)
        /// </summary>
        /// <seealso cref="NaturalConvectiveHeatTransferRate"/>
        public double cValue { get; set; }

        /// <summary>
        /// 固体(壁体)/流体(空気)の対流熱伝達量 [W]
        /// </summary>
        public IVariable<double>[] HeatOut { get; private set; } = new[] {
            new LinkVariable<double>("固体(壁体)/流体(空気)の自然対流伝熱 [J/s]"),
            new LinkVariable<double>("固体(壁体)/流体(空気)の自然対流伝熱 [J/s]"),
        };

        /// <summary>
        /// 流体(空気)の移動熱量 [W]
        /// </summary>
        public IVariable<double> HeatOut1 { get; private set; } = new LinkVariable<double>();

        /// <summary>
        /// 固体(壁体)の移動熱量 [W]
        /// </summary>
        public IVariable<double> HeatOut0 { get; private set; } = new LinkVariable<double>();

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
