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
    /// 入力:
    /// - 太陽高度と方位角 SolarPosition
    /// - 日射量 SolarRadiation [W/m2]
    /// 
    /// 出力:
    /// - 傾斜日射 TiltSolarRadiation []
    /// - 入射角の方向余弦 DirectionCosine [rad]
    /// - 透過日射熱取得量 HeatOut [W]
    /// </summary>
    public class SolarTransmissionModule : BaseModule
    {
        #region 入力

        /// <summary>
        /// 太陽高度と方位角
        /// </summary>
        public IVariable<ISolarPositionData> SolarPosition { get; private set; }

        /// <summary>
        /// 日射量
        /// </summary>
        public IVariable<double> SolarRadiation { get; private set; }

        #endregion

        #region 途中出力

        /// <summary>
        /// 傾斜日射
        /// </summary>
        public SolarRadiationTilterModule TiltSolarRadiation { get; private set; }

        /// <summary>
        /// 入射角の方向余弦
        /// </summary>
        public IVariable<double> DirectionCosine { get; private set; }

        public override void Init(FunctionFactory F)
        {
            TiltSolarRadiation.Init(F);
        }

        #endregion

        #region 最終出力

        /// <summary>
        /// 透過日射熱取得量 QGT [W]
        /// </summary>
        public IVariable<double> HeatOut { get; private set; } = new LinkVariable<double>();

        #endregion



        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="area">開口部面積[m2]</param>
        /// <param name="tiltAngle">傾斜角 [°]</param>
        /// <param name="azimuthAngle">方位角 [°]</param>
        /// <param name="groundReturnRate">地面日射反射率 [-]</param>
        /// <param name="solarThroughRate">垂直入射時の日射透過率 [-]</param>
        /// <param name="solarPosition">太陽高度と方位角</param>
        /// <param name="solarRadiation">日射量</param>
        public SolarTransmissionModule(
            double area,
            double tiltAngle,
            double azimuthAngle,
            double groundReturnRate,
            double solarThroughRate,
            IVariable<ISolarPositionData> solarPosition,
            IVariable<double> solarRadiation,
            int tickTime,
            int beginDay,
            int days
            )
        {
            // 度 [°] をラジアン [rad] に変換する
            const double toRad = Math.PI / 180.0;

            var F = new FunctionFactory();

            var tiltAngleCos = Math.Cos(tiltAngle * toRad);

            //太陽位置
            this.SolarPosition = solarPosition;

            //気象データ
            this.SolarRadiation = solarRadiation;

            //入射角の方向余弦
            this.DirectionCosine = new IncidentAngleCosine(
                tiltAngle: tiltAngle,
                azimuthAngle: azimuthAngle,
                solarPosition: solarPosition
            );

            //直達日射
            var directSolarRadiation = F.DirectSolarRadiation(tickTime, beginDay, days, solarRadiation, solarPosition);
            var diffusedSolarRadiation = F.Subtract(solarRadiation, directSolarRadiation);

            //傾斜日射
            this.TiltSolarRadiation = new SolarRadiationTilterModule(
                shapeFactorToSky: (1.0 + tiltAngleCos) / 2,
                groundReturnRate: groundReturnRate,
                directSolarRadiation: directSolarRadiation,
                diffusedSolarRadiation: diffusedSolarRadiation,
                solarPosition: solarPosition,
                directionCosine: DirectionCosine
            );

            //透過日射熱 [W]
            (HeatOut as LinkVariable<double>).Link = new WindowThroughSolar(
                area: area,
                solarThroughRate: solarThroughRate,
                directionCosine: DirectionCosine,
                solarPositionSource: solarPosition,
                ID: TiltSolarRadiation.DirectOut,
                Id: TiltSolarRadiation.DiffusedOut
            );
        }
    }
}
