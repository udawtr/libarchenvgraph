using LibArchEnvGraph.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Modules
{
    /// <summary>
    /// 熱容量モジュール
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

        public double dt { get; set; } = 1.0;

        /// <summary>
        /// 温度 [K]
        /// </summary>
        public IVariable<double> TempOut { get; private set; } = new LinkVariable<double>();

        /// <summary>
        /// 熱流 [W]
        /// </summary>
        public List<IVariable<double>> HeatIn { get; private set; } = new List<IVariable<double>>();

        private IGateVariable<double> _memT;

        public void Init(FunctionFactory F)
        {
            //熱流を合算し、dtを掛ける
            var _sumQ = F.Multiply(dt, F.Concat(HeatIn));

            //温度差分 [K]
            var _dT = F.Temperature(cro, V, _sumQ);

            //記録している温度に温度差分を加える
            var _memTin = new LinkVariable<double>();
            var _add = F.Add(_memTin, _dT);

            //温度を記憶する
            _memT = F.Memory(_add);
            _memTin.Link = _memT;   //加算入力と温度出力を連結

            //記録した温度をTempOutに接続する
            (TempOut as LinkVariable<double>).Link = _memT;
        }

        public void SetTemperature(double initialTemp)
        {
            _memT.Set(initialTemp);
        }

        public void Commit(int t)
        {
            _memT.Commit(t);
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
