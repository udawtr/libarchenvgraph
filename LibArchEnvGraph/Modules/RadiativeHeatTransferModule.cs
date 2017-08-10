using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Modules
{
    /// <summary>
    /// 放射熱伝達モジュール
    /// 
    ///             +---------------+
    ///             |               |
    ///  TempIn1 -->+               +--> HeatOut1
    ///             |  放射熱伝達M  |
    ///  TempIn2 -->+               +--> HeatOut2
    ///             |               |
    ///             +-------+-------+
    ///                     |
    ///             e1,e2 --+
    ///               F12 --+
    ///
    /// 入力:
    /// - 灰色体1,2の放射率: e1,e2 [-]
    /// - 灰色体1の面から灰色体2の面への形態係数 F12 [-]
    /// - 灰色体1,2の表面温度 TempOut [K]
    /// 
    /// 出力:
    /// - 灰色体1,2の熱移動量 HeatOut [W]
    /// </summary>
    /// <remarks>
    /// 
    ///            +-----------+
    ///            |           |
    /// TempIn1 -->+ F.Stefan  |
    ///            | Bolzmann  +--+------------------> HeatOut2
    /// TempIn2 -->+           |  |
    ///            |           |  |   +----------+
    ///            +-----------+  |   |          |
    ///                           +---+ F.Invert +---> HeatOut1
    ///                               |          |
    ///                               +----------+  
    ///                               
    /// </remarks>
    /// <seealso cref="Functions.StefanBolzmann"/>
    public class RadiativeHeatTransferModule : HeatTransferModule
    {
        /// <summary>
        /// 灰色体1の放射率 [-]
        /// </summary>
        public double e1 { get; set; } = 0.9;

        /// <summary>
        /// 灰色体2の放射率 [-]
        /// </summary>
        public double e2 { get; set; } = 0.9;

        /// <summary>
        /// 灰色体1の面から灰色体2の面への形態係数 [-]
        /// </summary>
        public double F12 { get; set; } = 0.0;

        /// <summary>
        /// 初期化
        /// </summary>
        public override void Init(FunctionFactory F)
        {
            var baseFunction = F.StefanBolzmann(F12, TempIn[0], TempIn[1], e1, e2);

            (HeatOut[0] as LinkVariable<double>).Link = F.Invert(baseFunction);
            (HeatOut[1] as LinkVariable<double>).Link = baseFunction;
        }
    }
}
