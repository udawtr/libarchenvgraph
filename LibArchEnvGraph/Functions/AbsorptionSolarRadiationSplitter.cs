using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Functions
{
    /// <summary>
    /// 室内表面の吸収日射量を分配するクラス
    /// </summary>
    /// <remarks>
    /// 入射してしてきた日射を面積に応じて床や壁に分配します。
    /// 1. 床とそれ以外で日射を半分に分けます。
    /// 2. 床とそれ以外でそれぞれ面積に応じて配分します。
    /// ただし、床が存在しない場合は床以外ですべて配分します。
    /// </remarks>
    public class AbsorptionSolarRadiationSplitter : IVariable<double>
    {
        /// <summary>
        /// 透過日射熱 [W]
        /// </summary>
        private readonly IVariable<double> QGT;

        /// <summary>
        /// 表面積 [m2]
        /// </summary>
        private readonly double area;

        /// <summary>
        /// 透過日射分配率 [-]
        /// </summary>
        private readonly double fsol;

        /// <summary>
        /// 吸収日射量 [W/m2]
        /// </summary>
        private double[] Sol;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="QGT">透過日射熱 [W]</param>
        /// <param name="area">面積 [m2]</param>
        /// <param name="solarTransmissionDistributionRate">透過日射分配率 [-]</param>
        public AbsorptionSolarRadiationSplitter(
            IVariable<double> QGT,
            double area,
            double solarTransmissionDistributionRate
            )
        {
            this.QGT = QGT;
            this.area = area;
            this.fsol = solarTransmissionDistributionRate;
        }

        public void Init()
        {
            Sol = new double[24 * 365 * 4];

            for (int n = 0; n < 24 * 365 * 4; n++)
            {
                Sol[n] = GetAbsorbingSolarRadiationTo(n);
            }

            Inited_Sol = true;
        }

        public double Get(int index)
        {
            if (Inited_Sol == false) Init();

            return Sol[index];
        }

        private bool Inited_Sol = false;

        /// <summary>
        /// 吸収日射量の取得
        /// </summary>
        /// <returns>吸収日射量 [W/m2] Sol_ikn</returns>
        private double GetAbsorbingSolarRadiationTo(int n)
        {
            //透過日射熱 [W]
            double QGT_in = QGT.Get(n);

            //i室の部位kの表面積 [m2]
            double A_ik = area;

            //吸収日射量[W/m2]
            double Sol_ikn = (fsol * QGT_in) / A_ik;

            return Sol_ikn;
        }
    }
}
