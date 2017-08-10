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
    ///                 +-----------+
    ///                 |           |
    ///  DayOfYearIn -->|           |
    ///                 |           +--> SolHOut
    ///       HourIn -->|           |
    ///                 | 太陽位置M |
    ///     MinuteIn -->|           |
    ///                 |           +--> SolAOut
    ///     SecondIn -->|           |
    ///                 |           |
    ///                 +-----+-----+
    ///                       |
    ///                   L --+
    ///                 Lat --+
    ///                 
    /// 入力:
    /// - 計算対象地点の緯度(10進数) Lat [deg]
    /// - 計算対象地点の経度(10進数) L [deg]
    /// - 年間積算日 DayOfYearIn [日]
    /// - 時刻の時間部分 HourIn
    /// - 時刻の分部分 MinuteIn
    /// - 時刻の秒部分 SecondIn
    /// 
    /// 出力:
    /// - 太陽高度角 SolHOut [deg]
    /// - 太陽方位角 SolAOut [deg]
    /// </summary>
    public class SolarPositionModule : BaseModule
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
        public IVariable<double> SolHOut { get; private set; } = new LinkVariable<double>();

        /// <summary>
        /// 太陽方位角 [°] An
        /// </summary>
        public IVariable<double> SolAOut { get; private set; } = new LinkVariable<double>();

        /// <summary>
        /// 年間積算日[日] (1始まり最大366)
        /// </summary>
        public IVariable<int> DayOfYearIn { get; set; }

        /// <summary>
        /// 時刻の時間部分(0-23)
        /// </summary>
        public IVariable<int> HourIn { get; set; }

        /// <summary>
        /// 時刻の分部分(0-59)
        /// </summary>
        public IVariable<int> MinuteIn { get; set; }

        /// <summary>
        /// 時刻の秒部分(0-59)
        /// </summary>
        public IVariable<int> SecondIn { get; set; }

        /// <summary>
        /// 実際の計算を行うプライベートクラスへの参照
        /// </summary>
        private SolarPositionFunction _innerFunc;


        public override void Init(FunctionFactory F)
        {
            _innerFunc = new SolarPositionFunction(this);

            (SolHOut as LinkVariable<double>).Link = new Variable<double>(t => _innerFunc.Get(t).SolarElevationAngle);
            (SolAOut as LinkVariable<double>).Link = new Variable<double>(t => _innerFunc.Get(t).SolarAzimuth);

            this.Label = $"太陽位置 緯度:{Lat},経度:{L}";
        }


        private class SolarPositionFunction : BaseVariable<SolarPositionData>
        {
            private SolarPositionModule parent;

            public SolarPositionFunction(SolarPositionModule parent)
            {
                this.parent = parent;
            }

            public override SolarPositionData Update(int tick)
            {
                var Lat_rad = parent.Lat * Math.PI / 180;
                var L_rad = parent.L * Math.PI / 180;

                //赤緯 δ [deg]
                var mconDelta0 = -23.4393;  // 冬至の日赤緯
                var sin_d0 = Math.Sin(mconDelta0 * Math.PI / 180);
                var D = parent.DayOfYearIn.Get(tick);
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
                var Tm = parent.HourIn.Get(tick) + parent.MinuteIn.Get(tick) / 60.0 + parent.SecondIn.Get(tick) / 3600.0;

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

                return new SolarPositionData
                {
                    SolarElevationAngle = hs * 180 / Math.PI,
                    SolarAzimuth = As * 180 / Math.PI,
                };
            }
        }
    }
}
