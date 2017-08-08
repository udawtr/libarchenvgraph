using LibArchEnvGraph.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph
{
    /// <summary>
    /// 外壁
    /// </summary>
    public class Wall
    {
        /// <summary>
        /// 熱伝導率 [W/mK]
        /// </summary>
        public double Rambda { get; set; }

        /// <summary>
        /// 比熱 (壁体内を非定常計算する場合は必要)
        /// </summary>
        public double cro { get; set; }

        /// <summary>
        /// 奥行 [m]
        /// </summary>
        public double depth { get; set; }

        /// <summary>
        /// 面積 [m2]
        /// </summary>
        public double S { get; set; }

        public bool IsFloor { get; set; }

        public bool IsCeiling { get; set; }

        /// <summary>
        /// 傾斜角
        /// </summary>
        public double TiltAngle { get; set; }

        /// <summary>
        /// 方位角
        /// </summary>
        public double AzimuthAngle { get; set; }

        /// <summary>
        /// 地面日射反射率
        /// </summary>
        public double GroundReturnRate { get; set; }

        /// <summary>
        /// 透過率 [-]
        /// </summary>
        public double SolarThroughRate { get; set; }

        /// <summary>
        /// 開口部
        /// </summary>
        public bool IsOpen { get; set; }
    }
}
