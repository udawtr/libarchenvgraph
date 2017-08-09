﻿using LibArchEnvGraph.Functions;
using LibArchEnvGraph.Modules;
using System;
using System.Collections.Generic;
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

        public FunctionFactory FunctionFactory { get; set; } = FunctionFactory.Default;

        /// <summary>
        /// 外気温 [度]
        /// </summary>
        public IVariable<double> OutsideTemperature { get; private set; }

        /// <summary>
        /// 日射量 [W]
        /// </summary>
        public IVariable<double> SolarRadiation { get; private set; }

        #region 計算グラフ作成オプション

        /// <summary>
        /// 相当温度を使用する場合はtrueを指定します。
        /// </summary>
        /// <remarks>
        /// 壁体への入射熱取得への計算に相当温度を用います。
        /// </remarks>
        public bool UseSAT { get; set; } = false;

        /// <summary>
        /// 相互放射を解く場合はtrue
        /// </summary>
        public bool UseMutualRadiation { get; set; } = false;

        /// <summary>
        /// 壁体を定常状態で解く場合はtrue
        /// </summary>
        public bool UseWallSteady { get; set; } = true;

        #endregion

        public void Run()
        {
            var F = FunctionFactory;

            var G = Build();

            G.Init(F);

            int t = BeginDay * (24 * 60 * 60 / TickSecond);
            for (int i = 0; i < TotalDays; i++)
            {
                for (int h = 0; h < 24; h++)
                {
                    for (int m = 0; m < 60; m++)
                    {
                        for (int s = 0; s < 60 * 60; s++, t++)
                        {
                            G.Commit(t);

                            Console.WriteLine($"{OutsideTemperature.Get(t)}\t{SolarRadiation.Get(t)}\t{House.OuterSurfaces[0].Temperature.Get(t)}\t{House.Rooms[0].Walls[0].Temperature.Get(t)}\t{House.Rooms[0].RoomTemperature.Get(t)}");
                        }
                    }
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
                cro = room.cro,
                V = room.V,
                dt = TickSecond,
                Label = room.Name
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
                var K = 1.0 / (1.0 / wall.a1 + 1.0 / wall.a2 + wall.r);

                return new SteadyWallModule()
                {
                    Label = $"{wall.Name}",

                    //表面積 [m2]
                    S = wall.S,

                    //熱貫流率 [W/m2K]
                    K = K,

                    //表面熱伝達率 [W/m2K]
                    a_o = wall.a1,
                    a_i = wall.a2,
                };
            }
            else
            {
                return new UnsteadyWallModule()
                {
                    Label = $"{wall.Name}",

                    //比熱
                    cro = wall.cro,

                    //奥行 [m]
                    depth = wall.depth,

                    //表面積 [m2]
                    S = wall.S,

                    //熱伝導率 [W/mK]
                    Rambda = wall.Rambda,

                    //計算間隔 [s]
                    dt = TickSecond,

                    //層分割数
                    n_slice = 5,

                    //自然対流作用の程度
                    c = new[]
                    {
                    NaturalConvectiveHeatTransferRate.cValueVerticalWallSurface,
                    NaturalConvectiveHeatTransferRate.cValueVerticalWallSurface,
                }
                };
            }
        }

        protected virtual ICalculationGraph Build()
        {
            var F = FunctionFactory;
            var container = new ContainerModule();

            //日射量データ [W/m2]
            //データ元: 気象庁　http://www.data.jma.go.jp/
            var radlist = CSVSource<SolarRadiationCSVRow>.ToList(SolarRadiationFilename, Encoding.GetEncoding(932));
            var solarRadiation = F.Interpolate(radlist.Select(x => x.SolarRadiation * 1000.0 / 3.6).ToArray(), 60 * 60);
            solarRadiation.Label = "日射量[W/m2]";

            //外気データ(大阪市)
            //データ元: 気象庁　http://www.data.jma.go.jp/
            var templist = CSVSource<OutsideTemperatureCSVRow>.ToList(OutsideTemperatureFilename, Encoding.GetEncoding(932));
            var outsideTemperature = F.Interpolate(templist.Select(x => x.Temp + 273.15).ToArray(), 60 * 60);
            outsideTemperature.Label = "外気温[K]";

            var _walls = House.Walls.Select(x => GetCalcuationGraph(x)).ToArray();
            container.Modules.AddRange(_walls);

            var wallDic = new Dictionary<Wall, IWallModule>();
            for (int i = 0; i < House.Walls.Count; i++)
            {
                wallDic[House.Walls[i]] = _walls[i];
            }


            //太陽位置
            var sol_pos = F.SolarPosition(Lat, L, TickSecond, BeginDay, TotalDays);

            foreach (var room in House.Rooms)
            {
                var roomModule = GetCalculationGraph(room);
                var wallModuleAndSurfaceNo = room.Walls.Select(x => new { Module = wallDic[x.Wall], x.SurfaceNo }).ToArray();
                var Nw = wallModuleAndSurfaceNo.Count();

                #region 透過日射

                var QGT = new List<IVariable<double>>();
                for (int i = 0; i < Nw; i++)
                {
                    //透過日射
                    if (room.Walls[i].Wall.IsOpen)
                    {
                        var win = room.Walls[i].Wall;

                        var sol_tr = new SolarTransmissionModule(
                            area: win.S,  //2.0m2
                            tiltAngle: win.TiltAngle,
                            azimuthAngle: win.AzimuthAngle,
                            groundReturnRate: win.GroundReturnRate,
                            solarThroughRate: win.SolarThroughRate,
                            solarPosition: sol_pos,
                            solarRadiation: solarRadiation,
                            tickTime: TickSecond,
                            beginDay: BeginDay,
                            days: TotalDays
                        );

                        QGT.Add(sol_tr.HeatOut);

                        container.Modules.Add(sol_tr);
                    }
                }

                //透過日射の合計 [W]
                var sumQGT = F.Multiply(TickSecond, F.Concat(QGT));

                for (int i = 0; i < Nw; i++)
                {
                    if (room.Walls[i].Wall.IsOpen == false)
                    {
                        var wall = room.Walls[i].Wall;

                        //TODO: まじめの分配率の計算をする
                        var h = F.SplitQGT(sumQGT, wall.S, 0.25);

                        var s = wallModuleAndSurfaceNo[i].SurfaceNo - 1;
                        wallModuleAndSurfaceNo[i].Module.HeatIn[s].Add(h);
                    }
                }

                #endregion

                #region  相互放射
                if (UseMutualRadiation &&  Nw >= 2)
                {
                    var MR = new MutualRadiationModule(Nw);

                    for (int i = 0; i < Nw; i++)
                    {
                        var wallModule = wallModuleAndSurfaceNo[i].Module;

                        var s = wallModuleAndSurfaceNo[i].SurfaceNo - 1;
                        MR.TempIn[i] = wallModule.TempOut[s];
                        wallModule.HeatIn[s].Add(F.Multiply(TickSecond, MR.HeatOut[i]));
                    }

                    container.Modules.Add(MR);
                }
                #endregion

                #region 室内の対流熱移動
                for (int i = 0; i < Nw; i++)
                {
                    var s = wallModuleAndSurfaceNo[i].SurfaceNo - 1;
                    var wallModule = wallModuleAndSurfaceNo[i].Module;

                    //部屋と壁を相互接続
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

                //SATを使う場合は、外気温の替わりに相当外気温を設定する
                if (UseSAT)
                {
                    #region 相当外気温度(SAT)

                    var cos = F.IncidentAngleCosine(wall.TiltAngle, wall.AzimuthAngle, sol_pos);
                    var ID = F.DirectSolarRadiation(TickSecond, BeginDay, TotalDays, solarRadiation, sol_pos);
                    var Id = F.Subtract(solarRadiation, ID);

                    //直達日射(傾斜)
                    var J_dt = F.TiltDirectSolarRadiation(cos, ID);

                    //天空日射(傾斜)
                    var J_st = F.TiltDiffusedSolarRadiation(1, wall.GroundReturnRate, sol_pos, ID, Id);

                    //反射日射
                    var J_h = F.Add(ID, Id);
                    var J_rt = F.Function(t => (1.0 - (1.0 + cos.Get(t)) / 2) * 0.25 * J_h.Get(t));

                    //日射(傾斜)
                    var J_t = F.Concat(J_dt, J_st, J_rt);

                    //実効放射
                    var J_e = F.Brunt(wall.TiltAngle * Math.PI / 180, outsideTemperature, F.Variable(4.28), F.Variable(0.8), F.Variable(1));
                    var SAT = F.SAT(To: outsideTemperature, J: J_t, J_e: J_e);

                    SAT.Label = $"{wallModule.Label} - 外気相当温度[K]";

                    #endregion

                    wallModule.TempIn[s] = SAT;
                }
                else
                {
                    //TODO: 外壁への入射量を壁体モジュールへ入れる
                    wallModule.TempIn[s] = outsideTemperature;
                }

                //バインド
                var Ts = F.KelvinToCelsius(wallModule.TempOut[s]);
                Ts.Label = $"{wallModule.Label} - 表面温度{s + 1}[C]";
                House.OuterSurfaces[i].Temperature = Ts;
            }
            #endregion

            this.OutsideTemperature = F.KelvinToCelsius(outsideTemperature);
            this.SolarRadiation = solarRadiation;

            return container;
        }
    }
}
