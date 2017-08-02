using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Modules
{
    /// <summary>
    /// 放射熱伝達ブロック
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
        public IVariable<double> T1 { get; set; }

        /// <summary>
        /// 灰色体2の表面温度 [K]
        /// </summary>

        public IVariable<double> T2 { get; set; }

        /// <summary>
        /// 灰色体1の熱移動量 [J/s]
        /// </summary>
        public IVariable<double> dU1 { get; private set; } = new LinkVariable<double>();


        /// <summary>
        /// 灰色体2の熱移動量 [J/s]
        /// </summary>
        public IVariable<double> dU2 { get; private set; } = new LinkVariable<double>();

        /// <summary>
        /// 初期化
        /// </summary>
        public override void Init(FunctionFactory F)
        {
            var baseFunction = F.StefanBolzmann(F12, T1, T2, e1, e2);

            (dU1 as LinkVariable<double>).Link = F.Invert(baseFunction);
            (dU2 as LinkVariable<double>).Link = baseFunction;
        }
    }
}
