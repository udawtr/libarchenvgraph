using LibArchEnvGraph.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Modules
{
    /// <summary>
    /// 熱容量ブロック
    /// </summary>
    public class HeatCapacityModule : ICalculationGraph
    {
        /// <summary>
        /// 容積[m^3]
        /// </summary>
        public double V { get; set; }

        /// <summary>
        /// 容積比熱 cρ [kJ/m^3・K]
        /// </summary>
        public double cro { get; set; }

        /// <summary>
        /// 温度 [℃]
        /// </summary>
        public IVariable<double> TempOut { get; private set; } = new LinkVariable<double>();

        public List<IVariable<double>> HeatIn { get; private set; } = new List<IVariable<double>>();

        private IVariable<double> _temp;

        private IGateVariable<double> _heat;

        public void Init(FunctionFactory F)
        {
            _heat = F.HeatMemory(HeatIn);
            _temp = F.Temperature(cro, V, _heat);

            (TempOut as LinkVariable<double>).Link = _temp;
        }

        public void SetTemperature(double initialTemp)
        {
            _heat.Set(initialTemp * cro * 1000 * V);
        }

        public void Commit(int t)
        {
            _heat.Commit(t);
        }

        /// <summary>
        /// せっこうボード(JIS A 6901)の容量比熱 [kJ/m3K]
        /// </summary>
        public const double croGypsumBoard = 854.0;

        /// <summary>
        /// 空気の容量比熱 [kJ/m3K]
        /// </summary>
        public const double croAir = 1.024 * 1.007;
    }
}
