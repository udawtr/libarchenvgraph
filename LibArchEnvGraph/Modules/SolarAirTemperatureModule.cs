using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Modules
{
    /// <summary>
    /// 吸収日射を計算するモジュール
    /// 
    /// 外気温度,全天日射量,太陽高度角および方位角の入力を受け付け、相当外気温度を計算します。
    /// 
    ///              +---------------+
    ///              |               |
    ///    TempIn -->+               |
    ///              |               +--> SolTiltOut
    ///    SolHIn -->+               |
    ///              | 相当外気温度M +--> RadNightOut
    ///    SolAIn -->+               |
    ///              |               +--> TempOut
    ///     SolIn -->+               |
    ///              |               |
    ///              +---------------+
    /// 入力:
    /// - 外気温度 TempIn [K]
    /// - 太陽高度角 SolH [deg]
    /// - 太陽方位角 SolA [deg]
    /// - 日射量     SolIn [W/m2]
    /// 
    /// 出力:
    /// - 傾斜日射量 SolTiltOut [W/m2]
    /// - 夜間放射量 RadNightOut [W/m2]
    /// - 相当外気温度 TempOut [K]
    /// </summary>
    /// <seealso cref="Functions.Brunt"/>
    /// <seealso cref="SolarTransmissionModule"/>
    public class SolarAirTemperatureModule : BaseModule
    {
        /// <summary>
        /// 傾斜角 [deg]
        /// </summary>
        public double TiltAngle { get; set; }

        /// <summary>
        /// 方位角 [deg]
        /// </summary>
        public double AzimuthAngle { get; set; }

        public double GroundReturnRate { get; set; }

        /// <summary>
        /// 太陽高度角 [deg]
        /// </summary>
        public IVariable<double> SolHIn { get; set; }

        /// <summary>
        /// 太陽傾斜角 [deg]
        /// </summary>
        public IVariable<double> SolAIn { get; set; }

        /// <summary>
        /// 日射 [W/m2]
        /// </summary>
        public IVariable<double> SolIn { get; set; }

        /// <summary>
        /// 外気温 [K]
        /// </summary>
        public IVariable<double> TempIn { get; set; }

        /// <summary>
        /// 傾斜日射 [W/m2]
        /// </summary>
        public IVariable<double> SolTiltOut { get; private set; } = new LinkVariable<double>();

        /// <summary>
        /// 夜間放射(実効放射) [W/m2]
        /// </summary>
        public IVariable<double> RadNightOut { get; private set; } = new LinkVariable<double>();

        /// <summary>
        /// 外気相当温度 [K]
        /// </summary>
        public IVariable<double> TempOut { get; private set; } = new LinkVariable<double>();

        public int TickSecond, BeginDay, TotalDays;


        public override void Init(FunctionFactory F)
        {
            if (SolHIn == null) throw new InvalidOperationException("太陽高度角を設定してから初期化してください。");
            if (SolAIn == null) throw new InvalidOperationException("太陽方位角を設定してから初期化してください。");
            if (SolIn == null) throw new InvalidOperationException("日射量を設定してから初期化してください。");
            if (TempIn == null) throw new InvalidOperationException("外気温を設定してから初期化してください。");

            //入射角の方向余弦
            var tiltCos = F.IncidentAngleCosine(TiltAngle, AzimuthAngle, SolHIn, SolAIn);

            //直散分離
            var solDirect = F.DirectSolarRadiation(TickSecond, BeginDay, TotalDays, SolIn, SolHIn);
            var solDiffuse = F.Subtract(SolIn, solDirect);

            //傾斜面直達日射量
            var J_dt = F.TiltDirectSolarRadiation(tiltCos, solDirect);

            //天空日射(傾斜)
            var J_st = F.TiltDiffusedSolarRadiation(1, GroundReturnRate, SolHIn, solDirect, solDiffuse);

            //反射日射
            var J_h = F.Add(solDirect, solDiffuse);
            var J_rt = F.Function(t => (1.0 - (1.0 + tiltCos.Get(t)) / 2) * 0.25 * J_h.Get(t));

            //日射(傾斜)
            var J_t = F.Concat(J_dt, J_st, J_rt);

            //実効放射
            var J_e = F.Brunt(TiltAngle * Math.PI / 180, TempIn, F.Variable(4.28), F.Variable(0.8), F.Variable(1));
            var SAT = F.SAT(To: TempIn, J: J_t, J_e: J_e);

            //リンク設定
            (SolTiltOut as LinkVariable<double>).Link = J_t;
            (RadNightOut as LinkVariable<double>).Link = J_e;
            (TempOut as LinkVariable<double>).Link = SAT;

            //ラベル
            SolTiltOut.Label = $"{Label} - 傾斜日射量[W/m2]";
            RadNightOut.Label = $"{Label} - 夜間放射[W/m2]";
            TempOut.Label = $"{Label} - 外気相当温度[K]";

            base.Init(F);
        }
    }
}
