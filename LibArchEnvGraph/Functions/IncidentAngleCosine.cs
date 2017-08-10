using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Functions
{
    /// <summary>
    /// 入射角の方向余弦
    /// 
    ///              +-----------+
    ///              |           |
    ///    SolPos -->+           +--> TiltCos
    ///              |           |
    ///              +-----------+
    /// </summary>
    public class IncidentAngleCosine : BaseVariable<double>
    {
        /// <summary>
        /// 傾斜角 [deg]
        /// </summary>
        public double TiltAngle { get; set; }

        /// <summary>
        /// 方位角 [deg]
        /// </summary>
        public double AzimuthAngle { get; set; }

        /// <summary>
        /// 太陽高度角 [deg]
        /// </summary>
        public IVariable<double> SolH { get; set; }

        /// <summary>
        /// 太陽方位角 [deg]
        /// </summary>
        public IVariable<double> SolA { get; set; }


        public override double Update(int n)
        {
            const double toRad = Math.PI / 180.0;

            var tiltAngleCos = Math.Cos(TiltAngle * toRad);

            var Ww = Math.Sin(TiltAngle * toRad) * Math.Sin(AzimuthAngle * toRad);
            var Ws = Math.Sin(TiltAngle * toRad) * Math.Cos(AzimuthAngle * toRad);

            var H = SolH.Get(n) * toRad;
            var A = SolA.Get(n) * toRad;

            var Sh = Math.Sin(H);
            var Sw = 0.0;
            var Ss = 0.0;

            if (Sh > 0)
            {
                Sw = Math.Cos(H) * Math.Sin(A);
                Ss = Math.Cos(H) * Math.Cos(A);
            }

            var temp = Sh * tiltAngleCos + Sw * Ww + Ss * Ws;

            return temp > 0 ? temp : 0;
        }

    }
}
