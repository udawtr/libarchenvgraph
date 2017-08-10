using LibArchEnvGraph.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Modules
{
    /// <summary>
    /// 透過日射を計算するモジュール
    /// 
    /// 全天日射量,太陽高度角および方位角の入力を受け付け、透過日射熱取得量を計算します。
    /// 
    ///                 +-----------+
    ///                 |           |
    ///       SolHIn -->+           |
    ///                 |           |
    ///       SolAIn -->+           |
    ///                 | 透過日射M +--> HeatOut
    ///        SolIn -->+           |
    ///                 |           |
    ///  DayOfYearIn -->+           |
    ///                 |           |
    ///                 +-----+-----+
    ///                       |
    ///                   S --+
    ///           TiltAngle --+
    ///        AzimuthAngle --+
    ///    GroundReturnRate --+
    ///    SolarThroughRate --+
    /// 
    /// 入力:
    /// - 太陽高度角 SolH [deg]
    /// - 太陽方位角 SolA [deg]
    /// - 日射量     SolIn [W/m2]
    /// - 年間積算日 DayOfYearIn [日]
    /// - 開口部面積 S [m2]
    /// - 傾斜角     TiltAngle [deg]
    /// - 方位角     AzimuthAngle [deg]
    /// - 地面日射反射率 GroundReturnRate [-]
    /// - 垂直入射時の日射透過率 SolarThroughRate [-]
    /// 
    /// 出力:
    /// - 透過日射熱取得量 HeatOut [W]
    /// </summary>
    public class SolarTransmissionModule : BaseModule
    {
        /// <summary>
        /// 太陽高度
        /// </summary>
        public IVariable<double> SolHIn { get; set; }

        /// <summary>
        /// 方位角
        /// </summary>
        public IVariable<double> SolAIn { get; set; }

        /// <summary>
        /// 日射量
        /// </summary>
        public IVariable<double> SolIn { get; set; }

        /// <summary>
        /// 透過日射熱取得量 [W]
        /// </summary>
        public IVariable<double> HeatOut { get; private set; } = new LinkVariable<double>();

        /// <summary>
        /// 年間積算日(1-366)
        /// </summary>
        public IVariable<int> DayOfYearIn { get; set; }

        /// <summary>
        /// 開口部面積[m2]
        /// </summary>
        public double S { get; set; }

        /// <summary>
        /// 傾斜角 [deg]
        /// </summary>
        public double TiltAngle { get; set; }

        /// <summary>
        /// 方位角 [deg]
        /// </summary>
        public double AzimuthAngle { get; set; }

        /// <summary>
        /// 地面日射反射率 [-]
        /// </summary>
        public double GroundReturnRate { get; set; }

        /// <summary>
        /// 垂直入射時の日射透過率 [-]
        /// </summary>
        public double SolarThroughRate { get; set; }

        public SolarTransmissionModule()
        {
            Label = "透過日射M";
        }

        public override void Init(FunctionFactory F)
        {
            // 度 [°] をラジアン [rad] に変換する
            const double toRad = Math.PI / 180.0;

            var tiltAngleCos = Math.Cos(TiltAngle * toRad);

            //入射角の方向余弦
            var tiltCos = F.IncidentAngleCosine(TiltAngle, AzimuthAngle, SolHIn, SolAIn);

            //直散分離
            var solDirect = F.DirectSolarRadiation(SolIn, SolHIn, DayOfYearIn);
            var solDiffuse = F.Subtract(SolIn, solDirect);

            //傾斜面直達日射量
            var solDirectTilt = F.TiltDirectSolarRadiation(tiltCos, solDirect);

            //傾斜面拡散日射量
            var shapeFactorToSky = (1.0 + tiltAngleCos) / 2.0;
            var solDiffuseTilt = F.TiltDiffusedSolarRadiation(shapeFactorToSky, GroundReturnRate, SolHIn, solDirect, solDiffuse);

            //傾斜面全天日射量
            //var solTiltOut = F.Add(solDirectTilt, solDiffuseTile);

            //透過日射熱 [W]
            var solTran = F.ThroughSolar(S, SolarThroughRate, tiltCos, solDirectTilt, solDiffuseTilt);

            //最終出力
            (HeatOut as LinkVariable<double>).Link = solTran;
        }
    }
}
