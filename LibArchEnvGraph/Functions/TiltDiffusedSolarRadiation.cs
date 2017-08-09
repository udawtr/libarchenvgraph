using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Functions
{
    /// <summary>
    /// 傾斜面拡散日射量 [W/m2]
    /// 
    ///                 +-----------------+
    ///                 |                 |
    ///     SolPosIn -->+                 |
    ///                 |                 |
    ///  SolDirectIn -->+ 傾斜面拡散日射M +--> SolDiffuseTiltOut
    ///                 |                 |
    /// SolDiffuseIn -->+                 |
    ///                 |                 |
    ///                 +---+----+--------+
    ///                     |    |
    ///                     |    +---<-- ShapeFactorToSky
    ///                     |
    ///                     +---<-- GroundReturnRate
    /// 
    /// 入力:
    /// - ShapeFactorToSky
    /// - GroundReturnRate
    /// - 太陽位置 SolPos [W/m2]
    /// - 法線面直達日射量 SolDirectIn [W/m2]
    /// - 拡散日射量 SolDiffuseIn [W/m2]
    /// </summary>
    /// <remarks>
    /// 傾斜面拡散日射量 = 傾斜面天空日射量 + 地物反射日射
    /// </remarks>
    /// <seealso cref="DirectSolarRadiation"/>
    public class TiltDiffusedSolarRadiation : BaseVariable<double>
    {
        const double toRad = Math.PI / 180.0;

        public double ShapeFactorToSky { get; set; }

        public double ShapeFactorToGround { get { return 1.0 - ShapeFactorToSky; } }

        public double GroundReturnRate { get; set; }

        /// <summary>
        /// 太陽高度[deg]
        /// </summary>
        public IVariable<double> SolHIn { get; set; }

        /// <summary>
        /// 法線面直達日射量 [W/m2]
        /// </summary>
        public IVariable<double> SolDirectIn { get; set; }

        /// <summary>
        /// 拡散日射量 [W/m2]
        /// </summary>
        public IVariable<double> SolDiffuseIn { get; set; }

        public override double Update(int t)
        {
            return GetSkySolarRadiation(t) + GetGroundReflectedSolarRadiation(t);
        }


        /// <summary>
        /// 傾斜面天空日射量 IS [W/m2] の取得
        /// </summary>
        /// <returns>IS_ikn</returns>
        private double GetSkySolarRadiation(int n)
        {
            double I_sky = SolDiffuseIn.Get(n);

            return ShapeFactorToSky * I_sky;
        }

        /// <summary>
        /// 地物反射日射 IR [W/m2] の取得
        /// </summary>
        private double GetGroundReflectedSolarRadiation(int n)
        {
            double IDN = SolDirectIn.Get(n);   //法線面直達日射量 [W/m2]
            double Shn = Math.Sin(SolHIn.Get(n) * toRad);     //太陽高度角
            double I_sky = SolDiffuseIn.Get(n);    //水平面天空日射量 [W/m2]

            return GroundReturnRate * ShapeFactorToGround * (IDN * Shn + I_sky); // P.11 (45)
        }
    }
}
