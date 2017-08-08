using LibArchEnvGraph.Functions;
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
        public HeatCapacityModule GetCalculationGraph(Room room)
        {
            return new HeatCapacityModule()
            {
                cro = room.cro,
                V = room.V,
                dt = TickSecond,
            };
        }

        private SerialHeatConductionModule GetCalcuationGraph(Wall wall)
        {
            return new SerialHeatConductionModule()
            {
                cro = wall.cro,
                depth = wall.depth,
                S = wall.S,
                Rambda = wall.Rambda,
                dt = TickSecond,
                n_slice = 5,
            };
        }

        public ICalculationGraph Build()
        {
            var F = FunctionFactory;
            var container = new ContainerModule();

            //日射量データ [W/m2]
            //データ元: 気象庁　http://www.data.jma.go.jp/
            var radlist = CSVSource<SolarRadiationCSVRow>.ToList(SolarRadiationFilename, Encoding.GetEncoding(932));
            var solarRadiation = F.Interpolate(radlist.Select(x => x.SolarRadiation * 1000.0 / 3.6).ToArray(), 60 * 60);

            //外気データ(大阪市)
            //データ元: 気象庁　http://www.data.jma.go.jp/
            var templist = CSVSource<OutsideTemperatureCSVRow>.ToList(OutsideTemperatureFilename, Encoding.GetEncoding(932));
            var outsideTemperature = F.Interpolate(templist.Select(x => x.Temp + 273.15).ToArray(), 60 * 60);


            var _walls = House.Walls.Select(x => GetCalcuationGraph(x)).ToArray();
            container.Modules.AddRange(_walls);

            var wallDic = new Dictionary<Wall, SerialHeatConductionModule>();
            for (int i = 0; i < House.Walls.Count; i++)
            {
                wallDic[House.Walls[i]] = _walls[i];
            }


            //太陽位置
            var sol_pos = F.SolarPosition(Lat, L, TickSecond, BeginDay, TotalDays);

            foreach (var room in House.Rooms)
            {
                var _room = GetCalculationGraph(room);
                var __walls = room.Walls.Select(x => new { Module = wallDic[x.Wall], x.SurfaceNo }).ToArray();
                var Nw = __walls.Count();

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

                        if (__walls[i].SurfaceNo == 1)
                        {
                            __walls[i].Module.HeatIn1.Add(h);
                        }
                        else
                        {
                            __walls[i].Module.HeatIn2.Add(h);
                        }
                    }
                }

                #endregion


                #region  相互放射
                if (Nw >= 2)
                {
                    var MR = new MutualRadiationModule(Nw);

                    for (int i = 0; i < Nw; i++)
                    {
                        var __wall = __walls[i].Module;

                        if (__walls[i].SurfaceNo == 1)
                        {
                            MR.TempIn[i] = __wall.TempOut1;
                            __wall.HeatIn1.Add(F.Multiply(TickSecond, MR.HeatOut[i]));
                        }
                        else
                        {
                            MR.TempIn[i] = __wall.TempOut2;
                            __wall.HeatIn2.Add(F.Multiply(TickSecond, MR.HeatOut[i]));
                        }
                    }

                    container.Modules.Add(MR);
                }
                #endregion

                #region 室内の対流熱移動
                for (int i = 0; i < Nw; i++)
                {
                    var cv = new NaturalConvectiveHeatTransferModule
                    {
                        cValue = NaturalConvectiveHeatTransferRate.cValueVerticalWallSurface,
                        S = __walls[i].Module.S,
                        TempFluidIn = _room.TempOut,
                    };


                    if (__walls[i].SurfaceNo == 1)
                    {
                        cv.TempSolidIn = __walls[i].Module.TempOut1;
                        __walls[i].Module.HeatIn1.Add(F.Multiply(TickSecond, cv.HeatSolidOut));
                    }
                    else
                    {
                        cv.TempSolidIn = __walls[i].Module.TempOut2;
                        __walls[i].Module.HeatIn2.Add(F.Multiply(TickSecond, cv.HeatSolidOut));
                    }

                    _room.HeatIn.Add(cv.HeatFluidOut);

                    container.Modules.Add(cv);
                }
                #endregion

                //バインド
                for (int i = 0; i < Nw; i++)
                {
                    if (__walls[i].SurfaceNo == 1)
                    {
                        room.Walls[i].Temperature = F.KelvinToCelsius(__walls[i].Module.TempOut1);
                    }
                    else
                    {
                        room.Walls[i].Temperature = F.KelvinToCelsius(__walls[i].Module.TempOut2);
                    }
                }
                room.RoomTemperature = F.KelvinToCelsius(_room.TempOut);

                container.Modules.Add(_room);
            }

            #region 外気との自然対流熱移動
            for (int i = 0; i < House.OuterSurfaces.Count; i++)
            {
                var wall = House.OuterSurfaces[i].Wall;
                var _wall = wallDic[House.OuterSurfaces[i].Wall];

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

                #endregion


                var nv = new NaturalConvectiveHeatTransferModule
                {
                    cValue = NaturalConvectiveHeatTransferRate.cValueVerticalWallSurface,
                    S = _wall.S,
                    TempFluidIn = SAT
                };

                IVariable<double> T;
                if (House.OuterSurfaces[i].SurfaceNo == 1)
                {
                    T = _wall.TempOut1;
                    _wall.HeatIn1.Add(nv.HeatSolidOut);
                }
                else
                {
                    T = _wall.TempOut2;
                    _wall.HeatIn2.Add(nv.HeatSolidOut);
                }
                nv.TempSolidIn = T;

                //バインド
                House.OuterSurfaces[i].Temperature = F.KelvinToCelsius(T);

                container.Modules.Add(nv);
            }
            #endregion

            this.OutsideTemperature = F.KelvinToCelsius(outsideTemperature);
            this.SolarRadiation = solarRadiation;

            return container;
        }
    }
}
