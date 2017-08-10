using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph
{
    /// <summary>
    /// 関数工場
    /// </summary>
    public class FunctionFactory
    {
        private static FunctionFactory _singleton = null;

        public static FunctionFactory Default
        {
            get
            {
                if (_singleton == null) _singleton = new FunctionFactory();
                return _singleton;
            }
        }


        #region 基本演算

        /// <summary>
        /// 加算
        /// </summary>
        public virtual IVariable<double> Add(IVariable<double> a, IVariable<double> b)
        {
            return new Functions.Add(a, b);
        }

        /// <summary>
        /// 多変数加算
        /// </summary>
        public virtual IVariable<double> Concat(IEnumerable<IVariable<double>> varin)
        {
            return new Functions.Concat(varin);
        }

        /// <summary>
        /// 多変数加算
        /// </summary>
        public virtual IVariable<double> Concat(params IVariable<double>[] varin)
        {
            return new Functions.Concat(varin);
        }

        /// <summary>
        /// 減算
        /// </summary>
        public virtual IVariable<double> Subtract(IVariable<double> a, IVariable<double> b)
        {
            return new Functions.Subtract(a, b);
        }

        /// <summary>
        /// 符号反転
        /// </summary>
        public virtual IVariable<double> Invert(IVariable<double> var_in)
        {
            return new Functions.Invert(var_in);
        }


        /// <summary>
        /// 乗算
        /// </summary>
        public virtual IVariable<double> Multiply(IVariable<double> a, IVariable<double> b)
        {
            return new Functions.Multiply(a, b);
        }

        /// <summary>
        /// 乗算
        /// </summary>
        public virtual IVariable<double> Multiply(double a, IVariable<double> b)
        {
            return new Functions.Multiply(new Variable<double>(a), b);
        }

        #endregion

        #region ユーティリティ

        public virtual IVariable<double> Variable(double c)
        {
            return new Variable<double>(c);
        }

        public virtual IVariable<double> Function(Func<int,double> func)
        {
            return new Variable<double>(func);
        }

        /// <summary>
        /// ケルビン to 摂氏
        /// </summary>
        public virtual IVariable<double> KelvinToCelsius(IVariable<double> T)
        {
            return new Functions.KelvinToCelsius
            {
                Temp = T
            };
        }

        /// <summary>
        /// データ補完
        /// </summary>
        public virtual IVariable<double> Interpolate(double[] original, int scaleFactor)
        {
            return new Functions.DataInterpolator(original, scaleFactor);
        }

        /// <summary>
        /// 記憶
        /// </summary>
        public virtual IGateVariable<double> Memory(IVariable<double> dataIn)
        {
            return new Functions.Memory
            {
                DataIn = dataIn
            };
        }

        #endregion

        #region 熱量と温度

        /// <summary>
        /// 温度 [K]
        /// </summary>
        public virtual IVariable<double> Temperature(double cro, double V, IVariable<double> heat)
        {
            return new Functions.HeatToTemp
            {
                cro = cro,
                V = V,
                Heat = heat
            };
        }


        /// <summary>
        /// 換気熱移動量
        /// </summary>
        /// <param name="cro">流体(空気)容積比熱 [kJ/m3K]</param>
        /// <param name="T1">固体(壁体)の表面温度 [K]</param>
        /// <param name="T2">流体(空気)の温度 [K]</param>
        /// <param name="V">流体(空気)の容積 [m3]</param>
        public virtual IVariable<double> VentilationHeatTransfer(double V, IVariable<double> T1, IVariable<double> T2, double cro = 1.007 * 1.024)
        {
            return new Functions.VentilationHeatTransfer
            {
                cro = cro,
                T1 = T1,
                T2 = T2,
                V = V,
            };
        }

        #endregion

        #region 熱流

        /// <summary>
        /// 熱伝導による熱移動の計算 (フーリエの法則)
        /// </summary>
        /// <param name="depth">固体(壁体)の厚み [m]</param>
        /// <param name="lambda">熱伝導率 [W/mK]</param>
        /// <param name="S">熱流の通過面積 [m2]</param>
        /// <param name="T1">温度1 [K]</param>
        /// <param name="T2">温度2 [K]</param>
        public virtual IVariable<double> Fourier(double depth, double lambda, double S, IVariable<double> T1, IVariable<double> T2)
        {
            return new Functions.Fourier
            {
                Depth = depth,
                Lambda = lambda,
                S = S,
                T1 = T1,
                T2 = T2,
            };
        }

        /// <summary>
        /// 貫流熱の計算(定常熱計算)
        /// </summary>
        /// <returns>面積Sの貫流熱量[]</returns>
        public virtual IVariable<double> OverallHeatTransmission(IVariable<double> T1, IVariable<double> T2, double K, double S)
        {
            return new Functions.OverallHeatTransmission
            {
                K = K,
                S = S,
                T1 = T1,
                T2 = T2
            };
        }

        /// <summary>
        /// 対流による熱伝達の計算 (ニュートンの冷却則)
        /// </summary>
        public virtual IVariable<double> NewtonCooling(double S, IVariable<double> T1, IVariable<double> T2, IVariable<double> alpha_c)
        {
            return new Functions.NewtonCooling
            {
                alpha_c = alpha_c,
                S = S,
                T1 = T1,
                T2 = T2
            };
        }

        /// <summary>
        /// 放射による熱移動の計算 (シュテファンボルツマン)
        /// </summary>
        public virtual IVariable<double> StefanBolzmann(double F12, IVariable<double> T1, IVariable<double> T2, double e1 = 0.9, double e2 = 0.9)
        {
            return new Functions.StefanBolzmann
            {
                e1 = e1,
                e2 = e2,
                F12 = F12,
                T1 = T1,
                T2 = T2
            };
        }

        /// <summary>
        /// SAT温度(相当外気温度)
        /// </summary>
        /// <param name="To">外気温度 [K]</param>
        /// <param name="J">面に入射する日射量 [W/m2]</param>
        /// <param name="J_e">面の実効放射量 [W/m2]</param>
        /// <param name="a_s">面の日射(短波)吸収率 [-]</param>
        /// <param name="eps">面の長波長吸収率 [-]</param>
        public virtual IVariable<double> SAT(IVariable<double> To, IVariable<double> J, IVariable<double> J_e, double a_s = 0.7, double eps = 0.9, double alpha_o = 23.0)
        {
            return new Variable<double>(t => To.Get(t) + (a_s * J.Get(t) - eps * J_e.Get(t)) / alpha_o);
        }

        /// <summary>
        /// 無限並行2平面間の放射熱交換量(オッペンハイムのネットワーク手法) [W]
        /// </summary>
        public virtual IVariable<double> Oppenheim(IVariable<double> T1, IVariable<double> T2, double S = 1.0, double e1 = 0.9, double e2 = 0.9)
        {
            //並行2平面間の有効放射率 [-]
            var e12 = 1.0 / (1.0/e1 + 1.0/e2 - 1);
            return new Variable<double>(t => e12 * 5.67 * (Math.Pow(T1.Get(t) / 100, 4) - Math.Pow(T2.Get(t) / 100, 4)) * S);
        }

        #endregion

        #region 日射計算

        /// <summary>
        /// 直散分離(全天日射から直達日射を分離)
        /// </summary>
        /// <param name="sol">日射量 [W/m2]</param>
        /// <param name="solH">太陽高度角 [deg]</param>
        /// <param name="dayOfYear">年間積算日(1-366)</param>
        public virtual IVariable<double> DirectSolarRadiation(IVariable<double> sol, IVariable<double> solH, IVariable<int> dayOfYear)
        {
            if (sol == null) throw new InvalidOperationException("日射量を指定してください。");
            if (solH == null) throw new InvalidOperationException("太陽高度角を指定してください。");

            return new Functions.DirectSolarRadiation
            {
                SolIn = sol,
                SolH = solH,
                DayOfYearIn = dayOfYear,
            };
        }

        /// <summary>
        /// 入射角の方向余弦
        /// </summary>
        /// <param name="tiltAngle">傾斜角 [deg]</param>
        /// <param name="azimuthAngle">方位角 [deg]</param>
        /// <param name="solH">太陽高度角 [deg]</param>
        /// <param name="solA">太陽方位角 [deg]</param>
        public virtual IVariable<double> IncidentAngleCosine(double tiltAngle, double azimuthAngle, IVariable<double> solH, IVariable<double> solA)
        {
            if (solH == null) throw new InvalidOperationException("太陽高度角を指定してください。");
            if (solA == null) throw new InvalidOperationException("太陽方位角を指定してください。");

            return new Functions.IncidentAngleCosine
            {
                TiltAngle = tiltAngle,
                AzimuthAngle = azimuthAngle,
                SolH = solH,
                SolA = solA,
            };
        }

        /// <summary>
        /// 傾斜面直達日射量
        /// </summary>
        public virtual  IVariable<double> TiltDirectSolarRadiation(IVariable<double> cos, IVariable<double> ID)
        {
            return new Functions.Multiply(cos, ID);
        }

        /// <summary>
        /// 傾斜面拡散日射量
        /// </summary>
        public virtual IVariable<double> TiltDiffusedSolarRadiation(double shapeFactorToSky, double groundReturnRate, IVariable<double> solH, IVariable<double> ID, IVariable<double> Id)
        {
            return new Functions.TiltDiffusedSolarRadiation
            {
                ShapeFactorToSky = shapeFactorToSky,
                GroundReturnRate = groundReturnRate,
                SolHIn = solH,
                SolDirectIn = ID,
                SolDiffuseIn = Id
            };
        }

        /// <summary>
        /// 透過日射の計算
        /// </summary>
        /// <param name="S">透過面積 [m2]</param>
        /// <param name="solarThroughRate">垂直入射時の日射透過率 [-]</param>
        /// <param name="tiltCos">入射角の方向余弦</param>
        /// <param name="solDirectTilt">傾斜面直達日射量 [W/m2]</param>
        /// <param name="solDiffuseTilt">傾斜面拡散日射量 [W/m2]</param>
        public virtual IVariable<double> ThroughSolar(double S, double solarThroughRate, IVariable<double> tiltCos, IVariable<double> solDirectTilt, IVariable<double> solDiffuseTilt)
        {
            if (S <= 0) throw new InvalidOperationException("面積は正の値を指定してください。");
            if (!(0 <= solarThroughRate && solarThroughRate <= 1.0)) throw new InvalidOperationException("垂直入射時の日射透過率は0.0から1.0の間で指定してください。");
            if (tiltCos == null) throw new InvalidOperationException("入射角の方向余弦を指定してください。");
            if (solDirectTilt == null) throw new InvalidOperationException("傾斜面直達日射量を指定してください。");
            if (solDiffuseTilt == null) throw new InvalidOperationException("傾斜面拡散日射量を指定してください。");

            return new Functions.WindowThroughSolar
            {
                S = S,
                SolarThroughRate = solarThroughRate,
                TiltCos = tiltCos,
                SolDirectTilt = solDirectTilt,
                SolDiffuseTile = solDiffuseTilt
            };
        }

        /// <summary>
        /// 透過日射の分配
        /// </summary>
        /// <param name="fsol">透過日射分配率 [-]</param>
        public virtual IVariable<double> Split(IVariable<double> Q, double S, double fsol)
        {
            if (Q == null) throw new InvalidOperationException("透過日射量を指定してください。");
            if (S <= 0) throw new InvalidOperationException("面積は正の値を指定してください。");
            if (!(0 <= fsol && fsol <= 1.0)) throw new InvalidOperationException("透過日射分配率は0.0から1.0の間で指定してください。");

            return new Functions.Split
            {
                HeatIn = Q,
                S = S,
                fsol = fsol
            };
        }

        /// <summary>
        /// ブラントの式による実効放射量[W/m2]の取得
        /// </summary>
        /// <param name="theta">大気放射到達面の傾斜角 [rad] (鉛直=Math.PI/2, 水平=0)</param>
        /// <param name="Ta">地表付近の空気の絶対温度 [K]</param>
        /// <param name="f">地表付近の空気の水蒸気分圧 [mmHg]</param>
        /// <param name="k">雲高によって決まる修正定数(上層雲時: 0.8, 中層雲時: 0.3, 下層雲時: 0.15)</param>
        /// <param name="c">雲量(快晴:0 ～ 全天雲:10)</param>
        /// <returns>実効放射量[W/m2]</returns>
        public virtual IVariable<double> Brunt(double theta, IVariable<double> Ta, IVariable<double> f, IVariable<double> k, IVariable<double> c)
        {
            if (Ta == null) throw new InvalidOperationException("地表付近の空気の絶対温度を指定してください。");
            if (f == null) throw new InvalidOperationException("地表付近の空気の水蒸気分圧を指定してください。");
            if (k == null) throw new InvalidOperationException("雲高によって決まる修正定数を指定してください。");
            if (c == null) throw new InvalidOperationException("雲量を指定してください。");

            return new Functions.Brunt
            {
                theta = theta,
                Ta = Ta,
                f = f,
                k = k,
                c = c
            };
        }

        #endregion
    }
}
