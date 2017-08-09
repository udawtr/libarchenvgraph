using LibArchEnvGraph.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Modules
{
    /// <summary>
    /// 定常1次元壁体モジュール
    /// 
    ///               +-------------+
    ///               |             |
    ///    HeatIn1 -->+             +--> HeatOut1
    ///               |  非定常一次 |
    ///    HeatIn2 -->+  元壁体M    +--> HeatOut2
    ///               |             |
    ///    TempIn1 -->+             +--> TempOut1
    ///               |             |
    ///    TempIn2 -->+             +--> TempOut2
    ///               |             |
    ///               +-------------+
    /// 
    /// 入力:
    /// - 表面積 S [m2]
    /// - 室外側熱伝達率 a_o [W/m2K]
    /// - 室内側熱伝達率 a_i [W/m2K]
    /// - 熱貫流率 K [W/m2K]
    /// - 室外側／室内側入力温度 TempIn [K]
    /// - 室外側／室内側入力熱量 HeatIn [W]
    /// 
    /// 出力:
    /// - 室外側／室内側出力温度 TempOut [K]
    /// - 出力対流熱移動量 HeatOut [W]
    /// </summary>
    /// <remarks>
    /// 
    ///              +------------+
    ///              |            |
    /// TempIn1  --->+            |    
    ///              | F.Function +--+
    /// HeatIn1  --->+            |  |   +-----------+
    ///              |            |  |   |           |
    ///              +------------+  +-->+ F.Overall |
    ///                                  | HeatTrans +--+-------------------> HeatOut1    
    ///              +------------+  +-->+ mission   |  |
    ///              |            |  |   |           |  |   +----------+
    /// TempIn2  --->+            |  |   +-----------+  |   |          |
    ///              | F.Function +--+                  +---+ F.Invert +----> HeatOut2
    /// HeatIn2  --->+            |                         |          |
    ///              |            |                         +----------+
    ///              +------------+   
    ///              
    /// </remarks>
    public class SteadyWallModule : BaseModule, IWallModule
    {
        /// <summary>
        /// 表面積 [m2]
        /// </summary>
        public double S { get; set; }

        /// <summary>
        /// 室外側熱伝達率 [W/m2K]
        /// </summary>
        public double a_o { get; set; } = 23;

        /// <summary>
        /// 室内側熱伝達率 [W/m2K]
        /// </summary>
        public double a_i { get; set; } = 9;

        /// <summary>
        /// 熱貫流率 [W/m2K]
        /// </summary>
        public double K { get; set; }

        /// <summary>
        /// 室外側／室内側出力温度 [K]
        /// </summary>
        public IVariable<double>[] TempOut { get; private set; } = new[]
        {
            new LinkVariable<double>(),
            new LinkVariable<double>()
        };

        /// <summary>
        /// 室外側／室内側入力温度 [K]
        /// </summary>
        public IVariable<double>[] TempIn { get; set; } = new IVariable<double>[2];

        /// <summary>
        /// 室外側／室内側入力熱量 [W]
        /// </summary>
        public IList<IVariable<double>>[] HeatIn { get; set; } = new[]
        {
            new List<IVariable<double>>(),
            new List<IVariable<double>>()
        };

        /// <summary>
        /// 出力対流熱移動量 [W]
        /// </summary>
        public IVariable<double>[] HeatOut { get; private set; } = new[]
        {
            new LinkVariable<double>(),
            new LinkVariable<double>()
        };

        public SteadyWallModule()
        {
        }

        public override void Init(FunctionFactory F)
        {
            if (!(S > 0.0)) throw new InvalidOperationException($"表面積を設定してから初期化してください。");
            if (!(K > 0.0)) throw new InvalidOperationException($"熱貫流率を設定してから初期化してください。");
            if (!(a_i > 0.0 && a_o > 0.0)) throw new InvalidOperationException($"表面熱伝達率を設定してから初期化してください。");

            var SATo = F.Function(t => TempIn[0].Get(t) + F.Concat(HeatIn[0]).Get(t) / a_o);
            var SATi = F.Function(t => TempIn[1].Get(t) + F.Concat(HeatIn[1]).Get(t) / a_i);

            //定常状態であるから
            // K * (SATo - SATi) = a_o * (SATo - Tso)
            // 故に、 (SATo - SATi) * K / a_o = SATo - Tso
            //        Tso = SATo - (SATo - SATi) * K / a_o
            //            = SATo * (1.0 - K / a_o) + SATi * (K / a_o);
            var Tso = F.Function(t => SATo.Get(t) * (1.0 - K / a_o) + SATi.Get(t) * (K / a_o));

            //定常状態であるから
            // K * (SATo - SATi) = a_i * (Tsi - SATi)
            // 故に、(SATo - SATi) * K / a_i = Tsi - SATi
            //       Tsi = (SATo - SATi) * K / a_i + SATi
            //           = SATo * K / a_i + SATi * (1.0 - K / a_i)
            var Tsi = F.Function(t => SATo.Get(t) * K / a_i + SATi.Get(t) * (1.0 - K / a_i));

            var Q = F.OverallHeatTransmission(SATo, SATi, K, S);

            (TempOut[0] as LinkVariable<double>).Link = Tso;
            (TempOut[1] as LinkVariable<double>).Link = Tsi;
            (HeatOut[0] as LinkVariable<double>).Link = F.Invert(Q);
            (HeatOut[1] as LinkVariable<double>).Link = Q;
        }
    }
}
