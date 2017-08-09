using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph
{
    /// <summary>
    /// 室モジュールインタフェイス
    /// </summary>
    public interface IRoomModule : ICalculationGraph
    {
        /// <summary>
        /// 温度 [K]
        /// </summary>
        IVariable<double> TempOut { get; }

        /// <summary>
        /// 熱流 [W]
        /// </summary>
        IList<IVariable<double>> HeatIn { get; }
    }
}
