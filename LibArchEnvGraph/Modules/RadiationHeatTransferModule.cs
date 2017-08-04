using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Modules
{
    /// <summary>
    /// 放射熱伝達モジュール
    /// </summary>
    /// <seealso cref="Functions.StefanBolzmann"/>
    public class RadiationHeatTransferModule : BaseModule
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
        /// 灰色体1の表面温度 [K]
        /// </summary>
        public IVariable<double> TempIn1 { get; set; }

        /// <summary>
        /// 灰色体2の表面温度 [K]
        /// </summary>

        public IVariable<double> TempIn2 { get; set; }

        /// <summary>
        /// 灰色体1の熱移動量 [J/s]
        /// </summary>
        public IVariable<double> HeatOut1 { get; private set; } = new LinkVariable<double>();


        /// <summary>
        /// 灰色体2の熱移動量 [J/s]
        /// </summary>
        public IVariable<double> HeatOut2 { get; private set; } = new LinkVariable<double>();

        /// <summary>
        /// 初期化
        /// </summary>
        public override void Init(FunctionFactory F)
        {
            var baseFunction = F.StefanBolzmann(F12, TempIn1, TempIn2, e1, e2);

            (HeatOut1 as LinkVariable<double>).Link = F.Invert(baseFunction);
            (HeatOut2 as LinkVariable<double>).Link = baseFunction;
        }
    }
}
