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
        // 度 [°] をラジアン [rad] に変換する
        const double toRad = Math.PI / 180.0;

        private readonly double tiltAngle;

        private readonly double azimuthAngle;

        private readonly double tiltAngleCos;

        public IVariable<double> SolH { get; set; }

        public IVariable<double> SolA { get; set; }

        public IncidentAngleCosine(double tiltAngle, double azimuthAngle, IVariable<double> solH, IVariable<double> solA)
        {
            this.tiltAngle = tiltAngle;
            this.tiltAngleCos = Math.Cos(tiltAngle * Math.PI / 180.0);
            this.azimuthAngle = azimuthAngle;
            this.SolH = solH;
            this.SolA = solA;
        }

        public override double Update(int n)
        {
            var Ww = Math.Sin(tiltAngle * toRad) * Math.Sin(azimuthAngle * toRad);
            var Ws = Math.Sin(tiltAngle * toRad) * Math.Cos(azimuthAngle * toRad);

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
