using LibArchEnvGraph.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Functions
{
    /// <summary>
    /// 透過日射熱 [W]
    /// 
    ///                       +----------+
    ///                       |          |
    ///   SolDirectTiltOut -->+          |
    ///                       |          |
    ///  SolDiffuseTiltOut -->+          +--> SolTran
    ///                       |          |
    ///            TiltCos -->+          |
    ///                       |          |
    ///                       +----------+
    ///             
    /// </summary>
    public class WindowThroughSolar : BaseVariable<double>
    {
        /// <summary>
        /// 透過面積 [m2]
        /// </summary>
        public double S { get; set; }

        /// <summary>
        /// 垂直入射時の日射透過率 [-]
        /// </summary>
        public double SolarThroughRate { get; set; }

        /// <summary>
        /// 入射角の方向余弦
        /// </summary>
        public IVariable<double> TiltCos { get; set; }


        public IVariable<double> SolDirectTilt { get; set; }

        public IVariable<double> SolDiffuseTile { get; set; }


        #region 透過日射熱取得量

        /// <summary>
        /// 透過日射熱取得量 [W]
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public override double Update(int n)
        {
            //日射透過率
            double tauN = SolarThroughRate;
            double tauD = GetDirectSolarTransmittanceRatio(n);
            double taud = GetDiffusionSolarTransmittaneRatio();

            //日射量 [W/m2]

            //傾斜面直達日射
            double ID = this.SolDirectTilt.Get(n);

            //傾斜面拡散日射
            double Id = this.SolDiffuseTile.Get(n);

            //傾斜面全天日射量
            double Iw = ID + Id;

            //窓の透過日射熱取得・吸収日射熱取得

            //傾斜面直達日射がない場合は計算対象外
            if (Iw > 0.0)
            {
                return GetQGT(tauD, ID, taud, Id);
            }

            return 0.0;
        }

        /// <summary>
        /// 窓の透過日射熱取得 P.17(55)
        /// </summary>
        /// <param name="room">部屋</param>
        /// <param name="tauD">i 室の部位 k における n 時点の直達日射に対する日射透過率</param>
        /// <param name="ID">i 室の部位 k における n 時点の傾斜面直達日射量</param>
        /// <param name="taud">i 室の部位 k における n 時点の拡散日射に対する日射透過率</param>
        /// <param name="Id">i 室の部位 k における n 時点の傾斜面拡散日射量</param>
        /// <returns></returns>
        private double GetQGT(double tauD, double ID, double taud, double Id)
        {
            //直達成分
            var QGTD = tauD * ID;

            //拡散成分
            var GDTS = taud * Id;

            return S * (QGTD + GDTS);
        }

        #endregion

        #region 吸収日射熱取得量

        /// <summary>
        /// 窓の吸収日射熱取得 P.17(56)
        /// </summary>
        /// <param name="room">部屋</param>
        /// <param name="Fsdw">i 室の部位 k における日影面積率</param>
        /// <param name="BD">i 室の部位 k における n 時点の直達日射に対する吸収日射透過率</param>
        /// <param name="ID">i 室の部位 k における n 時点の傾斜面直達日射量</param>
        /// <param name="Bd">i 室の部位 k における n 時点の拡散日射に対する吸収日射透過率</param>
        /// <param name="Id">i 室の部位 k における n 時点の傾斜面拡散日射量</param>
        /// <returns></returns>
        private double GetQGA(double Fsdw, double BD, double ID, double Bd, double Id)
        {
            //直達成分
            var QGAD = (1 - Fsdw) * BD * ID;

            //拡散成分
            var GDAS = Bd * Id;

            return S * (QGAD + GDAS);
        }

        #endregion

        #region 日射透過率

        /// <summary>
        /// i 室の部位 k における n 時点の直達日射に対する日射透過率の取得 P.14(51)
        /// </summary>
        /// <param name="tau_Nik">i 室の部位 k における垂直入射時の日射透過率</param>
        /// <param name="cos_ikn">i 室の部位 k における n 時点の入射角の方向余弦</param>
        /// <returns>i 室の部位 k における n 時点の直達日射に対する日射透過率 τ_Dikn</returns>
        private double GetDirectSolarTransmittanceRatio(int n)
        {
            double tau_Nik = SolarThroughRate;
            double cos = TiltCos.Get(n);

            var cos_pow2 = cos * cos;
            var cos_pow3 = cos_pow2 * cos;
            var cos_pow5 = cos_pow3 * cos_pow2;
            var cos_pow7 = cos_pow5 * cos_pow2;

            return tau_Nik * (2.392 * cos - 3.8636 * cos_pow3 + 3.7568 * cos_pow5 - 1.3965 * cos_pow7) / 0.88;
        }

        /// <summary>
        /// i 室の部位 k における n 時点の拡散日射に対する日射透過率の取得 P.14(52)
        /// </summary>
        /// <param name="tau_Nik">i 室の部位 k における垂直入射時の日射透過率</param>
        /// <returns>i 室の部位 k における n 時点の拡散日射に対する日射透過率 τ_dikn</returns>
        private double GetDiffusionSolarTransmittaneRatio()
        {
            const double Cd = 0.92;
            double tauN = SolarThroughRate;

            return Cd * tauN;
        }

        #endregion
    }
}
