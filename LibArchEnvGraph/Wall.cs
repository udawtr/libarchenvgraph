using LibArchEnvGraph.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph
{
    public class Wall
    {
        /// <summary>
        /// 熱伝導率 [W/mK]
        /// </summary>
        public double Rambda { get; set; }

        public double cro { get; set; }

        public double depth { get; set; }

        public double S { get; set; }

        public bool IsFloor { get; set; }

        public bool IsCeiling { get; set; }

        public double TiltAngle { get; set; }

        public double AzimuthAngle { get; set; }

        public double GroundReturnRate { get; set; }

        public double SolarThroughRate { get; set; }

        /// <summary>
        /// 開口部
        /// </summary>
        public bool IsOpen { get; set; }
    }
}
