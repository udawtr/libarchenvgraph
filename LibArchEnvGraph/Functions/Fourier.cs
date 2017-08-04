using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Functions
{
    /// <summary>
    /// フーリエの熱伝導式
    /// </summary>
    public class Fourier : IVariable<double>
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
        public double dt { get; set; } = 1.0;

        /// <summary>
        /// 温度1 [K]
        /// </summary>
        public IVariable<double> T1 { get; set; }

        /// <summary>
        /// 温度2 [K]
        /// </summary>
        public IVariable<double> T2 { get; set; }

        public double Get(int t)
        {
            System.Diagnostics.Debug.Assert(dx > 0);

            var dU = -1.0 * dt * Rambda * S * (T1.Get(t) - T2.Get(t)) / dx;
            return dU;
        }
    }
}
