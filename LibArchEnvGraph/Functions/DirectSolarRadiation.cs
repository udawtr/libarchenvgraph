using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Functions
{
    /// <summary>
    /// 直達日射量 [W/m2]
    /// 
    ///           +----------+
    ///           |          |
    ///  SolIn -->+          |         
    ///           |          +--> SolDirect
    /// SolPos -->+          |
    ///           |          |
    ///           +----------+
    /// </summary>
    public class DirectSolarRadiation : IVariable<double>
    {
        /// <summary>
        /// 日射量 [MJ/m2]
        /// </summary>
        private IVariable<double> SolIn;

        public string Label { get; set; }

        /// <summary>
        /// 太陽高度角[deg]
        /// </summary>
        private IVariable<double> SolH;

        private int tickTime;

        private int beginDay;

        private int days;

        private double[] data;

        public double Get(int index)
        {
            return data[index];
        }

        public DirectSolarRadiation(int tickTime, int beginDay, int days, IVariable<double> solarRadiation, IVariable<double> solH)
        {
            this.SolIn = solarRadiation;
            this.SolH = solH;
            this.tickTime = tickTime;
            this.beginDay = beginDay;
            this.days = days;
            Init();
        }

        /// <summary>
        public void Init()
        {
            data = new double[(int)(3600 / tickTime) * 24 * days];

            // デルタt
            double dt = tickTime;

            int off = 0;
            for (int d = 0; d < days; d++)
            {
                for (int i = 0; i < 24; i++)
                {
                    for (int j = 0; j < (int)(3600 / dt); j++, off++)
                    {
                        var H = SolIn.Get(off);
                        data[off] = GetHd(H, beginDay + d, off);
                    }
                }
            }
        }

        private double GetH0(int d, int t)
        {
            var h = SolH.Get(t);
            var omega = 2.0 * Math.PI / 365;
            var J = (double) d + 0.5;
            var r = 1.0;
            var r_dash = Math.Pow(1.00011 + 0.034221 * Math.Cos(omega * J) + 0.001280 * Math.Sin(omega * J)
                 + 0.000719 * Math.Cos(2.0 * omega * J) + 0.000077 * Math.Sin(2.0 * omega * J), 0.5);
            var distES = r / r_dash;
            var H0 = Math.Max(0, 1.367 / (distES * distES) * Math.Sin(h * Math.PI / 180)) * 3.6;
            return H0;
        }

        private double GetClearSkyIndex(double H, int d, int t)
        {
            var h = SolH.Get(t);
            var H0 = GetH0(d, t) * 3.6;
            if (H0 <= 0)
            {
                return 0;
            }
            else
            {
                return H / H0;
            }
        }

        private double GetHd(double H, int d, int t)
        {
            var CSI = GetClearSkyIndex(H, d, t);
            var Hd = 0.0;

            if (CSI < 0.22)
            {
                Hd = (1.0 - 0.99 * CSI) * H;
            }
            else if (CSI <= 0.80)
            {
                double CSI2 = CSI * CSI;
                double CSI3 = CSI2 * CSI;
                double CSI4 = CSI2 * CSI2;
                Hd = (0.9511 - 0.1604 * CSI + 4.388 * CSI2 - 16.638 * CSI3 + 12.366 * CSI4) * H;
            }
            else
            {
                Hd = 0.165 * H;
            }

            return Hd;
        }


    }
}
