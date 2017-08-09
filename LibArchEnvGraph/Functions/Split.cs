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
    public class Split : BaseVariable<double>
    {
        /// <summary>
        /// 透過日射熱 [W]
        /// </summary>
        public IVariable<double> HeatIn { get; set; }

        /// <summary>
        /// 表面積 [m2]
        /// </summary>
        public  double S { get; set; }

        /// <summary>
        /// 透過日射分配率 [-]
        /// </summary>
        public double fsol { get; set; }


        public override double Update(int n)
        {
            //透過日射熱 [W]
            double QGT_in = HeatIn.Get(n);

            //i室の部位kの表面積 [m2]
            double A_ik = S;

            //吸収日射量[W/m2]
            double Sol_ikn = (fsol * QGT_in) / A_ik;

            return Sol_ikn;
        }
    }
}
