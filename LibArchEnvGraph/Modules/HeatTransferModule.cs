using LibArchEnvGraph.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Modules
{
    /// <summary>
    /// 熱移動モジュール(抽象クラス)
    /// 
    ///               +-----------+
    ///               |           |
    ///    HeatIn1 -->+           +--> HeatOut1
    ///               |  熱移動M  |
    ///    HeatIn2 -->+           +--> HeatOut2
    ///               |           |
    ///               +-----------+
    /// 
    /// 入力:
    /// - 表面温度 TempIn [K]
    /// 
    /// 出力:
    /// - 熱移動量 HeatOut [W]
    /// </summary>
    /// <seealso cref="ConductiveHeatTransferModule"/>
    /// <seealso cref="ConvectiveHeatTransferModule"/>
    /// <seealso cref="HeatTransferModule"/>
    /// <seealso cref="NaturalConvectiveHeatTransferModule"/>
    /// <seealso cref="VentilationHeatTransferModule"/>
    /// <seealso cref="OverallHeatTransmissionModule"/>
    public abstract class HeatTransferModule : BaseModule
    {
        /// <summary>
        /// 温度1,2 [K]
        /// </summary>
        public IVariable<double>[] TempIn { get; set; } = new IVariable<double>[2];

        /// <summary>
        /// 熱移動量1,2 [W]
        /// </summary>
        public IVariable<double>[] HeatOut { get; private set; } = new[]
        {
            new LinkVariable<double>("熱移動量1 [W]"),
            new LinkVariable<double>("熱移動量2 [W]")
        };
    }
}
