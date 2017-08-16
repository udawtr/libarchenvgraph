using LibArchEnvGraph.Functions;
using LibArchEnvGraph.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Youworks.Text;

namespace LibArchEnvGraph
{
    /// <summary>
    /// 差分法
    /// </summary>
    public class Simulator
    {
        /// <summary>
        /// 計算間隔 [s]
        /// </summary>
        public int TickSecond { get; set; } = 1;

        /// <summary>
        /// 計算開始日
        /// </summary>
        public int BeginDay = 0;   //1月1日

        /// <summary>
        /// 計算日数
        /// </summary>
        public int TotalDays = 1;       //1日間

        /// <summary>
        /// 日射データCSVファイル名
        /// </summary>
        public string SolarRadiationFilename { get; set; }

        /// <summary>
        /// 外気温度データCSVファイル名
        /// </summary>
        public string OutsideTemperatureFilename { get; set; }

        /// <summary>
        /// 経度
        /// </summary>
        public double L { get; set; }

        /// <summary>
        /// 緯度
        /// </summary>
        public double Lat { get; set; }


        public House House { get; set; }

        public TextWriter Out { get; set; } = Console.Out;

        public Action<TextWriter, House,int> Log { get; set; }

        public FunctionFactory FunctionFactory { get; set; } = FunctionFactory.Default;

        #region 計算グラフ作成オプション

        /// <summary>
        /// 相当温度を使用する場合はtrueを指定します。
        /// </summary>
        /// <remarks>
        /// 壁体への入射熱取得への計算に相当温度を用います。
        /// </remarks>
        public bool UseSAT { get; set; } = false;


        /// <summary>
        /// 壁体を定常状態で解く場合はtrue
        /// </summary>
        public bool UseWallSteady { get; set; } = true;

        #endregion

        /// <summary>
        /// 
        /// </summary>
        public void Run()
        {
            var F = FunctionFactory;


            //参照の解決
            House.ResolveReference();

            var G = Build();

            G.Init(F);

            //モジュール階層構造確認
            GraphAnalyzer.TraceNestedModule(G as Modules.ContainerModule);

            //相互参照確認
            var allModules = GraphAnalyzer.GetAllModules(G as ContainerModule);
            foreach (var module in allModules)
            {
                GraphAnalyzer.CheckVariableLoop(module);
            }

            int t = BeginDay * (24 * 60 * 60 / TickSecond);
            int n = TotalDays * (24 * 60 * 60 / TickSecond);
            for (int i = 0; i < n; i++, t++)
            {
                G.Commit(i);

                if (Log != null)
                {
                    Log(Out, House, t);
                }
            }
        }


        /// <summary>
        /// 計算グラフの取得
        /// </summary>
        protected virtual HeatCapacityModule GetCalculationGraph(Room room)
        {
            var roomModule = new HeatCapacityModule()
            {
                Label = $"熱容量M({room.Name})",

                Cro = room.Cro,
                V = room.V,
                TickSecond = TickSecond,
            };

            return roomModule;
        }

        /// <summary>
        /// 一次元壁体のモジュールを取得
        /// </summary>
        protected virtual IWallModule GetCalcuationGraph(Wall wall)
        {
            if (UseWallSteady)
            {
                //熱貫流率 [W/m2K]
                var K = 1.0 / (1.0 / wall.A1 + 1.0 / wall.A2 + wall.Depth / wall.Lambda);

                return new SteadyWallModule()
                {
                    Label = $"定常1次元壁体M({wall.Name})",

                    //表面積 [m2]
                    S = wall.S,

                    //熱貫流率 [W/m2K]
                    K = K,

                    //表面熱伝達率 [W/m2K]
                    a_o = wall.A1,
                    a_i = wall.A2,
                };
            }
            else
            {
                return new UnsteadyWallModule()
                {
                    Label = $"非定常1次元壁体M({wall.Name})",

                    //比熱
                    Cro = wall.Cro,

                    //奥行 [m]
                    Depth = wall.Depth,

                    //表面積 [m2]
                    S = wall.S,

                    //熱伝導率 [W/mK]
                    Lambda = wall.Lambda,

                    //計算間隔 [s]
                    TickSecond = TickSecond,

                    //層分割数
                    SliceCount = 5,

                    //対流熱伝達
                    a = new[]
                    {
                        wall.A1,
                        wall.A2
                    }
                };
            }
        }

        protected virtual ICalculationGraph Build()
        {
            var F = FunctionFactory;
            var container = new ContainerModule
            {
                Label = "全体"
            };

            //日射量データ [W/m2]
            //データ元: 気象庁　http://www.data.jma.go.jp/
            var radlist = CSVSource<SolarRadiationCSVRow>.ToList(SolarRadiationFilename, Encoding.GetEncoding(932));
            var solarRadiation = F.Interpolate(radlist.Select(x => x.SolarRadiation * 1000.0 / 3.6).ToArray(), 60 * 60 / TickSecond);
            solarRadiation.Label = "日射量[W/m2]";

            //外気データ(大阪市)
            //データ元: 気象庁　http://www.data.jma.go.jp/
            var templist = CSVSource<OutsideTemperatureCSVRow>.ToList(OutsideTemperatureFilename, Encoding.GetEncoding(932));
            var outsideTemperature = F.Interpolate(templist.Select(x => x.Temp + 273.15).ToArray(), 60 * 60 / TickSecond);
            outsideTemperature.Label = "外気温[K]";

            var _walls = House.Walls.Select(x => GetCalcuationGraph(x)).ToArray();
            container.Modules.AddRange(_walls);

            var wallDic = new Dictionary<Wall, IWallModule>();
            for (int i = 0; i < House.Walls.Count; i++)
            {
                wallDic[House.Walls[i]] = _walls[i];
            }


            //暦
            var calM = new CalendarModule
            {
                BeginDay= BeginDay,
                TotalDays = TotalDays,
                TickSecond = TickSecond
            };
            container.Modules.Add(calM);

            //太陽位置
            var solPosM = new SolarPositionModule
            {
                //緯度経度
                Lat = Lat,
                L = L,

                //暦
                DayOfYearIn = calM.DayOfYearOut,
                HourIn = calM.HourOut,
                MinuteIn = calM.MinuteOut,
                SecondIn = calM.SecondOut,
            };
            container.Modules.Add(solPosM);

            foreach (var room in House.Rooms)
            {
                var roomModule = GetCalculationGraph(room);
                var wallModuleAndSurfaceNo = room.Walls.Select(x => new { Module = wallDic[x.Wall], x.SurfaceNo }).ToArray();
                var Nw = wallModuleAndSurfaceNo.Count();

                #region 透過日射

                var QGT = new List<IVariable<double>>();
                for (int i = 0; i < Nw; i++)
                {
                    if (room.Walls[i].Wall.IsOpen)
                    {
                        var win = room.Walls[i].Wall;

                        //透過日射M
                        var solTranM = new SolarTransmissionModule
                        {
                            Label = $"透過日射M({win.Name})",

                            S = win.S,
                            TiltAngle = win.TiltAngle,
                            AzimuthAngle = win.AzimuthAngle,
                            GroundReturnRate = win.GroundReturnRate,
                            SolarThroughRate = win.SolarThroughRate,

                            SolHIn = solPosM.SolHOut,
                            SolAIn = solPosM.SolAOut,
                            SolIn = solarRadiation,
                            DayOfYearIn  =calM.DayOfYearOut
                        };

                        container.Modules.Add(solTranM);

                        QGT.Add(solTranM.HeatOut);
                    }
                }

                //透過日射の合計 [W]
                var sumQGT = F.Concat(QGT);

                #endregion

                var MR = new MutualRadiationModule(Nw);

                for (int i = 0; i < Nw; i++)
                {
                    var wall = room.Walls[i].Wall;
                    var wallModule = wallModuleAndSurfaceNo[i].Module;
                    var s = wallModuleAndSurfaceNo[i].SurfaceNo - 1;

                    if (room.Walls[i].Wall.IsOpen == false)
                    {
                        // 透過日射の分配
                        //                         +-------+
                        //                         |       |
                        //  (透過日射) -----> 熱in-+ 壁体M |  
                        //                         |       |
                        //                         +-------+
                        //

                        //TODO: まじめの分配率の計算をする

                        var h = F.Split(sumQGT, wall.S, 1.0 / Nw);
                        h.Label = $"分配日射({wall.Name})[W]";

                        //分配された日射を壁に入れる
                        wallModule.HeatIn[s].Add(h);
                    }

                    if (UseWallSteady == false)
                    {

                        //相互放射の接続
                        //  +--------+                     +-------+
                        //  |        +-温度in <--- 温度out-+       |
                        /// | 放射M  |                     | 壁体M |
                        //  |        +-放射out -----> 熱in-+       |
                        //  +--------+                     +-------+
                        MR.TempIn[i] = wallModule.TempOut[s];
                        wallModule.HeatIn[s].Add(MR.HeatOut[i]);
                    }
                }

                if (UseWallSteady == false)
                {
                    container.Modules.Add(MR);
                }

                #region 室内の対流熱移動
                for (int i = 0; i < Nw; i++)
                {
                    var s = wallModuleAndSurfaceNo[i].SurfaceNo - 1;
                    var wallModule = wallModuleAndSurfaceNo[i].Module;

                    //部屋と壁を相互接続
                    //  +--------+                     +-------+
                    //  |        +-温度out ---> 温度in-+       |
                    /// | 部屋M  |                     | 壁体M |
                    //  |        +-熱in <------- 熱out-+       |
                    //  +--------+                     +-------+
                    wallModule.TempIn[s] = roomModule.TempOut;
                    roomModule.HeatIn.Add(wallModule.HeatOut[s]);
                }
                #endregion

                //バインド
                for (int i = 0; i < Nw; i++)
                {
                    var s = wallModuleAndSurfaceNo[i].SurfaceNo - 1;

                    room.Walls[i].Temperature = F.KelvinToCelsius(wallModuleAndSurfaceNo[i].Module.TempOut[s]);
                }
                room.RoomTemperature = F.KelvinToCelsius(roomModule.TempOut);

                container.Modules.Add(roomModule);
            }

            #region 外壁
            for (int i = 0; i < House.OuterSurfaces.Count; i++)
            {
                var wall = House.OuterSurfaces[i].Wall;
                var wallModule = wallDic[House.OuterSurfaces[i].Wall];
                var s = House.OuterSurfaces[i].SurfaceNo - 1;

                var satM = new SolarAirTemperatureModule
                {
                    Label = $"相当外気温度M({wall.Name})",
                    TiltAngle = wall.TiltAngle,
                    AzimuthAngle = wall.AzimuthAngle,
                    GroundReturnRate = wall.GroundReturnRate,
                    SolHIn = solPosM.SolHOut,
                    SolAIn = solPosM.SolAOut,
                    SolIn = solarRadiation,
                    TempIn = outsideTemperature,
                    DayOfYearIn = calM.DayOfYearOut,
                };
                container.Modules.Add(satM);

                if (UseSAT)
                {
                    //SATを使う場合は、外気温の替わりに相当外気温を設定する
                    wallModule.TempIn[s] = satM.TempOut;
                }
                else
                {
                    //SATを使わずに、日射量と夜間放射に面積を掛けて入力させる
                    wallModule.HeatIn[s].Add(F.Function(t => (satM.SolTiltOut.Get(t) + satM.RadNightOut.Get(t)) * wall.S));
                    wallModule.TempIn[s] = outsideTemperature;
                }

                //バインド
                var Ts = F.KelvinToCelsius(wallModule.TempOut[s]);
                Ts.Label = $"{wallModule.Label} - 表面温度{s + 1}[C]";
                House.OuterSurfaces[i].Temperature = Ts;
            }
            #endregion

            this.House.OutsideTemperature = F.KelvinToCelsius(outsideTemperature);
            this.House.SolarRadiation = solarRadiation;

            return container;
        }
    }
}
