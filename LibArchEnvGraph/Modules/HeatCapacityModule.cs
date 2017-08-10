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
    /// 
    ///               +---------+
    ///               |         |
    ///    HeatIn1 -->+         |
    ///               |         |
    ///    HeatIn2 -->+         |
    ///               | 熱容量M +--> TempOut
    ///      ...   -->+         |
    ///               |         |
    ///  HeatIn(n) -->+         |
    ///               |         |
    ///               +----+----+
    ///                    |
    ///                V --+
    ///              cro --+
    ///               dt --+
    ///
    /// 入力:
    /// - 容積 V [m3]
    /// - 容積比熱 cro [kJ/m3K]
    /// - 熱流 HeatIn [W]
    /// - 単位時間 dt [s]
    /// 
    /// 出力:
    /// - 温度 TempOut [K]
    /// </summary>
    public class HeatCapacityModule : BaseModule
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
        /// 単位時間 [s]
        /// </summary>
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

        public override void Init(FunctionFactory F)
        {
            //熱流を合算し、dtを掛ける
            var _sumQ = F.Multiply(dt, F.Concat(HeatIn));
            _sumQ.Label = $"入力熱量 [J] ({Label})";

            //温度差分 [K]
            var _dT = F.Temperature(cro, V, _sumQ);
            _dT.Label = $"温度差分 [K] ({Label})";

            //記録している温度に温度差分を加える
            var _memTin = new LinkVariable<double>();
            _memTin.Label = $"記録している温度[K]への参照 ({Label})";
            var _add = F.Add(_memTin, _dT);
            _add.Label = $"記録している温度[K]に温度差分[K]を加える ({Label})";

            //温度を記憶する
            _memT = F.Memory(_add);
            _memTin.Link = _memT;   //加算入力と温度出力を連結
            _memT.Label = $"記憶している温度[K] ({Label})";

            //記録した温度をTempOutに接続する
            (TempOut as LinkVariable<double>).Link = _memT;
            TempOut.Label = $"温度 [K] ({Label})";
        }

        public void SetTemperature(double initialTemp)
        {
            _memT.Set(initialTemp);
        }

        public override void Commit(int t)
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
