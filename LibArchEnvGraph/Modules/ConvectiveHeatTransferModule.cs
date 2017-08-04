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
    /// </summary>
    /// <seealso cref="NewtonCooling"/>
    public class ConvectiveHeatTransferModule : BaseModule
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
        /// 対流熱伝達率 [-]
        /// </summary>
        public IVariable<double> alpha_c { get; set; }


        /// <summary>
        /// 流体(空気)の移動熱量 [J/s]
        /// </summary>
        public IVariable<double> HeatFluidOut { get; private set; } = new LinkVariable<double>();

        /// <summary>
        /// 固体(壁体)の移動熱量 [J/s]
        /// </summary>
        public IVariable<double> HeatSolidOut { get; private set; } = new LinkVariable<double>();

        public override void Init(FunctionFactory F)
        {
            var newtonCooling = F.NewtonCooling(S, TempSolidIn, TempFluidIn, alpha_c);
            (HeatSolidOut as LinkVariable<double>).Link = new Invert(newtonCooling);
            (HeatFluidOut as LinkVariable<double>).Link = newtonCooling;
        }
    }
}
