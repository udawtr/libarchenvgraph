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
    public interface ISolarRadiationData
    {
        /// <summary>
        /// 外気温度 To_n　[℃]
        /// </summary>
        double OutsideTemperature { get; }

        /// <summary>
        /// 法線面直達日射量 IDN_n [W/m2]
        /// </summary>
        double DirectSolarRadiation { get; }

        /// <summary>
        /// 水平面天空日射量 Isky_n [W/m2]
        /// </summary>
        double SkySolarRadiation { get; }

        /// <summary>
        /// 夜間放射量 RN_n [W/m2]
        /// </summary>
        double NocturnalRadiation { get; }

        /// <summary>
        /// 絶対湿度 [g/kg']
        /// </summary>
        double AbsoluteHumidity { get; }
    }
}
