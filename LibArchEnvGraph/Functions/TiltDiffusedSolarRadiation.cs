using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Functions
{
    public class TiltDiffusedSolarRadiation : IVariable<double>
    {
        const double toRad = Math.PI / 180.0;

        public double ShapeFactorToSky { get; set; }

        public double ShapeFactorToGround { get { return 1.0 - ShapeFactorToSky; } }

        public double GroundReturnRate { get; set; }

        /// <summary>
        /// 太陽位置
        /// </summary>
        public IVariable<ISolarPositionData> SolarPosition { get; set; }

        /// <summary>
        /// 法線面直達日射量 [W/m2]
        /// </summary>
        public IVariable<double> DirectSolarRadiation { get; set; }

        /// <summary>
        /// 拡散日射量 [W/m2]
        /// </summary>
        public IVariable<double> DiffusedSolarRadiation { get; set; }

        public double Get(int t)
        {
            return GetSkySolarRadiation(t) + GetGroundReflectedSolarRadiation(t);
        }


        /// <summary>
        /// 傾斜面天空日射量 IS [W/m2] の取得
        /// </summary>
        /// <returns>IS_ikn</returns>
        private double GetSkySolarRadiation(int n)
        {
            double I_sky = DiffusedSolarRadiation.Get(n);

            return ShapeFactorToSky * I_sky;
        }

        /// <summary>
        /// 地物反射日射 IR [W/m2] の取得
        /// </summary>
        private double GetGroundReflectedSolarRadiation(int n)
        {
            double IDN = DirectSolarRadiation.Get(n);   //法線面直達日射量 [W/m2]
            double Shn = Math.Sin(SolarPosition.Get(n).SolarElevationAngle * toRad);     //太陽高度角
            double I_sky = DiffusedSolarRadiation.Get(n);    //水平面天空日射量 [W/m2]

            return GroundReturnRate * ShapeFactorToGround * (IDN * Shn + I_sky); // P.11 (45)
        }
    }
}
