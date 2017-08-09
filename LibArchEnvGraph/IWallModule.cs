using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Modules
{
    /// <summary>
    /// 一次元壁体モジュールインタフェイス
    /// </summary>
    public interface IWallModule : ICalculationGraph
    {
        /// <summary>
        /// 表面温度出力 [K]
        /// </summary>
        IVariable<double>[] TempOut { get; }

        /// <summary>
        /// 入力日射熱量 [W]
        /// </summary>
        IList<IVariable<double>>[] HeatIn { get; set; }

        /// <summary>
        /// 入力流体温度 [K]
        /// </summary>
        IVariable<double>[] TempIn { get; set; }

        /// <summary>
        /// 出力対流熱移動量 [W]
        /// </summary>
        IVariable<double>[] HeatOut { get; }
    }
}
