using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Functions
{
    /// <summary>
    /// シュテファン-ボルツマンの法則
    /// </summary>
    public class StefanBolzmann : BaseVariable<double>
    {
        /// <summary>
        /// 灰色体1の放射率 [-]
        /// </summary>
        public double e1 { get; set; }

        /// <summary>
        /// 灰色体2の放射率 [-]
        /// </summary>
        public double e2 { get; set; }

        /// <summary>
        /// 灰色体1の面から灰色体2の面への形態係数 [-]
        /// </summary>
        public double F12 { get; set; }

        /// <summary>
        /// 灰色体1の表面温度 [K]
        /// </summary>
        public IVariable<double> T1 { get; set; }

        /// <summary>
        /// 灰色体2の表面温度 [K]
        /// </summary>

        public IVariable<double> T2 { get; set; }


        public override double Update(int t)
        {
            var dU = F12 * e1 * e2 * 5.67 * (Math.Pow(T1.Get(t) / 100, 4.0) - Math.Pow(T2.Get(t) / 100, 4.0));

            System.Diagnostics.Debug.Assert(!Double.IsNaN(dU));

            System.Diagnostics.Debug.WriteLine($"[{t}] StefanBolzmann: {T1.Label}:{T1.Get(t)} -> {T2.Label}:{T2.Get(t)} = {dU} [W]");

            return dU;
        }
    }
}
