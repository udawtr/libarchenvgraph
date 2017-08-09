﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Functions
{
    /// <summary>
    /// 熱貫流率による熱移動の計算
    /// </summary>
    public class OverallHeatTransmission : BaseVariable<double>
    {

        public double K { get; set; }

        /// <summary>
        /// 熱流の通過面積 [m2]
        /// </summary>
        public double S { get; set; }

        /// <summary>
        /// 温度1 [K]
        /// </summary>
        public IVariable<double> T1 { get; set; }

        /// <summary>
        /// 温度2 [K]
        /// </summary>
        public IVariable<double> T2 { get; set; }

        public override double Update(int t)
        {
            System.Diagnostics.Debug.Assert(K > 0);

            var dU = K * S * (T1.Get(t) - T2.Get(t));

            System.Diagnostics.Debug.WriteLine($"[{t}] OverallHeatTransmission: {T1.Label}:{T1.Get(t)} -> {T2.Label}:{T2.Get(t)} = {dU} [W]");

            return dU;
        }
    }
}
