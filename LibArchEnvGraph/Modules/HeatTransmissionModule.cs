using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Modules
{

    /// <summary>
    /// 熱貫流モジュール
    /// </summary>
    public class HeatTransmissionModule : BaseModule
    {
        public IVariable<double> T1 { get; set; }

        public IVariable<double> T2 { get; set; }

        /// <summary>
        /// 熱貫流率 [W/m2K]
        /// </summary>
        public double K { get; set; }

        /// <summary>
        /// 面積 [m2]
        /// </summary>
        public double S { get; set; }

        /// <summary>
        /// 単位時間 [s]
        /// </summary>
        public double dt { get; set; } = 1.0;

        /// <summary>
        /// 貫流熱量 [J]
        /// </summary>
        public IVariable<double> HeatOut1 { get; private set; } = new LinkVariable<double>();

        /// <summary>
        /// 貫流熱量 [J]
        /// </summary>
        public IVariable<double> HeatOut2 { get; private set; } = new LinkVariable<double>();

        public override void Init(FunctionFactory F)
        {
            var q = F.HeatTransmission(T1, T2, K, S, dt);

            (HeatOut1 as LinkVariable<double>).Link = F.Invert(q);
            (HeatOut2 as LinkVariable<double>).Link = q;
        }
    }
}
