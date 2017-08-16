using LibArchEnvGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionSample
{
    public static class EnthalpyClass
    {
        public static void Run()
        {
            Console.WriteLine("----------------------------------");
            Console.WriteLine("Enthalpy using class");
            Console.WriteLine("----------------------------------");

            var F = new FunctionFactory();

            //エンタルピー
            var h = new Enthalpy
            {
                //湿り空気温度 [C]
                theta = F.Constant(20),

                //湿り空気の絶対湿度 [kg/kg']
                x = F.Constant(0.001, 0.005, 0.01, 0.05)
            };

            for (int t= 0; t < 4; t++)
            {
                Console.WriteLine($"theta:{h.theta.Get(t)}, x:{h.x.Get(t)} => {h.Get(t)} [kcal/kg']");
            }

            Console.WriteLine("");
        }

        public class Enthalpy : BaseVariable<double>
        {
            /// <summary>
            /// 湿り空気温度 [C]
            /// </summary>
            public IVariable<double> theta { get; set; }

            /// <summary>
            /// 湿り空気の絶対湿度 [kg/kg']
            /// </summary>
            public IVariable<double> x { get; set; }

            public override double Update(int t)
            {
                return 0.24 * theta.Get(t) + x.Get(t) * (587.5 + 0.441 * theta.Get(t));
            }
        }
    }
}
