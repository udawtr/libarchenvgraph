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
    /// </summary>
    /// <seealso cref="Fourier"/>
    public class HeatConductionModule : BaseModule
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
        /// 単位時間 [s]
        /// </summary>
        public double dt { get; set; }

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
        public IVariable<double> HeatOut1 { get; private set; } = new LinkVariable<double>();

        /// <summary>
        /// 固体2の熱移動量 [J/s]
        /// </summary>
        public IVariable<double> HeatOut2 { get; private set; } = new LinkVariable<double>();

        public override void Init(FunctionFactory F)
        {
            var fourier = F.Fourier(dx, Rambda, S, TempIn1, TempIn2, dt);

            (HeatOut1 as LinkVariable<double>).Link = fourier;
            (HeatOut2 as LinkVariable<double>).Link = new Invert(fourier);
        }
    }
}
