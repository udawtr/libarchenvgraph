using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Modules
{
    /// <summary>
    /// 太陽位置の計算
    /// 
    ///           +---------+
    ///           |         |
    ///           +         +--> SolHOut
    ///           |         |
    ///           +         +--> SolAOut
    ///           |         |
    ///           +--+--+---+
    ///              |  |
    ///              L Lat
    /// </summary>
    public class SolarPositionModule : ICalculationGraph
    {
        /// <summary>
        /// 計算対象地点の緯度(10進数) [deg]
        /// </summary>
        public double Lat { get; set; } = 34.643139;

        /// <summary>
        /// 計算対象地点の経度(10進数) [deg]
        /// </summary>
        public double L { get; set; } = 134.997222;

        /// <summary>
        /// 太陽高度角 [°] hn
        /// </summary>
        public IVariable<double> SolHOut { get; private set; }

        /// <summary>
        /// 太陽方位角 [°] An
        /// </summary>
        public IVariable<double> SolAOut { get; private set; }
        
        private SolarPositionData[] data;

        public string Label { get; set; }

        public ISolarPositionData Get(int index)
        {
            return data[index];
        }

        public SolarPositionModule(double Lat, double L, int tickTime, int beginDay, int days)
        {
            this.Lat = Lat;
            this.L = L;
            Init(tickTime, beginDay, days);

            this.Label = $"太陽位置 緯度:{Lat},経度:{L}";

            SolHOut = new Variable<double>(t => data[t].SolarElevationAngle);
            SolAOut = new Variable<double>(t => data[t].SolarAzimuth);
        }

        public void Init(int tickTime, int beginDay, int days)
        {
            data = new SolarPositionData[(int)(3600 / tickTime) * 24 * days];

            var Lat_rad = Lat * Math.PI / 180;
            var L_rad = L * Math.PI / 180;

            // デルタt
            int off = 0;
            for (int d = beginDay; d < beginDay + days; d++)
            {
                for (int i = 0; i < 24; i++)
                {
                    for (int j = 0; j < (int)(3600 / tickTime); j++, off++)
                    {

                        //赤緯 δ [deg]
                        var mconDelta0 = -23.4393;  // 冬至の日赤緯
                        var sin_d0 = Math.Sin(mconDelta0 * Math.PI / 180);
                        var D = d + 1;
                        var n = 2014 - 1968;
                        var d0 = 3.71 + 0.2596 * n - ((n + 3) / 4);
                        var coeff = 360.0 / 365.2596;
                        var M = coeff * (D - d0);
                        var eps = 12.3901 + 0.0172 * (n + M / 360);
                        var v = M + 1.914 * Math.Sin(M * Math.PI / 180) + 0.02 * Math.Sin(2 * M * Math.PI / 180);
                        var Veps_rad = (v + eps) * Math.PI / 180;
                        var sin_decl = sin_d0 * Math.Cos(Veps_rad);
                        var cos_decl = Math.Sqrt(1.0 - Math.Pow(sin_decl, 2.0));

                        Func<double, double> norm = (x) => x - Math.Ceiling((x - Math.PI) / Math.PI / 2) * Math.PI * 2;

                        //均時差 Et [rad]
                        var Et1 = M - v;
                        var Et2 = Math.Atan2((0.043 * Math.Sin(2.0 * Veps_rad)), (1.0 - 0.043 * Math.Cos(2.0 * Veps_rad))) * 180 / Math.PI;
                        var Et = Et1 - Et2;

                        //標準時 Tm [h]
                        var Tm = i + ((double)j) / (3600 / tickTime);

                        //時角 [rad]
                        const double L0 = 135 * Math.PI / 180;   //明石市
                        var t = (15.0 * (Tm - 12) + Et) * Math.PI / 180 + (L_rad - L0);

                        //太陽高度 hs [rad]
                        var sin_h = Math.Sin(Lat_rad) * sin_decl + Math.Cos(Lat_rad) * cos_decl * Math.Cos(t);
                        var hs = Math.Asin(sin_h);
                        if (hs < 0.0) hs = 0.0;

                        //太陽方位角 As [rad]
                        var As = 0.0;
                        if (hs > 0)
                        {
                            var cosA = (sin_h * Math.Sin(Lat_rad) - sin_decl) / (Math.Cos(hs) * Math.Cos(Lat_rad));
                            As = Math.Sign(t) * Math.Acos(cosA);
                        }

                        data[off].SolarElevationAngle = hs * 180 / Math.PI;
                        data[off].SolarAzimuth = As * 180 / Math.PI;

                    }
                }
            }
        }

        public void Init(FunctionFactory F)
        {
        }

        public void Commit(int t)
        {
        }
    }
}
