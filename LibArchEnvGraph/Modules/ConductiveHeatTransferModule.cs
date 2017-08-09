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
    /// 入力:
    /// - 熱伝達率 Rambda [W/mK]
    /// - 厚み dx [m]
    /// - 表面積 S [m2]
    /// - 表面温度 TempIn [K]
    /// 
    /// 出力:
    /// - 熱伝導による熱移動量 HeatOut [W]
    /// </summary>
    /// <seealso cref="Fourier"/>
    public class ConductiveHeatTransferModule : BaseModule
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

        /// <summary>
        /// 固体1の温度 [℃]
        /// </summary>
        public IVariable<double> TempIn1 { get; set; }

        /// <summary>
        /// 固体2の温度 [℃]
        /// </summary>
        public IVariable<double> TempIn2 { get; set; }

        /// <summary>
        /// 固体1の熱移動量 [J/s]
        /// </summary>
        public IVariable<double> HeatOut1 { get; private set; } = new LinkVariable<double>("固体1の熱移動量 [J/s]");

        /// <summary>
        /// 固体2の熱移動量 [J/s]
        /// </summary>
        public IVariable<double> HeatOut2 { get; private set; } = new LinkVariable<double>("固体2の熱移動量 [J/s]");

        public override void Init(FunctionFactory F)
        {
            var fourier = F.Fourier(dx, Rambda, S, TempIn1, TempIn2);

            (HeatOut1 as LinkVariable<double>).Link = fourier;
            (HeatOut2 as LinkVariable<double>).Link = new Invert(fourier);
        }
    }
}
