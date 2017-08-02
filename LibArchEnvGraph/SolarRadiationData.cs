using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph
{
    /// <summary>
    /// 日射
    /// </summary>
    public struct SolarRadiationData : ISolarRadiationData
    {
        /// <summary>
        /// 外気温度 To_n　[℃]
        /// </summary>
        public double OutsideTemperature { get; set; }

        /// <summary>
        /// 法線面直達日射量 IDN_n [W/m2]
        /// </summary>
        public double DirectSolarRadiation { get; set; }

        /// <summary>
        /// 水平面天空日射量 Isky_n [W/m2]
        /// </summary>
        public double SkySolarRadiation { get; set; }

        /// <summary>
        /// 夜間放射量 RN_n [W/m2]
        /// </summary>
        public double NocturnalRadiation { get; set; }

        /// <summary>
        /// 絶対湿度 [g/kg']
        /// </summary>
        public double AbsoluteHumidity { get; set; }
    }
}
