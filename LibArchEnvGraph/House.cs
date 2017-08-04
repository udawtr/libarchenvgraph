using LibArchEnvGraph.Functions;
using LibArchEnvGraph.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph
{
    /// <summary>
    /// 住宅を表すクラス
    /// </summary>
    public class House
    {
        public int tickTime = 60 * 60; //1時間単位
        public int beginDay = 0;   //1月1日
        public int days = 1;       //1日間

        public string SolarRadiationFilename { get; set; }

        public string OutsideTemperatureFilename { get; set; } = "s47772_201.csv";


        public double L { get; set; } = 135.52;

        public double Lat { get; set; } = 34.68639;

        /// <summary>
        /// 室一覧
        /// </summary>
        public List<Room> Rooms { get; set; } = new List<Room>();

        /// <summary>
        /// 壁体一覧
        /// </summary>
        public List<Wall> Walls { get; set; } = new List<Wall>();

        /// <summary>
        /// 外壁面一覧
        /// </summary>
        public List<WallSurface> OuterSurfaces { get; set; }

        public ICalculationGraph GetCalcuationGraph(IVariable<double> solarRadiation, IVariable<double> outsideTemperature, FunctionFactory F)
        {
            var container = new ContainerModule();

            var _walls = Walls.Select(x => x.GetCalcuationGraph(tickTime) as SerialHeatConductionModule).ToArray();
            container.Modules.AddRange(_walls);

            var wallDic = new Dictionary<Wall, SerialHeatConductionModule>();
            for (int i = 0; i < Walls.Count; i++)
            {
                wallDic[Walls[i]] = _walls[i];
            }


            //太陽位置
            var sol_pos = F.SolarPosition(Lat, L, tickTime, beginDay, days);

            foreach (var room in Rooms)
            {
                var _room = room.GetCalcuationGraph(tickTime) as HeatCapacityModule;
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
                            tickTime: tickTime,
                            beginDay: beginDay,
                            days: days
                        );

                        QGT.Add(sol_tr.HeatOut);

                        container.Modules.Add(sol_tr);
                    }
                }

                //透過日射の合計 [W]
                var sumQGT = F.Multiply(tickTime, F.Concat(QGT));
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
                            __wall.HeatIn1.Add(F.Multiply(tickTime, MR.HeatOut[i]));
                        }
                        else
                        {
                            MR.TempIn[i] = __wall.TempOut2;
                            __wall.HeatIn2.Add(F.Multiply(tickTime, MR.HeatOut[i]));
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
                        __walls[i].Module.HeatIn1.Add(F.Multiply(tickTime, cv.HeatflowSolidOut));
                    }
                    else
                    {
                        cv.TempSolidIn = __walls[i].Module.TempOut2;
                        __walls[i].Module.HeatIn2.Add(F.Multiply(tickTime, cv.HeatflowSolidOut));
                    }

                    _room.HeatFlowIn.Add(cv.HeatflowFluidOut);

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
            for (int i = 0; i < OuterSurfaces.Count; i++)
            {
                var _wall = wallDic[OuterSurfaces[i].Wall];

                var nv = new NaturalConvectiveHeatTransferModule
                {
                    cValue = NaturalConvectiveHeatTransferRate.cValueVerticalWallSurface,
                    S = _wall.S,
                    TempFluidIn = outsideTemperature
                };

                IVariable<double> T;
                if (OuterSurfaces[i].SurfaceNo == 1)
                {
                    T = _wall.TempOut1;
                    _wall.HeatIn1.Add(nv.HeatflowSolidOut);
                }
                else
                {
                    T = _wall.TempOut2;
                    _wall.HeatIn2.Add(nv.HeatflowSolidOut);
                }
                nv.TempSolidIn = T;

                //バインド
                OuterSurfaces[i].Temperature = F.KelvinToCelsius(T);

                container.Modules.Add(nv);
            }
            #endregion

            return container;
        }
    }
}
