using LibArchEnvGraph;
using LibArchEnvGraph.Modules;
using LibArchEnvGraph.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleSample
{
    /// <summary>
    /// 
    ///  空気とせっこうボードの自然対流熱移動の計算
    /// 
    /// 
    ///  概要図:
    ///    
    ///             V = 10m3   V=0.006m3
    ///          * - - - - - **-*
    ///                       | |
    ///          |    -->    || |
    ///            /＼    |   | |
    ///  20 [C]  |  |     |  || | 40 [C]
    ///             |    ＼/  | |
    ///          |    <--    || |
    ///                       | |
    ///          * - - - - - *+-+
    ///             cro = 1    cro=854 
    ///  
    ///  モジュール接続図:
    ///  
    /// +--------------------------------------------------------------------------------+
    /// |             +-----------+                                                      |
    /// |             |           |                                                      |
    /// +---HeatIn -->+  熱容量M  +--> TempOut--+             +-----------+              |
    ///               |           |             |             |           |              |
    ///               +-----------+             +--TempIn1--->+           +--> HeatOut1--+
    ///                                                       | 自然換気M |            
    ///               +-----------+             +--TempIn2 -->+           +--> HeatOut2--+
    ///               |           |             |             |           |              |
    /// +---HeatIn -->+  熱容量M  +--> TempOut--+             +-----------+              |
    /// |             |           |                                                      | 
    /// |             +-----------+                                                      |
    /// +--------------------------------------------------------------------------------+
    /// </summary>
    public static class NaturalConvectiveHeatTransferSample
    {
        public static void Run()
        {
            var container = new ContainerModule();

            //壁 6m2*0.001m
            var wall = new HeatCapacityModule
            {
                V = 6.0 * 0.001,
                Cro = HeatCapacityModule.croGypsumBoard,
            };

            //空気
            var air = new HeatCapacityModule
            {
                V = 10,
                Cro = HeatCapacityModule.croAir,
            };

            //垂直壁 6m2 自然換気
            var nv = new NaturalConvectiveHeatTransferModule
            {
                cValue = NaturalConvectiveHeatTransferRate.cValueVerticalWallSurface,
                S = 6.0,
                TempIn = new IVariable<double>[] {
                    wall.TempOut,
                    air.TempOut
                }
            };
            wall.HeatIn.Add(nv.HeatOut[0]);
            air.HeatIn.Add(nv.HeatOut[1]);


            //初期化
            var F = new FunctionFactory();
            container.Modules.AddRange(new ICalculationGraph[] { wall, air, nv });
            container.Init(F);

            //温度設定
            wall.SetTemperature(40);
            air.SetTemperature(20);

            for (int t = 0; t < 1000; t++)
            {
                container.Commit(t);

                Console.WriteLine($"{wall.TempOut.Get(t)}, {air.TempOut.Get(t)}, {nv.HeatOut[0].Get(t)}, {nv.HeatOut[1].Get(t)}");
            }
        }
    }
}
