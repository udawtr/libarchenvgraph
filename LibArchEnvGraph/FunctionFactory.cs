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

        #endregion

        #region 熱量と温度

        /// <summary>
        /// 熱量
        /// </summary>
        public virtual IGateVariable<double> HeatMemory(IVariable<double> heatIn)
        {
            return new Functions.HeatMemory
            {
                HeatIn = heatIn
            };
        }

        /// <summary>
        /// 温度
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
        public virtual IVariable<double> VentilationHeatTransfer(double V, IVariable<double> Ts, IVariable<double> Tf, double c_air = 1.007, double ro_air = 1.024)
        {
            return new Functions.VentilationHeatTransfer
            {
                c_air = c_air,
                ro_air = ro_air,
                Ts = Ts,
                Tf = Tf,
                V = V,
            };
        }

        #endregion

        #region 熱流

        /// <summary>
        /// 熱伝導による熱移動の計算 (フーリエの法則)
        /// </summary>
        public virtual IVariable<double> Fourier(double dx, double rambda, double S, IVariable<double> T1, IVariable<double> T2, double dt  = 1.0)
        {
            return new Functions.Fourier
            {
                dx = dx,
                Rambda = rambda,
                S = S,
                T1 = T1,
                T2 = T2,
                dt = dt
            };
        }

        /// <summary>
        /// 貫流熱の計算(定常熱計算)
        /// </summary>
        /// <returns>面積Sの貫流熱量[]</returns>
        public virtual IVariable<double> HeatTransmission(IVariable<double> T1, IVariable<double> T2, double K, double S, double dt = 1.0)
        {
            return new Variable<double>(t => K * S * (T1.Get(t) - T2.Get(t)) * dt);
        }

        /// <summary>
        /// 対流による熱伝達の計算 (ニュートンの冷却則)
        /// </summary>
        public virtual IVariable<double> NewtonCooling(double S, IVariable<double> Ts, IVariable<double> Tf, IVariable<double> alpha_c)
        {
            return new Functions.NewtonCooling
            {
                alpha_c = alpha_c,
                S = S,
                Ts = Ts,
                Tf = Tf
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
        public virtual IVariable<double> DirectSolarRadiation(int tickTime, int beginDay, int days, IVariable<double> solarRadiation, IVariable<ISolarPositionData> solarPosition)
        {
            return new Functions.DirectSolarRadiation(tickTime, beginDay, days, solarRadiation, solarPosition);
        }

        /// <summary>
        /// 太陽位置
        /// </summary>
        public virtual IVariable<ISolarPositionData> SolarPosition(double Lat, double L, int tickTime, int beginDay, int days)
        {
            return new Functions.SolarPosition(Lat, L, tickTime, beginDay, days);
        }

        /// <summary>
        /// 入射角の方向余弦
        /// </summary>
        public virtual IVariable<double> IncidentAngleCosine(double tiltAngle, double azimuthAngle, IVariable<ISolarPositionData> solarPosition)
        {
            return new Functions.IncidentAngleCosine(tiltAngle, azimuthAngle, solarPosition);
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
        public virtual IVariable<double> TiltDiffusedSolarRadiation(double shapeFactorToSky, double groundReturnRate, IVariable<ISolarPositionData> solarPositionSource, IVariable<double> ID, IVariable<double> Id)
        {
            return new Functions.TiltDiffusedSolarRadiation
            {
                ShapeFactorToSky = shapeFactorToSky,
                GroundReturnRate = groundReturnRate,
                SolarPosition = solarPositionSource,
                DirectSolarRadiation = ID,
                DiffusedSolarRadiation = Id
            };
        }

        /// <summary>
        /// 透過日射の計算
        /// </summary>
        public virtual IVariable<double> ThroughSolar(double area, double solarThroughRate, IVariable<double> directionCosine,
            IVariable<ISolarPositionData> solarPositionSource,IVariable<double> ID, IVariable<double> Id)
        {
            return new Functions.WindowThroughSolar(area, solarThroughRate, directionCosine, solarPositionSource, ID, Id);
        }

        /// <summary>
        /// 透過日射の分配
        /// </summary>
        public virtual IVariable<double> SplitQGT(IVariable<double> QGT, double area, double solarTransmissionDistributionRate)
        {
            return new Functions.AbsorptionSolarRadiationSplitter(QGT, area, solarTransmissionDistributionRate);
        }

        #endregion
    }
}
