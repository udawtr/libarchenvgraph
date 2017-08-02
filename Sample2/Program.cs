using LibArchEnvGraph;
using LibArchEnvGraph.Modules;
using LibArchEnvGraph.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Youworks.Text;

namespace Sample2
{

    class Program
    {
        static void Main(string[] args)
        {
            //
            // /#\
            // #*#  \  ************
            // \#\ \ \ *          *
            //    \ \ \|          *
            //     \ \ |\         *
            //      \  * \        *
            //         ************
            //
            //       開口部: 2.0m2
            //       垂直壁1(開口部あり): 4.0m2
            //       垂直壁2(開口部なし): 6.0m2
            //       床: 5m2
            //       天井: 5m2

            var F = new FunctionFactory();

            var tickTime = 60 * 60; //1時間単位
            var beginDay = 0;   //1月1日
            var days = 1;       //1日間

            var house = new House
            {
                Rooms = new List<Room> {
                    new Room{ cro = HeatCapacityModule.croAir, V = 10.0}
                },
                Walls = new List<Wall>
                {
                    new Wall { cro = HeatCapacityModule.croGypsumBoard, depth = 0.01, S= 4.0 },
                    new Wall { cro = HeatCapacityModule.croGypsumBoard, depth = 0.01, S= 6.0, IsCeiling = true },
                    new Wall { cro = HeatCapacityModule.croGypsumBoard, depth = 0.01, S= 5.0, IsFloor = true },
                    new Wall { cro = HeatCapacityModule.croGypsumBoard, depth = 0.01, S= 5.0 },
                }
            };

            house.Rooms[0].Walls = new List<WallSurface>
            {
                new WallSurface { Wall = house.Walls[0], SurfaceNo = 2 },
                new WallSurface { Wall = house.Walls[1], SurfaceNo = 2 },
                new WallSurface { Wall = house.Walls[2], SurfaceNo = 2 },
                new WallSurface { Wall = house.Walls[3], SurfaceNo = 2 },
            };

            house.OuterSurfaces = new List<WallSurface>
            {
                new WallSurface { Wall = house.Walls[0], SurfaceNo = 1 },
                new WallSurface { Wall = house.Walls[1], SurfaceNo = 1 },
                new WallSurface { Wall = house.Walls[2], SurfaceNo = 1 },
                new WallSurface { Wall = house.Walls[3], SurfaceNo = 1 },
            };

            //日射量データ [W/m2]
            //データ元: 気象庁　http://www.data.jma.go.jp/
            var radlist = CSVSource<SolarRadiationCSVRow>.ToList("s47772_610.csv", Encoding.GetEncoding(932));
            var var_rad = new DataVariable<double>(radlist.Select(x => x.SolarRadiation * 1000.0 / 3.6).ToArray());

            //外気データ(大阪市)
            //データ元: 気象庁　http://www.data.jma.go.jp/
            var templist = CSVSource<OutsideTemperatureCSVRow>.ToList("s47772_201.csv", Encoding.GetEncoding(932));
            var var_temp = new DataVariable<double>(templist.Select(x => x.Temp).ToArray());

            var G = house.GetCalcuationGraph(var_rad, var_temp, F);

            //TODO:夜間放射, 地面との熱伝導, 窓の対流熱・熱伝導・放射

            G.Init(F);

            for (int i = 0; i < 24; i++)
            {
                G.Commit(i);

                Console.WriteLine($"{i}\t{house.Rooms[0].RoomTemperature.Get(i)}");
            }
        }
    }
}
