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
        /// 固体(壁体)の表面温度 [K]
        /// </summary>
        public IVariable<double> TempSolidIn { get; set; }

        /// <summary>
        /// 流体(空気)の温度 [K]
        /// </summary>
        public IVariable<double> TempFluidIn { get; set; }

        /// <summary>
        /// c値(自然対流作用の程度)
        /// </summary>
        /// <seealso cref="NaturalConvectiveHeatTransferRate"/>
        public double cValue { get; set; }

        /// <summary>
        /// 流体(空気)の移動熱量 [J/s]
        /// </summary>
        public IVariable<double> HeatFluidOut { get; private set; } = new LinkVariable<double>();

        /// <summary>
        /// 固体(壁体)の移動熱量 [J/s]
        /// </summary>
        public IVariable<double> HeatSolidOut { get; private set; } = new LinkVariable<double>();

        /// <summary>
        /// 初期化
        /// </summary>
        public override void Init(FunctionFactory F)
        {
            var alpha_c = new NaturalConvectiveHeatTransferRate
            {
                Ts = TempSolidIn,
                Tf = TempFluidIn,
                cValue = cValue
            };

            var baseModule = new ConvectiveHeatTransferModule
            {
                S = S,
                TempSolidIn = TempSolidIn,
                TempFluidIn = TempFluidIn,
                alpha_c = alpha_c
            };

            baseModule.Init(F);

            (HeatSolidOut as LinkVariable<double>).Link = baseModule.HeatSolidOut;
            (HeatFluidOut as LinkVariable<double>).Link = baseModule.HeatFluidOut;
        }
    }
}
