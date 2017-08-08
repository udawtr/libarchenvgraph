using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Functions
{
    /// <summary>
    /// ブラントの式
    /// ref.  NOTE ON BRUNT'S FORMULTA FOR NOCTURNAL RADIATION OF THE ATMOSPHERE
    ///       http://adsabs.harvard.edu/full/1934ApJ....79..441P
    /// </summary>
    public class Brunt : IVariable<double>
    {
        /// <summary>
        /// 地表付近の空気の絶対温度 [K]
        /// </summary>
        public IVariable<double> Ta { get; set; }

        /// <summary>
        /// 地表付近の空気の水蒸気分圧 [mmHg]
        /// </summary>
        public IVariable<double> f { get; set; }

        /// <summary>
        /// 雲高によって決まる修正定数(上層雲時: 0.8, 中層雲時: 0.3, 下層雲時: 0.15)
        /// </summary>
        public IVariable<double> k { get; set; }

        /// <summary>
        /// 雲量(快晴:0 ～ 全天雲:10)
        /// </summary>
        public IVariable<double> c { get; set; }

        /// <summary>
        /// 大気放射到達面の傾斜角 [rad] (鉛直=Math.PI/2, 水平=0)
        /// </summary>
        public double theta { get; set; } = Math.PI / 2;

        /// <summary>
        /// 実効放射量[W/m2]の取得
        /// </summary>
        /// <returns>実効放射量[W/m2]</returns>
        public double Get(int t)
        {
            //実効放射の計算
            var Jeh = 5.67 * Math.Pow(Ta.Get(t) / 100, 4) * (0.474 - 0.076 * Math.Sqrt(f.Get(t)));

            //雲高,雲量による補正
            var Jehc = Jeh * (1.0 - (1.0 - k.Get(t)) * c.Get(t) / 10);

            //傾斜面への入射量の計算(鉛直面=半分,水平面=全部)
            var phi = (1.0 + Math.Cos(theta)) / 2.0;
            var Jet = phi * Jehc;

            return Jet;
        }
    }
}
