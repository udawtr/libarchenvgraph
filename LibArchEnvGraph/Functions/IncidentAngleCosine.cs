using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Functions
{
    /// <summary>
    /// 入射角の方向余弦
    /// </summary>
    public class IncidentAngleCosine : BaseVariable<double>
    {
        // 度 [°] をラジアン [rad] に変換する
        const double toRad = Math.PI / 180.0;

        private readonly double tiltAngle;

        private readonly double azimuthAngle;

        private readonly double tiltAngleCos;

        private IVariable<ISolarPositionData> solpossrc { get; set; }

        public IncidentAngleCosine(double tiltAngle, double azimuthAngle, IVariable<ISolarPositionData> solarPosition)
        {
            this.tiltAngle = tiltAngle;
            this.tiltAngleCos = Math.Cos(tiltAngle * Math.PI / 180.0);
            this.azimuthAngle = azimuthAngle;
            this.solpossrc = solarPosition;
        }

        public override double Update(int n)
        {
            var Ww = Math.Sin(tiltAngle * toRad) * Math.Sin(azimuthAngle * toRad);
            var Ws = Math.Sin(tiltAngle * toRad) * Math.Cos(azimuthAngle * toRad);

            ISolarPositionData solPos = solpossrc.Get(n);

            var Sh = Math.Sin(solPos.SolarElevationAngle * toRad);
            var Sw = 0.0;
            var Ss = 0.0;

            if (Sh > 0)
            {
                Sw = Math.Cos(solPos.SolarElevationAngle * toRad) * Math.Sin(solPos.SolarAzimuth * toRad);
                Ss = Math.Cos(solPos.SolarElevationAngle * toRad) * Math.Cos(solPos.SolarAzimuth * toRad);
            }

            var temp = Sh * tiltAngleCos + Sw * Ww + Ss * Ws;

            return temp > 0 ? temp : 0;
        }

    }
}
