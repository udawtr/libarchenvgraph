using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph
{
    public struct SolarPositionData : ISolarPositionData
    {
        public double SolarAzimuth { get; set; }

        public double SolarElevationAngle { get; set; }
    }
}
