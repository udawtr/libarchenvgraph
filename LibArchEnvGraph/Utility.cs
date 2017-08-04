using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph
{
    public static class Utility
    {
        /// <summary>
        /// 総合熱貫流率
        /// </summary>
        /// <param name="K">熱貫流率 [W/m2K]</param>
        /// <param name="S">面積[m2]</param>
        /// <param name="nV">換気量またはすきま風量 [m3/s]</param>
        /// <returns>総合熱貫流率 [W/m2K]</returns>
        public static double GetTotalHeatTransmissionRate(double[] K, double[] S, double nV)
        {
            System.Diagnostics.Debug.Assert(K.Length == S.Length);

            var tmp = 0.0;
            for (int i = 0; i < K.Length; i++)
            {
                tmp += K[i] * S[i];
            }

            return tmp + 1206.0 * nV;
        }
        /// <summary>
        /// 特殊断面の平均熱貫流率
        /// </summary>
        /// <param name="K">熱貫流率 [W/m2K]</param>
        /// <param name="S">面積[m2]</param>
        /// <returns>特殊断面の平均熱貫流率 [W/m2K]</returns>
        public static double GetAverageHeatTransmissionRate(double[] K, double[] S)
        {
            System.Diagnostics.Debug.Assert(K.Length == S.Length);

            var tmp = 0.0;
            for (int i = 0; i < K.Length; i++)
            {
                tmp += K[i] * S[i];
            }

            var K_bar = tmp / S.Sum();

            return K_bar;
        }


        /// <summary>
        /// 総合透過率 [-]
        /// </summary>
        /// <param name="tei">透過率 [-]</param>
        /// <param name="ro">吸収率 [-]</param>
        /// <returns>総合透過率 [-]</returns>
        public static double GetTotalTransmittance(double tei1, double tei2, double ro1, double ro2)
        {
            return tei1 * tei2 / (1.0 - ro1 * ro2);
        }

        /// <summary>
        /// 総合透過率 [-]
        /// </summary>
        /// <param name="tei">透過率 [-]</param>
        /// <param name="ro">吸収率 [-]</param>
        /// <returns>総合透過率 [-]</returns>
        public static double GetTotalTransmittance(double tei1, double tei2, double tei3, double ro1, double ro2, double ro3)
        {
            return tei1 * tei2 * tei3 / ((1.0 - ro2 * ro3) * (1.0 - ro1 * ro2) - tei2 * tei2 * ro1 * ro3);
        }

        /// <summary>
        /// 総合吸収率(二重半透明体)
        /// </summary>
        public static double[] GetTotalAbsorptivity(double a1, double a2, double tei1, double tei2, double ro1, double ro2)
        {
            return new double[]
            {
                a1 * (1.0 + tei1 * ro2 / (1.0 - ro1 * ro2)),
                tei1 * a2 / (1.0 - ro1 * ro2)
            };
        }


        /// <summary>
        /// 総合吸収率(三重半透明体)
        /// </summary>
        public static double[] GetTotalAbsorptivity(double a1, double a2, double a3, double tei1, double tei2, double tei3, double ro1, double ro2, double ro3)
        {
            return new double[]
            {
                a1 * (1.0 + (tei1 * ro2 + tei1* tei2* tei2 * ro3 - tei1 * ro2*ro2*ro3)/ ((1.0-ro2*ro3)*(1.0-ro1*ro2)-tei2*tei2*ro1*ro3)),
                a2 * (tei1 - tei1* ro2*ro3 + tei1*tei2*ro3)/((1.0-ro2*ro3)*(1.0-ro1*ro2)-tei2*tei2*ro1*ro3),
                a3* (tei1*tei2/((1.0-ro2*ro3)*(1.0-ro1*ro2)-tei2*tei2*ro1*ro3))
            };
        }
    }
}
