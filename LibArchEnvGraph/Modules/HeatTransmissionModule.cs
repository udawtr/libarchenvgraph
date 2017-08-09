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
    /// 入力:
    /// - 表面温度 T1,T2 [K]
    /// - 熱貫流率 K [W/m2K]
    /// - 面積 S [m2]
    /// 
    /// 出力:
    /// - 貫流熱量 HeatOut [W]
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
        /// 貫流熱量 [W]
        /// </summary>
        public IVariable<double>[] HeatOut { get; private set; } = new[] {
            new LinkVariable<double>("貫流熱量 [W]"),
            new LinkVariable<double>("貫流熱量 [W]")
        };

        public override void Init(FunctionFactory F)
        {
            var q = F.HeatTransmission(T1, T2, K, S);

            (HeatOut[0] as LinkVariable<double>).Link = F.Invert(q);
            (HeatOut[1] as LinkVariable<double>).Link = q;
        }
    }
}
