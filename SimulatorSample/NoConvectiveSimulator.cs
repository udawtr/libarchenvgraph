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
    /// 壁体のモジュールを自前実装に入れ替える例
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
    ///     |                       | | 4.00m
    ///     |                       | |
    /// W   |                       | |   E
    ///     |                       | |
    ///     |                       | |
    ///     |                       | |
    ///     +-----------------------+ -   
    /// 
    ///     |-----------------------|
    ///               4.00m
    ///                 S
    /// 
    ///   S側面図 = E側面図:
    ///   
    /// 
    ///     +-----------------------+  -
    ///     |                       |  |
    ///     |                       |  |
    ///     |                       |  | 2.40 m
    ///     |                       |  |
    ///     |                       |  |
    ///     |                       |  |
    ///     +-----------------------+  -
    ///               
    /// 
    /// 設定値：
    ///     外壁 = 0.05m厚 せっこうボード(0.213[W/mK], 比熱=854[kJ/m3K])
    /// 
    /// </summary>
    public static class NoConvectiveSimulator
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
                        Cro = HeatCapacityModule.croAir, V = 4.0 * 4.0 * 2.4,
                        Walls =new List<WallSurface> {
                            new WallSurface { Name = "WallN", SurfaceNo = 2 },
                            new WallSurface { Name = "WallE", SurfaceNo = 2 },
                            new WallSurface { Name = "WallW", SurfaceNo = 2 },
                            new WallSurface { Name = "WallS", SurfaceNo = 2 },
                        }
                    }
                },

                //壁
                Walls = new List<Wall>
                {
                    new Wall { Name = "WallN", Lambda = 0.213, Cro = 854,  Depth = 0.05, S= 4.0 * 2.4, TiltAngle = 90, AzimuthAngle = -90},
                    new Wall { Name = "WallE", Lambda = 0.213, Cro = 854,  Depth = 0.05, S= 4.0 * 2.4, TiltAngle = 90, AzimuthAngle = 180},
                    new Wall { Name = "WallW", Lambda = 0.213, Cro = 854,  Depth = 0.05, S= 4.0 * 2.4, TiltAngle = 90, AzimuthAngle =  90 },
                    new Wall { Name = "WallS", Lambda = 0.213, Cro = 854,  Depth = 0.05, S= 4.0 * 2.4, TiltAngle = 90, AzimuthAngle = 00},
                },

                //外皮
                OuterSurfaces = new List<WallSurface>
                {
                    new WallSurface { Name = "WallN", SurfaceNo = 1 },
                    new WallSurface { Name = "WallE", SurfaceNo = 1 },
                    new WallSurface { Name = "WallW", SurfaceNo = 1 },
                    new WallSurface { Name = "WallS", SurfaceNo = 1 },
                }
            };

            using (var sw = new StreamWriter("log.csv"))
            {
                //シミュレータ作成
                var sim = new MySimulator
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

        class MySimulator : Simulator
        {
            protected override IWallModule GetCalcuationGraph(Wall wall)
            {
                return new MyWallModule
                {
                    Cro = wall.Cro,
                    Depth = wall.Depth,
                    TickSecond = this.TickSecond,
                    Lambda = wall.Lambda,
                    S = wall.S,
                };
            }
        }

        /// <summary>
        /// <c>UnsteadyWallModule</c>のコードをコピーして改造したクラス
        /// </summary>
        class MyWallModule : ContainerModule, IWallModule
        {
            /// <summary>
            /// 奥行 [m]
            /// </summary>
            public double Depth { get; set; }

            /// <summary>
            /// 容積比熱 cρ [kJ/m^3・K]
            /// </summary>
            public double Cro { get; set; }

            /// <summary>
            /// 表面積 [m2]
            /// </summary>
            public double S { get; set; }

            /// <summary>
            /// 熱伝導率 [W/mK]
            /// </summary>
            public double Lambda { get; set; }

            /// <summary>
            /// 単位時間 [s]
            /// </summary>
            public double TickSecond { get; set; }

            /// <summary>
            /// 入力流体温度 [K]
            /// </summary>

            public IVariable<double>[] TempIn { get; set; } = new IVariable<double>[2];

            /// <summary>
            /// 出力壁体温度 [K]
            /// </summary>
            public IVariable<double>[] TempOut { get; private set; } = new[]
            {
                new LinkVariable<double>("表面温度1 [K]"),
                new LinkVariable<double>("表面温度2 [K]")
            };

            /// <summary>
            /// 入力熱流 [W]
            /// </summary>
            public IList<IVariable<double>>[] HeatIn { get; set; } = new[]
            {
                new List<IVariable<double>>(),
                new List<IVariable<double>>()
            };

            /// <summary>
            /// 出力熱流 [W]
            /// </summary>
            public IVariable<double>[] HeatOut { get; private set; } = new[]
            {
                new LinkVariable<double>("表面熱流1 [J]"),
                new LinkVariable<double>("表面熱流2 [J]"),
            };

            public MyWallModule()
            {
                Label = "対流なし壁体M";

            }

            public override void Init(FunctionFactory F)
            {
                //層の厚さ [m]
                var dx = Depth / 2;

                //層壁体の作成
                var heatCapacityModuleList = new List<HeatCapacityModule>();
                for (int i = 0; i < 2; i++)
                {
                    heatCapacityModuleList.Add(new HeatCapacityModule
                    {
                        Cro = Cro,          //容積比熱
                        V = dx * S,         //容積
                        TickSecond = TickSecond,
                        Label = $"層壁体{i + 1}({Label})"
                    });
                }

                //層壁体間の熱伝導の作成
                var conductiveM = new ConductiveHeatTransferModule
                {
                    dx = dx,            //層の中心間の距離 [m]
                    Lambda = Lambda,    //熱伝導率
                    S = S,              //表面積[m2]
                    Label = $"層壁体1<=>2間の熱伝導 ({Label})"
                };

                heatCapacityModuleList[0].HeatIn.Add(conductiveM.HeatOut[0]);
                heatCapacityModuleList[1].HeatIn.Add(conductiveM.HeatOut[1]);

                conductiveM.TempIn[0] = heatCapacityModuleList[0].TempOut;
                conductiveM.TempIn[1] = heatCapacityModuleList[1].TempOut;

                //室外側・室内側層壁体への外部からの熱移動
                heatCapacityModuleList[0].HeatIn.Add(F.Concat(HeatIn[0]));
                heatCapacityModuleList[1].HeatIn.Add(F.Concat(HeatIn[1]));


                //出力変数の登録
                (TempOut[0] as LinkVariable<double>).Link = heatCapacityModuleList[0].TempOut;  //室外側表面温度
                (TempOut[1] as LinkVariable<double>).Link = heatCapacityModuleList[1].TempOut;    //室内側表面温度
                (HeatOut[0] as LinkVariable<double>).Link = F.Constant(0);
                (HeatOut[1] as LinkVariable<double>).Link = F.Constant(0);

                TempOut[0].Label = $"{Label} - 表面温度1[K]";
                TempOut[1].Label = $"{Label} - 表面温度2[K]";
                HeatOut[0].Label = $"{Label} - 表面熱流1[J]";
                HeatOut[1].Label = $"{Label} - 表面熱流2[J]";

                //内部モジュールの登録
                Modules.AddRange(heatCapacityModuleList);
                Modules.Add(conductiveM);

                //初期化
                base.Init(F);
            }
        }
    }
}
