using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Functions
{
    /// <summary>
    /// 熱量から温度 [K] への変換
    /// </summary>
    public class HeatToTemp : BaseVariable<double>
    {
        /// <summary>
        /// 容積 [m3]
        /// </summary>
        public double V { get; set; }

        /// <summary>
        /// 容積比熱 cρ [kJ/m^3・K]
        /// </summary>
        public double Cro { get; set; }

        /// <summary>
        /// 熱量 [J]
        /// </summary>
        public IVariable<double> Heat { private get; set; }

        public override double Update(int t)
        {
            var C = Cro * 1000 * V;
            var T = Heat.Get(t) / C;

            return T;
        }
    }
}
