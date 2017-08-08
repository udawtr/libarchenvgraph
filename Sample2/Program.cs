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

            //シミュレータ
            var sim = new Simulator
            {
                TickSecond = 1,
                BeginDay = 0,
                TotalDays = 1,

                OutsideTemperatureFilename = "s47772_201.csv",
                SolarRadiationFilename = "s47772_610.csv",

                L = 135.52,
                Lat= 34.68639,

                House = new House
                {
                    Rooms = new List<Room> {
                        new Room{ cro = HeatCapacityModule.croAir, V = 10.0, }
                    },
                    Walls = new List<Wall>
                    {
                        new Wall { Rambda = 0.213, cro = 854, depth = 0.05, S= 2.0 },
                        new Wall { Rambda = 0.213, cro = 854, depth = 0.05, S= 6.0, IsCeiling = true },
                        new Wall { Rambda = 0.213, cro = 854, depth = 0.05, S= 5.0, IsFloor = true },
                        new Wall { Rambda = 0.213, cro = 854, depth = 0.05, S= 5.0 },
                        new Wall { Rambda = 0.776, cro = 2022, depth= 0.05, S= 2.0, IsOpen = true, AzimuthAngle = 20, GroundReturnRate = 0.2, SolarThroughRate = 0.9 }
                    }
                }
            };

            sim.House.Rooms[0].Walls = new List<WallSurface>
            {
                new WallSurface { Wall = sim.House.Walls[0], SurfaceNo = 2 },
                new WallSurface { Wall = sim.House.Walls[1], SurfaceNo = 2 },
                new WallSurface { Wall = sim.House.Walls[2], SurfaceNo = 2 },
                new WallSurface { Wall = sim.House.Walls[3], SurfaceNo = 2 },
                new WallSurface { Wall = sim.House.Walls[4], SurfaceNo = 2 },
            };

            sim.House.OuterSurfaces = new List<WallSurface>
            {
                new WallSurface { Wall = sim.House.Walls[0], SurfaceNo = 1 },
                new WallSurface { Wall = sim.House.Walls[1], SurfaceNo = 1 },
                new WallSurface { Wall = sim.House.Walls[2], SurfaceNo = 1 },
                new WallSurface { Wall = sim.House.Walls[3], SurfaceNo = 1 },
                new WallSurface { Wall = sim.House.Walls[4], SurfaceNo = 1 },
            };

            sim.Run();
        }
    }
}
