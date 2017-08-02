using LibArchEnvGraph.Functions;
using LibArchEnvGraph.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph
{
    public class House
    {
        public int tickTime = 60 * 60; //1時間単位
        public int beginDay = 0;   //1月1日
        public int days = 1;       //1日間

        public string SolarRadiationFilename { get; set; }

        public string OutsideTemperatureFilename { get; set; } = "s47772_201.csv";


        public double L { get; set; } = 135.52;

        public double Lat { get; set; } = 34.68639;

        public List<Room> Rooms { get; set; }

        public List<Wall> Walls { get; set; }

        public List<WallSurface> OuterSurfaces { get; set; }

        public ICalculationGraph GetCalcuationGraph(IVariable<double> solarRadiation, IVariable<double> outsideTemperature, FunctionFactory F)
        {
            var container = new ContainerModule();
            var _walls = Walls.Select(x => x.GetCalcuationGraph() as SerialHeatConductionModule).ToArray();
            container.Modules.AddRange(_walls);

            var wallDic = new Dictionary<Wall, SerialHeatConductionModule>();
            for (int i = 0; i < Walls.Count; i++)
            {
                wallDic[Walls[i]] = _walls[i];
            }


            //TODO: 窓を実装します。
            /*
            

            //太陽位置
            var sol_pos = F.SolarPosition(Lat, L, tickTime, beginDay, days);

            //透過日射
            var sol_tr = new SolarTransmissionModule(
                area: 2.0,  //2.0m2
                tiltAngle: 0,
                azimuthAngle: 60.0,
                groundReturnRate: 0.1,
                solarThroughRate: 0.7,
                solarPosition: sol_pos,
                solarRadiation: solarRadiation,
                tickTime: tickTime,
                beginDay: beginDay,
                days: days
            );
            */

            foreach (var room in Rooms)
            {
                var _room = room.GetCalcuationGraph() as HeatCapacityModule;
                var __walls = room.Walls.Select(x => new { Module = wallDic[x.Wall], x.SurfaceNo }).ToArray();

                #region  相互放射
                var Nw = __walls.Count();
                if (Nw >= 2)
                {
                    for (int i = 0; i < Nw - 1; i++)
                    {
                        for (int j = i + 1; j < Nw; j++)
                        {
                            var R = new RadiationHeatTransferModule
                            {
                                //TODO: 放射の収支を無視した値になっているので真面目の解く
                                F12 = 1.0 / (Nw - 1)
                            };

                            if (__walls[i].SurfaceNo == 1)
                            {
                                __walls[i].Module.HeatIn1.Add(R.dU1);
                            }
                            else
                            {
                                __walls[i].Module.HeatIn2.Add(R.dU1);
                            }

                            if (__walls[j].SurfaceNo == 1)
                            {
                                __walls[j].Module.HeatIn1.Add(R.dU2);
                            }
                            else
                            {
                                __walls[j].Module.HeatIn2.Add(R.dU2);
                            }
                        }
                    }
                }
                #endregion

                #region 室内の対流熱移動
                for (int i = 0; i < Nw; i++)
                {
                    var cv = new NaturalConvectiveHeatTransferModule
                    {
                        cValue = NaturalConvectiveHeatTransferRate.cValueVerticalWallSurface,
                        S = __walls[i].Module.S,
                        TfIn = _room.TempOut,
                        TsIn = __walls[i].Module.TempOut1,
                    };


                    if (__walls[i].SurfaceNo == 1)
                    {
                        __walls[i].Module.HeatIn1.Add(cv.dUsOut);
                    }
                    else
                    {
                        __walls[i].Module.HeatIn2.Add(cv.dUsOut);
                    }

                    _room.HeatIn.Add(cv.dUfOut);
                }
                #endregion


                //バインド
                for (int i = 0; i < Nw; i++)
                {
                    if (__walls[i].SurfaceNo == 1)
                    {
                        room.Walls[i].Temperature = __walls[i].Module.TempOut1;
                    }
                    else
                    {
                        room.Walls[i].Temperature = __walls[i].Module.TempOut2;
                    }
                }
                room.RoomTemperature = _room.TempOut;

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
                    TfIn = outsideTemperature
                };

                if (OuterSurfaces[i].SurfaceNo == 1)
                {
                    nv.TsIn = _wall.TempOut1;
                    _wall.HeatIn1.Add(nv.dUsOut);
                }
                else
                {
                    nv.TsIn = _wall.TempOut2;
                    _wall.HeatIn2.Add(nv.dUsOut);
                }

                //バインド
                OuterSurfaces[i].Temperature = nv.TsIn;
            }
            #endregion

            return container;
        }
    }
}
