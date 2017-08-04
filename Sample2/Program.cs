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

            var tickTime = 1;  //1分単位
            var beginDay = 0;   //1月1日
            var days = 1;       //1日間

            var house = new House
            {
                tickTime = tickTime,

                Rooms = new List<Room> {
                    new Room{ cro = HeatCapacityModule.croAir, V = 10.0, }
                },
                Walls = new List<Wall>
                {
                    new Wall { Rambda = 0.2, cro = 1000, depth = 0.05, S= 2.0 },
                    new Wall { Rambda = 0.1, cro = HeatCapacityModule.croGypsumBoard, depth = 0.01, S= 6.0, IsCeiling = true },
                    new Wall { Rambda = 0.1, cro = HeatCapacityModule.croGypsumBoard, depth = 0.01, S= 5.0, IsFloor = true },
                    new Wall { Rambda = 0.1, cro = HeatCapacityModule.croGypsumBoard, depth = 0.01, S= 5.0 },
                    new Wall { Rambda = 1, cro = 1, depth= 0.01, S= 2.0, IsOpen = true, AzimuthAngle = 20, GroundReturnRate = 0.2, SolarThroughRate = 0.9 }
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
            var var_rad = F.Interpolate(radlist.Select(x => x.SolarRadiation * 1000.0 / 3.6).ToArray(), 60);

            //外気データ(大阪市)
            //データ元: 気象庁　http://www.data.jma.go.jp/
            var templist = CSVSource<OutsideTemperatureCSVRow>.ToList("s47772_201.csv", Encoding.GetEncoding(932));
            var var_temp = F.Interpolate(templist.Select(x => x.Temp + 273.15).ToArray(), 60);
            //var var_temp = F.Function(x => 300);

            var G = house.GetCalcuationGraph(var_rad, var_temp, F);

            //TODO:夜間放射, 地面との熱伝導, 窓の対流熱・熱伝導・放射

            G.Init(F);

            int t = 0;
            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < 60; j++, t++)
                {
                    G.Commit(t);
                }

                Console.WriteLine($"{t}\t{var_temp.Get(t)-273.15}\t{house.OuterSurfaces[0].Temperature.Get(t)}");
            }
        }
    }
}
