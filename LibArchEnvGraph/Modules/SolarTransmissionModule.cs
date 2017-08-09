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
    ///              +-----------+
    ///              |           |
    ///    SolAIn -->+           |
    ///              |           |
    ///    SolHIn -->+           +--> HeatOut
    ///              |           |
    ///     SolIn -->+           |
    ///              |           |
    ///              +-----------+
    /// 入力:
    /// - 太陽高度角 SolH [deg]
    /// - 太陽方位角 SolA [deg]
    /// - 日射量     SolIn [W/m2]
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
        /// コンストラクタ
        /// </summary>
        /// <param name="area">開口部面積[m2]</param>
        /// <param name="tiltAngle">傾斜角 [°]</param>
        /// <param name="azimuthAngle">方位角 [°]</param>
        /// <param name="groundReturnRate">地面日射反射率 [-]</param>
        /// <param name="solarThroughRate">垂直入射時の日射透過率 [-]</param>
        /// <param name="solPos">太陽高度と方位角</param>
        /// <param name="sol">日射量</param>
        public SolarTransmissionModule(
            double area,
            double tiltAngle,
            double azimuthAngle,
            double groundReturnRate,
            double solarThroughRate,
            IVariable<double> sol,
            IVariable<double> solH,
            IVariable<double> solA,
            int tickTime,
            int beginDay,
            int days
            )
        {
            // 度 [°] をラジアン [rad] に変換する
            const double toRad = Math.PI / 180.0;

            var F = new FunctionFactory();

            var tiltAngleCos = Math.Cos(tiltAngle * toRad);

            //パラメータ保存
            this.SolIn = sol;
            this.SolAIn = solA;
            this.SolHIn = solH;

            //入射角の方向余弦
            var tiltCos = F.IncidentAngleCosine(tiltAngle, azimuthAngle, solH, solA);

            //直散分離
            var solDirect = F.DirectSolarRadiation(tickTime, beginDay, days, sol, solH);
            var solDiffuse = F.Subtract(sol, solDirect);

            //傾斜面直達日射量
            var solDirectTilt = F.TiltDirectSolarRadiation(tiltCos, solDirect);

            //傾斜面拡散日射量
            var shapeFactorToSky = (1.0 + tiltAngleCos) / 2.0;
            var solDiffuseTile = F.TiltDiffusedSolarRadiation(shapeFactorToSky, groundReturnRate, solH, solDirect, solDiffuse);

            //傾斜面全天日射量
            //var solTiltOut = F.Add(solDirectTilt, solDiffuseTile);

            //透過日射熱 [W]
            var solTran = F.ThroughSolar(area, solarThroughRate, tiltCos, solDirectTilt, solDiffuseTile);

            //最終出力
            (HeatOut as LinkVariable<double>).Link = solTran;
        }
    }
}
