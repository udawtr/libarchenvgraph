using LibArchEnvGraph;
using LibArchEnvGraph.Modules;
using LibArchEnvGraph.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Youworks.Text;
using System.IO;

namespace SimulatorSample
{

    /// <summary>
    /// 
    /// 標準実装のシミュレータクラスを使った計算例
    /// 
    /// ------------------------------------------------------------------------
    /// 
    ///    上面図:
    /// 
    ///                 N
    /// 
    ///     +-----------------------+ -
    ///     |                       | |
    ///     |                       | |
    ///     |                       | |
    ///     |                       | |
    ///     |                       | |
    ///     |                       | | 6.00m
    ///     |                       | |
    /// W   |                       | |   E
    ///     |                       | |
    ///     |                       | |
    ///     |                       | |
    ///     |                       | |
    ///     |                       | |
    ///     |                       | |
    ///     |                       | |
    ///     +--------========-------+ -   
    /// 
    ///     |-----------------------|
    ///               4.00m
    ///                 S
    /// 
    ///   S側面図:
    ///   
    /// 
    /// 
    ///     +-----------------------+  
    ///     |        +------+       |  -
    ///     |        |      |       |  |
    ///     |        |      |       |  | 2.00 m
    ///     |        |      |       |  |
    ///     |        |      |       |  |
    ///     |        +------+       |  -
    ///     +-----------------------+  
    ///
    ///              |------|
    ///               1.40m
    ///               
    /// 
    ///   E側面図:
    /// 
    ///     +----------------------------------+  -
    ///     |                                  |  |
    ///     |                                  |  |
    ///     |                                  |  | 2.40 m
    ///     |                                  |  |
    ///     |                                  |  |
    ///     |                                  |  |
    ///     +----------------------------------+  -
    ///              
    ///     |----------------------------------|
    ///                     6.00m
    /// 
    /// 
    /// 設定値：
    ///     外壁 = 0.05m厚 せっこうボード(0.213[W/mK], 比熱=854[kJ/m3K])
    ///     地面反射日射率 = 0.2
    ///     窓日射透過率 = 0.9
    ///     (室外側熱伝達率 = 23 [W/m2K])
    ///     (室内側熱伝達率 = 9 [W/m2K])
    /// 
    /// </summary>
    public static class SimpleSimulator
    {
        public static void Run()
        {
            //住宅モデル作成
            var house = new House
            {
                //室
                Rooms = new List<Room>
                {
                    new Room{
                        Name = "R1",
                        Cro = HeatCapacityModule.croAir, V = 4.0 * 6.0 * 2.4,
                        Walls =new List<WallSurface> {
                            new WallSurface { Name = "WallN", SurfaceNo = 2 },
                            new WallSurface { Name = "WallE", SurfaceNo = 2 },
                            new WallSurface { Name = "WallW", SurfaceNo = 2 },
                            new WallSurface { Name = "WallS", SurfaceNo = 2 },
                            new WallSurface { Name = "WinS",  SurfaceNo = 2 },
                        }
                    }
                },

                //壁
                Walls = new List<Wall>
                {
                    new Wall { Name = "WallN", Lambda = 0.213, Cro = 854,  Depth = 0.05, S= 4.0 * 2.4, TiltAngle = 90, AzimuthAngle = -90},
                    new Wall { Name = "WallE", Lambda = 0.213, Cro = 854,  Depth = 0.05, S= 6.0 * 2.4, TiltAngle = 90, AzimuthAngle = 180},
                    new Wall { Name = "WallW", Lambda = 0.213, Cro = 854,  Depth = 0.05, S= 6.0 * 2.4, TiltAngle = 00, AzimuthAngle =  90},
                    new Wall { Name = "WallS", Lambda = 0.213, Cro = 854,  Depth = 0.05, S= 4.0 * 2.4 - 1.4 * 2.0, TiltAngle = 90, AzimuthAngle = 00},
                    new Wall { Name = "WinS",  Lambda = 0.776, Cro = 2022, Depth = 0.05, S= 1.4 * 2.0, TiltAngle = 90, AzimuthAngle =  0, GroundReturnRate = 0.2, SolarThroughRate = 0.9, IsOpen = true},
                },

                //外皮
                OuterSurfaces = new List<WallSurface>
                {
                    new WallSurface { Name = "WallN", SurfaceNo = 1 },
                    new WallSurface { Name = "WallE", SurfaceNo = 1 },
                    new WallSurface { Name = "WallS", SurfaceNo = 1 },
                    new WallSurface { Name = "WinS",  SurfaceNo = 1 },
                }
            };

            using (var sw = new StreamWriter("log.csv"))
            {
                //シミュレータ作成
                var sim = new Simulator
                {
                    //計算範囲・間隔
                    TickSecond = 60,
                    BeginDay = 0,
                    TotalDays = 3,

                    //読込データ
                    OutsideTemperatureFilename = "s47772_201.csv",
                    SolarRadiationFilename = "s47772_610.csv",

                    //緯度経度
                    L = 135.52,
                    Lat = 34.68639,

                    //計算モデル指定
                    House = house,

                    //書き込み先指定
                    Out = sw,

                    //ログ書式指定
                    Log = (writer, h, t) =>
                    {
                        var To = h.OutsideTemperature.Get(t);
                        var Sol = h.SolarRadiation.Get(t);
                        var Tso = h.OuterSurfaces[0].Temperature.Get(t);
                        var Tsi = h.Rooms[0].Walls[0].Temperature.Get(t);
                        var Tr = h.Rooms[0].RoomTemperature.Get(t);

                        writer.WriteLine($"{t},{To},{Sol},{Tso},{Tsi},{Tr}");
                    }
                };

                //計算実行
                sim.Run();
            }
        }
    }
}
