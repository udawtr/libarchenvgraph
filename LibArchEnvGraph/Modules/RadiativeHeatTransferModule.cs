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
    /// 入力:
    /// - 灰色体1,2の放射率: e1,e2 [-]
    /// - 灰色体1の面から灰色体2の面への形態係数 F12 [-]
    /// - 灰色体1,2の表面温度 TempOut [K]
    /// 
    /// 出力:
    /// - 灰色体1,2の熱移動量 HeatOut [W]
    /// </summary>
    /// <seealso cref="Functions.StefanBolzmann"/>
    public class RadiativeHeatTransferModule : BaseModule
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
        /// 灰色体1,2の表面温度 [K]
        /// </summary>
        public IVariable<double>[] TempIn { get; set; } = new IVariable<double>[2];

        /// <summary>
        /// 灰色体1,2の熱移動量 [J/s]
        /// </summary>
        public IVariable<double>[] HeatOut { get; private set; } = new[] {
            new LinkVariable<double>("灰色体1,2の熱移動量 [J/s]"),
            new LinkVariable<double>("灰色体1,2の熱移動量 [J/s]")
        };

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
